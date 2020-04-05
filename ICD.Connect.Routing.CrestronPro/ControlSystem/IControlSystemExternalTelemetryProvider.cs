using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Telemetry;
using ICD.Connect.Telemetry.Attributes;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem
{
	public interface IControlSystemExternalTelemetryProvider : IExternalTelemetryProvider
	{
		[EventTelemetry(ControlSystemExternalTelemetryNames.DHCP_STATUS_CHANGED)]
		event EventHandler<BoolEventArgs> OnDhcpStatusChanged;

		[DynamicPropertyTelemetry(ControlSystemExternalTelemetryNames.DHCP_STATUS, null, ControlSystemExternalTelemetryNames.DHCP_STATUS_CHANGED)]
		bool DhcpStatus { get; }

		[StaticPropertyTelemetry(ControlSystemExternalTelemetryNames.PROCESSOR_MODEL)]
		string ProcessorModel { get; }

		[StaticPropertyTelemetry(ControlSystemExternalTelemetryNames.PROCESSOR_FIRMWARE_VER)]
		string ProcessorFirmwareVersion { get; }

		[StaticPropertyTelemetry(ControlSystemExternalTelemetryNames.PROCESSOR_FIRMWARE_DATE)]
		string ProcessorFirmwareDate { get; }

		[StaticPropertyTelemetry(ControlSystemExternalTelemetryNames.PROCESSOR_MAC_ADDRESS)]
		string ProcessorMacAddress { get; }

		[EventTelemetry(ControlSystemExternalTelemetryNames.PROCESSOR_IP_ADDRESS_CHANGED)]
		event EventHandler<StringEventArgs> OnProcessorIpAddressChanged; 
		
		[DynamicPropertyTelemetry(ControlSystemExternalTelemetryNames.PROCESSOR_IP_ADDRESS, null, ControlSystemExternalTelemetryNames.PROCESSOR_IP_ADDRESS_CHANGED)]
		string ProcessorIpAddress { get; }

		[EventTelemetry(ControlSystemExternalTelemetryNames.PROCESSOR_HOSTNAME_CHANGED)]
		event EventHandler<StringEventArgs> OnProcessorHostnameChanged; 

		[DynamicPropertyTelemetry(ControlSystemExternalTelemetryNames.PROCESSOR_HOSTNAME, null, ControlSystemExternalTelemetryNames.PROCESSOR_HOSTNAME_CHANGED)]
		string ProcessorHostname { get; }
		
		[StaticPropertyTelemetry(ControlSystemExternalTelemetryNames.PROCESSOR_SERIAL_NUMBER)]
		string ProcessorSerialNumber { get; }

		[EventTelemetry(ControlSystemExternalTelemetryNames.PROCESSOR_UPTIME_CHANGED)]
		event EventHandler<StringEventArgs> OnProcessorUptimeChanged;

		[DynamicPropertyTelemetry(ControlSystemExternalTelemetryNames.PROCESSOR_UPTIME, null, ControlSystemExternalTelemetryNames.PROCESSOR_UPTIME_CHANGED)]
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

		[EventTelemetry(ControlSystemExternalTelemetryNames.PROCESSOR_IP_ADDRESS_SECONDARY_CHAGNED)]
		event EventHandler<StringEventArgs> OnIpAddressSecondaryChanged; 

		[DynamicPropertyTelemetry(ControlSystemExternalTelemetryNames.PROCESSOR_IP_ADDRESS_SECONDARY, null, ControlSystemExternalTelemetryNames.PROCESSOR_IP_ADDRESS_SECONDARY_CHAGNED)]
		string ProcessorIpAddressSecondary { get; }

		[EventTelemetry(ControlSystemExternalTelemetryNames.PROCESSOR_HOSTNAME_SECONDARY_CHANGED)]
		event EventHandler<StringEventArgs> OnProcessorHostnameCustomChanged; 

		[DynamicPropertyTelemetry(ControlSystemExternalTelemetryNames.PROCESSOR_HOSTNAME_SECONDARY, null, ControlSystemExternalTelemetryNames.PROCESSOR_HOSTNAME_SECONDARY_CHANGED)]
		string ProcessorHostnameCustom { get; }
	}
}