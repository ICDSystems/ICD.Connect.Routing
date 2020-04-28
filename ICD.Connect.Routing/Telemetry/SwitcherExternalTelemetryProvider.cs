using System;
using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Telemetry;

namespace ICD.Connect.Routing.Telemetry
{
	public sealed class SwitcherExternalTelemetryProvider : ISwitcherExternalTelemetryProvider
	{
		public event EventHandler OnRequestTelemetryRebuild;
		public event EventHandler<BoolEventArgs> OnAudioBreakawayEnabledChanged;
		public event EventHandler<BoolEventArgs> OnUsbBreakawayEnabledChanged;
		public event EventHandler<StringEventArgs> OnIpAddressChanged;
		public event EventHandler<StringEventArgs> OnHostnameChanged;
		public event EventHandler<StringEventArgs> OnSubnetMaskChanged;
		public event EventHandler<StringEventArgs> OnMacAddressChanged;
		public event EventHandler<StringEventArgs> OnDefaultRouterChanged;
		
		private readonly IcdHashSet<InputPort> m_InputPorts = new IcdHashSet<InputPort>();
		private readonly IcdHashSet<OutputPort> m_OutputPorts = new IcdHashSet<OutputPort>();

		private IRouteSwitcherControl m_Parent;

		public void SetParent(ITelemetryProvider provider)
		{
			var switcherControl = provider as IRouteSwitcherControl;
			if (switcherControl == null)
				throw new ArgumentException("Provider for Switcher Telemetry must be IRouteSwitcherControl.");

			m_Parent = switcherControl;
			m_InputPorts.Clear();
			m_OutputPorts.Clear();
            m_InputPorts.AddRange(m_Parent.GetInputPorts());
            m_OutputPorts.AddRange(m_Parent.GetOutputPorts());
		}   

		public bool AudioBreakawayEnabled { get { return GetSwitcherAudioBreakawayEnabled(); } }
		public bool UsbBreakawayEnabled { get { return GetSwitcherUsbBreakawayEnabled(); } }
		public IEnumerable<InputPort> SwitcherInputPorts { get { return m_InputPorts; } }
		public IEnumerable<OutputPort> SwitcherOutputPorts { get { return m_OutputPorts; } }

		public void Mute(int outputNumber)
		{
			throw new NotImplementedException();
		}

		public void Unmute(int outputNumber)
		{
			throw new NotImplementedException();
		}

		public void SetVolume(int outputNumber)
		{
			throw new NotImplementedException();
		}

		#region Private Methods

		private bool GetSwitcherAudioBreakawayEnabled()
		{
			return m_Parent.AudioBreakawayEnabled;
		}

		private bool GetSwitcherUsbBreakawayEnabled()
		{
			return m_Parent.UsbBreakawayEnabled;
		}

		#endregion
	}
}