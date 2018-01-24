#if SIMPLSHARP
using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DM;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Misc.CrestronPro.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd4X24kE
{
	public sealed class HdMd4X24kESwitcherControl : AbstractRouteSwitcherControl<HdMd4X24kEAdapter>
	{
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

		// Crestron is garbage at tracking the active routing states on the 4x2,
		// so lets just cache the assigned routes until the device tells us otherwise.
		private readonly SwitcherCache m_Cache;

		[CanBeNull] private HdMd4x24kE m_Switcher;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		public HdMd4X24kESwitcherControl(HdMd4X24kEAdapter parent)
			: base(parent, 0)
		{
			m_Cache = new SwitcherCache();
			m_Cache.OnActiveInputsChanged += CacheOnActiveInputsChanged;
			m_Cache.OnSourceDetectionStateChange += CacheOnSourceDetectionStateChange;
			m_Cache.OnActiveTransmissionStateChanged += CacheOnActiveTransmissionStateChanged;
			m_Cache.OnRouteChange += CacheOnRouteChange;

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
			if (m_Switcher == null)
				return false;

			eConnectionType type = info.ConnectionType;
			int input = info.LocalInput;
			int output = info.LocalOutput;

			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .Select(f => this.Route(input, output, f))
				                .Unanimous(false);
			}

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
					m_Switcher.Outputs[(uint)output].VideoOut = m_Switcher.Inputs[(uint)input];
					return m_Cache.SetInputForOutput(output, input, eConnectionType.Audio | eConnectionType.Video);

				default:
// ReSharper disable once NotResolvedInText
					throw new ArgumentOutOfRangeException("type", string.Format("Unexpected value {0}", type));
			}
		}

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		public override bool ClearOutput(int output, eConnectionType type)
		{
			if (m_Switcher == null)
				return false;

			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .Select(f => ClearOutput(output, f))
				                .Unanimous(false);
			}

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
					m_Switcher.Outputs[(uint)output].VideoOut = null;
					return m_Cache.SetInputForOutput(output, null, eConnectionType.Audio | eConnectionType.Video);

				default:
					throw new ArgumentOutOfRangeException("type", string.Format("Unexpected value {0}", type));
			}
		}

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
				                .Select(f => GetSignalDetectedState(input, f)).Unanimous(false);
			}

			switch (type)
			{
				case eConnectionType.Audio:
					return true;
				case eConnectionType.Video:
					return m_Cache.GetSourceDetectedState(input, type);

				default:
					throw new ArgumentOutOfRangeException("type", string.Format("Unexpected value {0}", type));
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
				throw new IndexOutOfRangeException(string.Format("{0} has no input with address {1}", GetType().Name, input));
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
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			return m_Cache.GetOutputsForInput(input, type);
		}

		/// <summary>
		/// Gets the input routed to the given output matching the given type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Type has multiple flags.</exception>
		public override ConnectorInfo? GetInput(int output, eConnectionType type)
		{
			return m_Cache.GetInputConnectorInfoForOutput(output, type);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns true if video is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private bool GetVideoDetectedFeedback(int input)
		{
			if (m_Switcher == null)
				return false;

			return m_Switcher.Inputs[(uint)input].VideoDetectedFeedback.BoolValue;
		}

		/// <summary>
		/// Gets the input for the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		private int? GetInputFeedback(int output)
		{
			if (m_Switcher == null)
				return null;

			DMInput input = m_Switcher.HdmiOutputs[(uint)output].GetSafeVideoOutFeedback();
			return input == null ? null : (int?)input.Number;
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

		private void ParentOnSwitcherChanged(IDmSwitcherAdapter dmSwitcherAdapter, Switch switcher)
		{
			SetSwitcher(switcher as HdMd4x24kE);
		}

		private void SetSwitcher(HdMd4x24kE switcher)
		{
			Unsubscribe(m_Switcher);
			m_Switcher = switcher;
			Subscribe(m_Switcher);

			RebuildCache();
		}

		/// <summary>
		/// Reverts the cache to the current state of the switcher.
		/// </summary>
		private void RebuildCache()
		{
			m_Cache.Clear();

			// Source detection
			foreach (ConnectorInfo input in GetInputs())
			{
				bool detected = GetVideoDetectedFeedback(input.Address);
				m_Cache.SetSourceDetectedState(input.Address, eConnectionType.Audio | eConnectionType.Video, detected);
			}

			// Routing
			foreach (ConnectorInfo output in GetOutputs())
			{
				int? input = GetInputFeedback(output.Address);
				m_Cache.SetInputForOutput(output.Address, input, eConnectionType.Audio | eConnectionType.Video);
			}
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
			int input = (int)args.Number;

			bool state = GetVideoDetectedFeedback(input);
			m_Cache.SetSourceDetectedState(input, eConnectionType.Audio | eConnectionType.Video, state);
		}

		/// <summary>
		/// Called when an output state changes.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void SwitcherOnDmOutputChange(Switch device, DMOutputEventArgs args)
		{
			if (m_Switcher == null)
				return;

			if (args.EventId != DMOutputEventIds.VideoOutEventId)
				return;

			DMInput input = m_Switcher.HdmiOutputs[args.Number].GetSafeVideoOutFeedback();
			int? address = input == null ? null : (int?)input.Number;

			m_Cache.SetInputForOutput((int)args.Number, address, eConnectionType.Audio | eConnectionType.Video);
		}

		#endregion

		#region Cache Callbacks

		private void CacheOnRouteChange(object sender, RouteChangeEventArgs args)
		{
			OnRouteChange.Raise(this, new RouteChangeEventArgs(args));
		}

		private void CacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs args)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(args));
		}

		private void CacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs args)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(args));
		}

		private void CacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs args)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(args));
		}

		#endregion
	}
}

#endif
