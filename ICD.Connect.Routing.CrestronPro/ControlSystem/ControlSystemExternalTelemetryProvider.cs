using System;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Timers;
using ICD.Connect.Routing.Telemetry;
using ICD.Connect.Devices.Telemetry;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Providers.External;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem
{
	[UsedImplicitly]
	public sealed class ControlSystemExternalTelemetryProvider : AbstractExternalTelemetryProvider<IControlSystemDevice>
	{
		private const string PROGRAMMER_NAME = "ICD Systems";
		private const string SYSTEM_NAME = "ICD.Connect";

		private const long UPTIME_UPDATE_TIMER_INTERVAL = 10 * 60 * 1000;

		#region Events

		[EventTelemetry(DeviceTelemetryNames.DEVICE_DHCP_STATUS_CHANGED)]
		public event EventHandler<BoolEventArgs> OnDhcpStatusChanged;

		[EventTelemetry(DeviceTelemetryNames.DEVICE_IP_ADDRESS_CHANGED)]
		public event EventHandler<StringEventArgs> OnProcessorIpAddressChanged;

		[EventTelemetry(DeviceTelemetryNames.DEVICE_IP_ADDRESS_SECONDARY_CHANGED)]
		public event EventHandler<StringEventArgs> OnProcessorIpAddressSecondaryChanged;

		[EventTelemetry(DeviceTelemetryNames.DEVICE_HOSTNAME_CHANGED)]
		public event EventHandler<StringEventArgs> OnProcessorHostnameChanged;

		[EventTelemetry(DeviceTelemetryNames.DEVICE_HOSTNAME_SECONDARY_CHANGED)]
		public event EventHandler<StringEventArgs> OnProcessorHostnameSecondaryChanged;

		[EventTelemetry(DeviceTelemetryNames.DEVICE_UPTIME_CHANGED)]
		public event EventHandler<GenericEventArgs<TimeSpan>> OnProcessorUptimeChanged;

		[EventTelemetry(ControlSystemExternalTelemetryNames.PROGRAM_UPTIME_CHANGED)]
		public event EventHandler<GenericEventArgs<TimeSpan>> OnProgramUptimeChanged;

		#endregion

		private readonly IcdTimer m_UptimeUpdateTimer;

		#region Properties

		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_DHCP_STATUS, null, DeviceTelemetryNames.DEVICE_DHCP_STATUS_CHANGED)]
		public bool DhcpStatus { get { return IcdEnvironment.DhcpStatus; } }

		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_FIRMWARE_VERSION, null, null)]
		public Version ProcessorFirmwareVersion { get { return ProcessorUtils.ModelVersion; } }

		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_FIRMWARE_DATE, null, null)]
		public DateTime ProcessorFirmwareDate { get { return ProcessorUtils.ModelVersionDate; } }

		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_MAC_ADDRESS, null, null)]
		public string ProcessorMacAddress { get { return IcdEnvironment.MacAddresses.FirstOrDefault() ?? string.Empty; } }

		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_IP_ADDRESS, null, DeviceTelemetryNames.DEVICE_IP_ADDRESS_CHANGED)]
		public string ProcessorIpAddress { get { return IcdEnvironment.NetworkAddresses.FirstOrDefault() ?? string.Empty; } }

		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_IP_ADDRESS_SECONDARY, null, DeviceTelemetryNames.DEVICE_IP_ADDRESS_SECONDARY_CHANGED)]
		public string ProcessorIpAddressSecondary { get { return IcdEnvironment.NetworkAddresses.Skip(1).FirstOrDefault() ?? string.Empty; } }

		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_HOSTNAME, null, DeviceTelemetryNames.DEVICE_HOSTNAME_CHANGED)]
		public string ProcessorHostname { get { return IcdEnvironment.Hostnames.FirstOrDefault() ?? string.Empty; } }

		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_HOSTNAME_SECONDARY, null, DeviceTelemetryNames.DEVICE_HOSTNAME_SECONDARY_CHANGED)]
		public string ProcessorHostnameSecondary { get { return IcdEnvironment.Hostnames.Skip(1).FirstOrDefault() ?? string.Empty; } }

		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_UPTIME, null, DeviceTelemetryNames.DEVICE_UPTIME_CHANGED)]
		public TimeSpan ProcessorUptime { get { return ProcessorUtils.GetSystemUptime(); } }

		[PropertyTelemetry(ControlSystemExternalTelemetryNames.PROGRAM_UPTIME, null, ControlSystemExternalTelemetryNames.PROGRAM_UPTIME_CHANGED)]
		public TimeSpan ProgramUptime { get { return ProcessorUtils.GetProgramUptime(); } }

		[PropertyTelemetry(ControlSystemExternalTelemetryNames.PROGRAMMER_NAME, null, null)]
		public string ProgrammerName { get { return PROGRAMMER_NAME; } }

		[PropertyTelemetry(ControlSystemExternalTelemetryNames.SYSTEM_NAME, null, null)]
		public string SystemName { get { return SYSTEM_NAME; } }

		[PropertyTelemetry(ControlSystemExternalTelemetryNames.PROGRAM_SOURCE_FILE, null, null)]
		public string ProgramSourceFile { get { return ProgramUtils.ProgramFile; } }

		[PropertyTelemetry(ControlSystemExternalTelemetryNames.PROGRAM_COMPLIE_DATE, null, null)]
		public DateTime ProgramCompileDate { get { return ProgramUtils.CompiledDate; } }

		#endregion

		public ControlSystemExternalTelemetryProvider()
		{
			IcdEnvironment.OnEthernetEvent += IcdEnvironmentOnEthernetEvent;

			// Adjust the uptime counter every 10 minutes
			m_UptimeUpdateTimer = new IcdTimer();
			m_UptimeUpdateTimer.OnElapsed += UptimeUpdateTimerOnElapsed;
			m_UptimeUpdateTimer.Restart(UPTIME_UPDATE_TIMER_INTERVAL);
		}

		#region Private Methods

		private void UptimeUpdateTimerOnElapsed(object sender, EventArgs eventArgs)
		{
			OnProcessorUptimeChanged.Raise(this, new GenericEventArgs<TimeSpan>(ProcessorUptime));
			OnProgramUptimeChanged.Raise(this, new GenericEventArgs<TimeSpan>(ProgramUptime));
			m_UptimeUpdateTimer.Restart(UPTIME_UPDATE_TIMER_INTERVAL);
		}

		private void IcdEnvironmentOnEthernetEvent(IcdEnvironment.eEthernetAdapterType adapter, IcdEnvironment.eEthernetEventType type)
		{
			OnDhcpStatusChanged.Raise(this, new BoolEventArgs(DhcpStatus));
			OnProcessorIpAddressChanged.Raise(this, new StringEventArgs(ProcessorIpAddress));
			OnProcessorIpAddressSecondaryChanged.Raise(this, new StringEventArgs(ProcessorIpAddressSecondary));
			OnProcessorHostnameChanged.Raise(this, new StringEventArgs(ProcessorHostname));
			OnProcessorHostnameSecondaryChanged.Raise(this, new StringEventArgs(ProcessorHostnameSecondary));
		}

		#endregion
	}
}
