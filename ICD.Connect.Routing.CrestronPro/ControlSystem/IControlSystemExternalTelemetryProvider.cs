using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices;
using ICD.Connect.Telemetry;
using ICD.Connect.Telemetry.Attributes;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem
{
	public interface IControlSystemExternalTelemetryProvider : IExternalTelemetryProvider
	{
		[EventTelemetry(DeviceTelemetryNames.DEVICE_DHCP_STATUS_CHANGED)]
		event EventHandler<BoolEventArgs> OnDhcpStatusChanged;

		[DynamicPropertyTelemetry(DeviceTelemetryNames.DEVICE_DHCP_STATUS, null, DeviceTelemetryNames.DEVICE_DHCP_STATUS_CHANGED)]
		bool DhcpStatus { get; }

		[StaticPropertyTelemetry(DeviceTelemetryNames.DEVICE_MODEL)]
		string ProcessorModel { get; }

		[StaticPropertyTelemetry(DeviceTelemetryNames.DEVICE_FIRMWARE_VERSION)]
		string ProcessorFirmwareVersion { get; }

		[StaticPropertyTelemetry(DeviceTelemetryNames.DEVICE_FIRMWARE_DATE)]
		string ProcessorFirmwareDate { get; }

		[StaticPropertyTelemetry(DeviceTelemetryNames.DEVICE_MAC_ADDRESS)]
		string ProcessorMacAddress { get; }

		[EventTelemetry(DeviceTelemetryNames.DEVICE_IP_ADDRESS_CHANGED)]
		event EventHandler<StringEventArgs> OnProcessorIpAddressChanged; 
		
		[DynamicPropertyTelemetry(DeviceTelemetryNames.DEVICE_IP_ADDRESS, null, DeviceTelemetryNames.DEVICE_IP_ADDRESS_CHANGED)]
		string ProcessorIpAddress { get; }

		[EventTelemetry(DeviceTelemetryNames.DEVICE_HOSTNAME_CHANGED)]
		event EventHandler<StringEventArgs> OnProcessorHostnameChanged; 

		[DynamicPropertyTelemetry(DeviceTelemetryNames.DEVICE_HOSTNAME, null, DeviceTelemetryNames.DEVICE_HOSTNAME_CHANGED)]
		string ProcessorHostname { get; }
		
		[StaticPropertyTelemetry(DeviceTelemetryNames.DEVICE_SERIAL_NUMBER)]
		string ProcessorSerialNumber { get; }

		[EventTelemetry(DeviceTelemetryNames.DEVICE_UPTIME_CHANGED)]
		event EventHandler<StringEventArgs> OnProcessorUptimeChanged;

		[DynamicPropertyTelemetry(DeviceTelemetryNames.DEVICE_UPTIME, null, DeviceTelemetryNames.DEVICE_UPTIME_CHANGED)]
		string ProcessorUptime { get; }

		[EventTelemetry(ControlSystemExternalTelemetryNames.PROGRAM_UPTIME_CHANGED)]
		event EventHandler<StringEventArgs> OnProgramUptimeChanged;

		[DynamicPropertyTelemetry(ControlSystemExternalTelemetryNames.PROGRAM_UPTIME, null, ControlSystemExternalTelemetryNames.PROGRAM_UPTIME_CHANGED
			)]
		string ProgramUptime { get; }
		
		[StaticPropertyTelemetry(ControlSystemExternalTelemetryNames.PROGRAMMER_NAME)]
		string ProgrammerName { get; }

		[EventTelemetry(ControlSystemExternalTelemetryNames.SYSTEM_NAME_CHANGED)]
		event EventHandler<StringEventArgs> OnSystemNameChanged; 

		[DynamicPropertyTelemetry(ControlSystemExternalTelemetryNames.SYSTEM_NAME, null, ControlSystemExternalTelemetryNames.SYSTEM_NAME_CHANGED)]
		string SystemName { get; }
		
		[StaticPropertyTelemetry(ControlSystemExternalTelemetryNames.PROGRAM_SOURCE_FILE)]
		string ProgramSourceFile { get; }

		[StaticPropertyTelemetry(ControlSystemExternalTelemetryNames.PROGRAM_COMPLIE_DATE)]
		string ProgramCompileDate { get; }

		[EventTelemetry(DeviceTelemetryNames.DEVICE_IP_ADDRESS_SECONDARY_CHANGED)]
		event EventHandler<StringEventArgs> OnIpAddressSecondaryChanged; 

		[DynamicPropertyTelemetry(DeviceTelemetryNames.DEVICE_IP_ADDRESS_SECONDARY, null, DeviceTelemetryNames.DEVICE_IP_ADDRESS_SECONDARY_CHANGED)]
		string ProcessorIpAddressSecondary { get; }

		[EventTelemetry(DeviceTelemetryNames.DEVICE_HOSTNAME_SECONDARY_CHANGED)]
		event EventHandler<StringEventArgs> OnProcessorHostnameCustomChanged; 

		[DynamicPropertyTelemetry(DeviceTelemetryNames.DEVICE_HOSTNAME_SECONDARY, null, DeviceTelemetryNames.DEVICE_HOSTNAME_SECONDARY_CHANGED)]
		string ProcessorHostnameCustom { get; }
	}
}