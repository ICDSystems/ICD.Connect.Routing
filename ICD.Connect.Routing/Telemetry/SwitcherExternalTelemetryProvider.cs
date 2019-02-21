using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Telemetry;
using ICD.Connect.Telemetry.EventArguments;

namespace ICD.Connect.Routing.Telemetry
{
	public class SwitcherExternalTelemetryProvider : ISwitcherExternalTelemetryProvider
	{
		public event EventHandler OnRequestTelemetryRebuild;
		public event EventHandler<BoolEventArgs> OnAudioBreakawayEnabledChanged;
		public event EventHandler<BoolEventArgs> OnUsbBreakawayEnabledChanged;
		public event EventHandler<IndexedBooleanEventArgs> OnVideoInputSyncChanged;
		public event EventHandler<IndexedBooleanEventArgs> OnVideoOutputSyncChanged;
		public event EventHandler<IndexedBooleanEventArgs> OnAudioOutputMuteChanged;
		public event EventHandler<IndexedUshortEventArgs> OnAudioOutputVolumeChanged;
		public event EventHandler<StringEventArgs> OnIpAddressChanged;
		public event EventHandler<StringEventArgs> OnHostnameChanged;
		public event EventHandler<StringEventArgs> OnSubnetMaskChanged;
		public event EventHandler<StringEventArgs> OnMacAddressChanged;
		public event EventHandler<StringEventArgs> OnDefaultRouterChanged;
		public event EventHandler<IndexedStringEventArgs> OnVideoInputIdChanged;
		public event EventHandler<IndexedStringEventArgs> OnVideoInputNameChanged;
		public event EventHandler<IndexedStringEventArgs> OnVideoInputSyncTypeChanged;
		public event EventHandler<IndexedStringEventArgs> OnVideoInputResolutionChanged;
		public event EventHandler<IndexedStringEventArgs> OnVideoOutputIdChanged;
		public event EventHandler<IndexedStringEventArgs> OnVideoOutputNameChanged;
		public event EventHandler<IndexedStringEventArgs> OnVideoOutputSyncTypeChanged;
		public event EventHandler<IndexedStringEventArgs> OnVideoOutputResolutionChanged;
		public event EventHandler<IndexedStringEventArgs> OnVideoOutputEncodingChanged;
		public event EventHandler<IndexedStringEventArgs> OnAudioOutputNameChanged;
		public event EventHandler<IndexedStringEventArgs> OnAudioOutputFormatChanged;
		public event EventHandler<IndexedStringEventArgs> OnUsbOutputIdChanged;

		private ITelemetryProvider m_Parent;

		public void SetParent(ITelemetryProvider provider)
		{
			m_Parent = provider;
		}   

		public bool AudioBreakawayEnabled { get { return GetSwitcherAudioBreakawayEnabled(); } }
		public bool UsbBreakawayEnabled { get { return GetSwitcherUsbBreakawayEnabled(); } }
		public IEnumerable<bool> VideoInputSync { get { return GetSwitcherVideoInputSyncStates(); } }
		public IEnumerable<bool> VideoOutputSync { get { return GetSwitcherVideoOutputSyncStates(); } }
		public IEnumerable<bool> AudioOutputMute { get; private set; }
		public IEnumerable<ushort> AudioOutputVolume { get; private set; }
		public string IpAddress { get; private set; }
		public string Hostname { get; private set; }
		public string SubnetMask { get; private set; }
		public string MacAddress { get; private set; }
		public string DefaultRouter { get; private set; }
		public IEnumerable<string> VideoInputId { get { return GetSwitcherVideoInputIds(); } }
		public IEnumerable<string> VideoInputName { get { return GetSwitcherVideoInputNames(); } }
		public IEnumerable<string> VideoInputSyncType { get { return GetSwitcherVideoInputSyncTypeNames(); } }
		public IEnumerable<string> VideoInputResolution { get { return GetSwitcherVideoInputResolutions(); } }
		public IEnumerable<string> VideoOutputId { get { return GetSwitcherVideoOutputIds(); } }
		public IEnumerable<string> VideoOutputName { get { return GetSwitcherVideoOutputNames(); } }
		public IEnumerable<string> VideoOutputSyncType { get; private set; }
		public IEnumerable<string> VideoOutputResolution { get; private set; }
		public IEnumerable<string> VideoOutputEncoding { get; private set; }
		public IEnumerable<string> AudioOutputName { get; private set; }
		public IEnumerable<string> AudioOutputFormat { get; private set; }
		public IEnumerable<string> UsbOutputId { get; private set; }

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
			IDeviceBase device = m_Parent as IDeviceBase;
			if (device == null)
				return false;

