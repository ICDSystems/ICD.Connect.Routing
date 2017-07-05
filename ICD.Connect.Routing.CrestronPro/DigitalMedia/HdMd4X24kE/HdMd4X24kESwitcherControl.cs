using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DM;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Misc.CrestronPro.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd4X24kE
{
	public sealed class HdMd4X24kESwitcherControl : AbstractRouteSwitcherControl<HdMd4X24kEAdapter>
	{
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public override event EventHandler OnActiveInputsChanged;
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

		private readonly Dictionary<int, bool> m_CachedSourceStates;
		private readonly SafeCriticalSection m_CachedSourceSection;

		// Crestron is garbage at tracking the active routing states on the 4x2,
		// so lets just cache the assigned routes until the device tells us otherwise.
		private readonly Dictionary<int, int?> m_CachedRoutes;
		private readonly SafeCriticalSection m_CachedRoutesSection;

		private HdMd4x24kE m_Switcher;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		public HdMd4X24kESwitcherControl(HdMd4X24kEAdapter parent)
			: base(parent, 0)
		{
			m_CachedSourceStates = new Dictionary<int, bool>();
			m_CachedSourceSection = new SafeCriticalSection();

			m_CachedRoutes = new Dictionary<int, int?>();
			m_CachedRoutesSection = new SafeCriticalSection();

			Subscribe(parent);
			SetSwitcher(parent.Switcher);
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnSourceDetectionStateChange = null;
			OnRouteChange = null;
			OnActiveTransmissionStateChanged = null;
			OnActiveInputsChanged = null;

			base.DisposeFinal(disposing);

			// Unsubscribe and unregister.
			Unsubscribe(Parent);
			SetSwitcher(null);
		}

		#region Methods

		/// <summary>
		/// Routes the input to the given output.
		/// </summary>
		/// <param name="info"></param>
		public override bool Route(RouteOperation info)
		{
			eConnectionType type = info.ConnectionType;
			int input = info.LocalInput;
			int output = info.LocalOutput;

			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .Select(f => this.Route(input, output, f)).Unanimous(false);
			}

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
					Parent.Switcher.Outputs[(uint)output].VideoOut = Parent.Switcher.Inputs[(uint)input];
					m_CachedRoutesSection.Execute(() => m_CachedRoutes[output] = input);
					return true;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		public override bool ClearOutput(int output, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .Select(f => ClearOutput(output, f)).Unanimous(false);
			}

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
					Parent.Switcher.Outputs[(uint)output].VideoOut = null;
					m_CachedRoutesSection.Execute(() => m_CachedRoutes.Remove(output));
					return true;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// Returns true if video is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .Select(f => GetSignalDetectedState(input, f)).Unanimous(false);
			}

			switch (type)
			{
				case eConnectionType.Audio:
					return true;
				case eConnectionType.Video:
					return Parent.Switcher.Inputs[(uint)input].VideoDetectedFeedback.BoolValue;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return input > 0 && input <= 4;
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int input)
		{
			if (!ContainsInput(input))
				throw new KeyNotFoundException(string.Format("{0} has no input with address {1}", GetType().Name, input));
			return new ConnectorInfo(input, eConnectionType.Audio | eConnectionType.Video);
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			return Enumerable.Range(1, 4).Select(i => GetInput(i));
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			return Enumerable.Range(1, 2).Select(i => new ConnectorInfo(i, eConnectionType.Audio | eConnectionType.Video));
		}

		/// <summary>
		/// Gets the inputs for the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs(int output, eConnectionType type)
		{
			if (!type.HasFlag(eConnectionType.Audio) && !type.HasFlag(eConnectionType.Video))
				yield break;

			DMInput input = Parent.Switcher.HdmiOutputs[(uint)output].GetSafeVideoOutFeedback();

			// If we can't find the input from the device, check the cache.
			int? address = input == null
				? m_CachedRoutesSection.Execute(() => m_CachedRoutes.GetDefault(output, null))
				: (int)input.Number;

			if (address != null)
				yield return GetInput((int)address);
		}

		#endregion

		#region Parent Callbacks

		private void Subscribe(HdMd4X24kEAdapter parent)
		{
			parent.OnSwitcherChanged += ParentOnSwitcherChanged;
		}

		private void Unsubscribe(HdMd4X24kEAdapter parent)
		{
			parent.OnSwitcherChanged -= ParentOnSwitcherChanged;
		}

		private void ParentOnSwitcherChanged(HdMd4X24kEAdapter sender, HdMd4x24kE switcher)
		{
			SetSwitcher(switcher);
		}

		private void SetSwitcher(HdMd4x24kE switcher)
		{
			Unsubscribe(m_Switcher);
			m_Switcher = switcher;
			Subscribe(m_Switcher);
		}

		#endregion

		#region Switcher Callbacks

		/// <summary>
		/// Subscribe to the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		private void Subscribe(HdMd4x24kE switcher)
		{
			if (switcher == null)
				return;

			switcher.DMInputChange += SwitcherOnDmInputChange;
			switcher.DMOutputChange += SwitcherOnDmOutputChange;
		}

		/// <summary>
		/// Unsubscribe from the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		private void Unsubscribe(HdMd4x24kE switcher)
		{
			if (switcher == null)
				return;

			switcher.DMInputChange -= SwitcherOnDmInputChange;
			switcher.DMOutputChange -= SwitcherOnDmOutputChange;
		}

		/// <summary>
		/// Called when an input state changes.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void SwitcherOnDmInputChange(Switch device, DMInputEventArgs args)
		{
			if (args.EventId != DMInputEventIds.SourceSyncEventId)
				return;

			int input = (int)args.Number;
			bool state = GetSignalDetectedState(input, eConnectionType.Video);

			m_CachedSourceSection.Enter();

			try
			{
				bool cachedState = m_CachedSourceStates.GetDefault(input, false);
				if (state == cachedState)
					return;

				m_CachedSourceStates[input] = state;
			}
			finally
			{
				m_CachedSourceSection.Leave();
			}

			OnSourceDetectionStateChange.Raise(this,
											   new SourceDetectionStateChangeEventArgs(input,
																					   eConnectionType.Audio |
																					   eConnectionType.Video, state));
		}

		/// <summary>
		/// Called when an output state changes.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void SwitcherOnDmOutputChange(Switch device, DMOutputEventArgs args)
		{
			if (args.EventId != DMOutputEventIds.VideoOutEventId)
				return;

			int output = (int)args.Number;
			ConnectorInfo[] inputs = GetInputs(output, eConnectionType.Audio | eConnectionType.Video).ToArray();

			m_CachedRoutesSection.Enter();

			try
			{
				foreach (ConnectorInfo input in inputs)
					m_CachedRoutes[output] = input.Address;
			}
			finally
			{
				m_CachedRoutesSection.Leave();
			}

			// Raise the route change event.
			OnRouteChange.Raise(this, new RouteChangeEventArgs(output, eConnectionType.Audio | eConnectionType.Video));

			// Raise the active transmission changed event for the output.
			bool state = GetActiveTransmissionState(output, eConnectionType.Video);
			OnActiveTransmissionStateChanged.Raise(this,
												   new TransmissionStateEventArgs(output,
																				  eConnectionType.Audio | eConnectionType.Video,
																				  state));

			// Raise the active inputs change event for the input
			OnActiveInputsChanged.Raise(this);
		}

		#endregion
	}
}
