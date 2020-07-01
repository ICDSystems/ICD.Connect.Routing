﻿using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Providers.External;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem
{
	public interface IControlSystemExternalTelemetryProvider : IExternalTelemetryProvider
	{
		[EventTelemetry(DeviceTelemetryNames.DEVICE_DHCP_STATUS_CHANGED)]
		event EventHandler<BoolEventArgs> OnDhcpStatusChanged;

		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_DHCP_STATUS, null, DeviceTelemetryNames.DEVICE_DHCP_STATUS_CHANGED)]
		bool DhcpStatus { get; }

		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_FIRMWARE_VERSION, null, null)]
		Version ProcessorFirmwareVersion { get; }

		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_FIRMWARE_DATE, null, null)]
		DateTime ProcessorFirmwareDate { get; }

		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_MAC_ADDRESS, null, null)]
		string ProcessorMacAddress { get; }

		[EventTelemetry(DeviceTelemetryNames.DEVICE_IP_ADDRESS_CHANGED)]
		event EventHandler<StringEventArgs> OnProcessorIpAddressChanged; 
		
		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_IP_ADDRESS, null, DeviceTelemetryNames.DEVICE_IP_ADDRESS_CHANGED)]
		string ProcessorIpAddress { get; }

		[EventTelemetry(DeviceTelemetryNames.DEVICE_HOSTNAME_CHANGED)]
		event EventHandler<StringEventArgs> OnProcessorHostnameChanged; 

		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_HOSTNAME, null, DeviceTelemetryNames.DEVICE_HOSTNAME_CHANGED)]
		string ProcessorHostname { get; }

		[EventTelemetry(DeviceTelemetryNames.DEVICE_UPTIME_CHANGED)]
		event EventHandler<GenericEventArgs<TimeSpan>> OnProcessorUptimeChanged;

		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_UPTIME, null, DeviceTelemetryNames.DEVICE_UPTIME_CHANGED)]
		TimeSpan ProcessorUptime { get; }

		[EventTelemetry(ControlSystemExternalTelemetryNames.PROGRAM_UPTIME_CHANGED)]
		event EventHandler<GenericEventArgs<TimeSpan>> OnProgramUptimeChanged;

		[PropertyTelemetry(ControlSystemExternalTelemetryNames.PROGRAM_UPTIME, null, ControlSystemExternalTelemetryNames.PROGRAM_UPTIME_CHANGED)]
		TimeSpan ProgramUptime { get; }

		[PropertyTelemetry(ControlSystemExternalTelemetryNames.PROGRAMMER_NAME, null, null)]
		string ProgrammerName { get; }

		[EventTelemetry(ControlSystemExternalTelemetryNames.SYSTEM_NAME_CHANGED)]
		event EventHandler<StringEventArgs> OnSystemNameChanged; 

		[PropertyTelemetry(ControlSystemExternalTelemetryNames.SYSTEM_NAME, null, ControlSystemExternalTelemetryNames.SYSTEM_NAME_CHANGED)]
		string SystemName { get; }

		[PropertyTelemetry(ControlSystemExternalTelemetryNames.PROGRAM_SOURCE_FILE, null, null)]
		string ProgramSourceFile { get; }

		[PropertyTelemetry(ControlSystemExternalTelemetryNames.PROGRAM_COMPLIE_DATE, null, null)]
		DateTime ProgramCompileDate { get; }

		[EventTelemetry(DeviceTelemetryNames.DEVICE_IP_ADDRESS_SECONDARY_CHANGED)]
		event EventHandler<StringEventArgs> OnProcessorIpAddressSecondaryChanged; 

		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_IP_ADDRESS_SECONDARY, null, DeviceTelemetryNames.DEVICE_IP_ADDRESS_SECONDARY_CHANGED)]
		string ProcessorIpAddressSecondary { get; }

		[EventTelemetry(DeviceTelemetryNames.DEVICE_HOSTNAME_SECONDARY_CHANGED)]
		event EventHandler<StringEventArgs> OnProcessorHostnameSecondaryChanged; 

		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_HOSTNAME_SECONDARY, null, DeviceTelemetryNames.DEVICE_HOSTNAME_SECONDARY_CHANGED)]
		string ProcessorHostnameSecondary { get; }
	}
}