			IRouteSwitcherControl control = device.Controls.GetControl<IRouteSwitcherControl>();
			return control != null && control.AudioBreakawayEnabled;
		}

		private bool GetSwitcherUsbBreakawayEnabled()
		{
			IDeviceBase device = m_Parent as IDeviceBase;
			if (device == null)
				return false;

			IRouteSwitcherControl control = device.Controls.GetControl<IRouteSwitcherControl>();
			return control != null && control.UsbBreakawayEnabled;
		}

		private IEnumerable<bool> GetSwitcherVideoInputSyncStates()
		{
			IDeviceBase device = m_Parent as IDeviceBase;
			if (device == null)
				yield break;

			IRouteDestinationControl control = device.Controls.GetControl<IRouteDestinationControl>();
			if (control == null)
				yield break;

			for (int i = 1; i <= control.GetInputs().Count(); i++)
			{
				yield return control.GetSignalDetectedState(i, eConnectionType.Video);
			}
		}

		private IEnumerable<bool> GetSwitcherVideoOutputSyncStates()
		{
			IDeviceBase device = m_Parent as IDeviceBase;
			if (device == null)
				yield break;

			IRouteSourceControl control = device.Controls.GetControl<IRouteSourceControl>();
			if (control == null)
				yield break;

			for (int i = 1; i <= control.GetOutputs().Count(); i++)
			{
				//TODO: FIX ME
			}
		}

		private IEnumerable<string> GetSwitcherVideoInputIds()
		{
			IDeviceBase device = m_Parent as IDeviceBase;
			if (device == null)
				yield break;

			IRouteSwitcherControl control = device.Controls.GetControl<IRouteSwitcherControl>();
			if (control == null)
				yield break;

			foreach (var id in control.GetSwitcherVideoInputIds())
				yield return id;
		}

		private IEnumerable<string> GetSwitcherVideoInputNames()
		{
			IDeviceBase device = m_Parent as IDeviceBase;
			if (device == null)
				yield break;

			IRouteSwitcherControl control = device.Controls.GetControl<IRouteSwitcherControl>();
			if (control == null)
				yield break;

			foreach (var name in control.GetSwitcherVideoInputNames())
				yield return name;
		}

		private IEnumerable<string> GetSwitcherVideoInputSyncTypeNames()
		{
			IDeviceBase device = m_Parent as IDeviceBase;
			if (device == null)
				yield break;

			IRouteSwitcherControl control = device.Controls.GetControl<IRouteSwitcherControl>();
			if (control == null)
				yield break;

			foreach (var sync in control.GetSwitcherVideoInputSyncType())
				yield return sync;
		}

		private IEnumerable<string> GetSwitcherVideoInputResolutions()
		{
			IDeviceBase device = m_Parent as IDeviceBase;
			if (device == null)
				yield break;

			IRouteSwitcherControl control = device.Controls.GetControl<IRouteSwitcherControl>();
			if (control == null)
				yield break;

			foreach (var resolution in control.GetSwitcherVideoInputResolutions())
				yield return resolution;
		}

		private IEnumerable<string> GetSwitcherVideoOutputIds()
		{
			IDeviceBase device = m_Parent as IDeviceBase;
			if (device == null)
				yield break;

			IRouteSwitcherControl control = device.Controls.GetControl<IRouteSwitcherControl>();
			if (control == null)
				yield break;

			foreach (var id in control.GetSwitcherVideoOutputIds())
				yield return id;
		}

		private IEnumerable<string> GetSwitcherVideoOutputNames()
		{
			IDeviceBase device = m_Parent as IDeviceBase;
			if (device == null)
				yield break;

			IRouteSwitcherControl control = device.Controls.GetControl<IRouteSwitcherControl>();
			if (control == null)
				yield break;

			foreach (var name in control.GetSwitcherVideoOutputNames())
				yield return name;
		}

		#endregion
	}
}