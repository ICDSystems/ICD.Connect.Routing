using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DM;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Misc.CrestronPro.Utils;
using ICD.Connect.Misc.CrestronPro.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd8X2
{
	public sealed class HdMd8X2SwitcherControl : AbstractRouteSwitcherControl<HdMd8X2Adapter>
	{
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public override event EventHandler OnActiveInputsChanged;
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

		private readonly Dictionary<eConnectionType, Dictionary<int, bool>> m_CachedSourceStates;
		private readonly SafeCriticalSection m_CachedSourceSection;

		private HdMd8x2 m_Switcher;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		public HdMd8X2SwitcherControl(HdMd8X2Adapter parent)
			: base(parent, 0)
		{
			m_CachedSourceStates = new Dictionary<eConnectionType, Dictionary<int, bool>>();
			m_CachedSourceSection = new SafeCriticalSection();

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

		#region Routing

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
								.Select(t => GetSignalDetectedState(input, t))
								.Unanimous(false);
			}

			DMInput switcherInput = m_Switcher.Inputs[(uint)input];

			switch (type)
			{
				case eConnectionType.Video:
					return switcherInput.VideoDetectedFeedback.BoolValue;

				case eConnectionType.Audio:
					// No way of detecting audio?
					return true;

				case eConnectionType.Usb:
					return switcherInput.USBRoutedToFeedback.EndpointOnlineFeedback;

				default:
					return false;
			}
		}

		/// <summary>
		/// Routes the input to the given output.
		/// </summary>
		/// <param name="info"></param>
		/// <returns>True if routing successful.</returns>
		public override bool Route(RouteOperation info)
		{
			eConnectionType type = info.ConnectionType;
			int input = info.LocalInput;
			int output = info.LocalOutput;

			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
								.Select(t => this.Route(input, output, t))
								.Unanimous(false);
			}

			DMOutput switcherOutput = m_Switcher.Outputs[(uint)output];
			DMInput switcherInput = m_Switcher.Inputs[(uint)input];

			switch (type)
			{
				case eConnectionType.Audio:
					switcherOutput.AudioOut = switcherInput;
					return switcherOutput.GetSafeAudioOutFeedback() == switcherInput;

				case eConnectionType.Video:
					switcherOutput.VideoOut = switcherInput;
					return switcherOutput.GetSafeVideoOutFeedback() == switcherInput;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if unrouting successful.</returns>
		public override bool ClearOutput(int output, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
								.Select(t => ClearOutput(output, t))
								.Unanimous(false);
			}

			DMOutput switcherOutput = m_Switcher.Outputs[(uint)output];

			switch (type)
			{
				case eConnectionType.Video:
					if (switcherOutput.GetSafeVideoOutFeedback() == null)
						return false;
					switcherOutput.VideoOut = null;
					return true;

				case eConnectionType.Audio:
					if (switcherOutput.GetSafeAudioOutFeedback() == null)
						return false;
					switcherOutput.AudioOut = null;
					return true;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// Gets the routed inputs for the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs(int output, eConnectionType type)
		{
			IcdHashSet<int> inputs = new IcdHashSet<int>();
			DMOutput switcherOutput = m_Switcher.Outputs[(uint)output];

			if (type.HasFlag(eConnectionType.Audio))
			{
				DMInput audioInput = switcherOutput.GetSafeAudioOutFeedback();
				if (audioInput != null)
					inputs.Add((int)audioInput.Number);
			}

			if (type.HasFlag(eConnectionType.Video))
			{
				DMInput videoInput = switcherOutput.GetSafeVideoOutFeedback();
				if (videoInput != null)
					inputs.Add((int)videoInput.Number);
			}

			return inputs.Select(i => GetInput(i));
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			return
				Enumerable.Range(1, m_Switcher.NumberOfOutputs)
						  .Select(i => new ConnectorInfo(i, eConnectionType.Audio | eConnectionType.Video));
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			return
				Enumerable.Range(1, m_Switcher.NumberOfInputs)
						  .Select(i => new ConnectorInfo(i, eConnectionType.Audio | eConnectionType.Video));
		}

		#endregion

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Subscribe(HdMd8X2Adapter parent)
		{
			parent.OnSwitcherChanged += ParentOnSwitcherChanged;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Unsubscribe(HdMd8X2Adapter parent)
		{
			parent.OnSwitcherChanged -= ParentOnSwitcherChanged;
		}

		private void ParentOnSwitcherChanged(HdMd8X2Adapter sender, HdMd8x2 switcher)
		{
			SetSwitcher(switcher);
		}

		/// <summary>
		/// Sets the wrapped switcher.
		/// </summary>
		/// <param name="switcher"></param>
		private void SetSwitcher(HdMd8x2 switcher)
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
		private void Subscribe(HdMd8x2 switcher)
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
		private void Unsubscribe(HdMd8x2 switcher)
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
			eConnectionType type = DmUtils.DmEventToConnectionType(args.EventId);

			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
				SourceDetectionChange((int)args.Number, flag);
		}

		/// <summary>
		/// Handles the detection change for individual connection types.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		private void SourceDetectionChange(int input, eConnectionType type)
		{
			bool state = GetSignalDetectedState(input, type);

			m_CachedSourceSection.Enter();

			try
			{
				if (!m_CachedSourceStates.ContainsKey(type))
					m_CachedSourceStates[type] = new Dictionary<int, bool>();

				bool cachedState = m_CachedSourceStates[type].GetDefault(input, false);
				if (state == cachedState)
					return;

				m_CachedSourceStates[type][input] = state;
			}
			finally
			{
				m_CachedSourceSection.Leave();
			}

			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(input, type, state));
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
