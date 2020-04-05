using System;
using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Telemetry;
using ICD.Connect.Telemetry.Attributes;

namespace ICD.Connect.Routing.Telemetry
{
	public interface ISwitcherExternalTelemetryProvider : IExternalTelemetryProvider
	{
		[EventTelemetry(SwitcherTelemetryNames.AUDIO_BREAKAWAY_ENABLED_CHANGED)]
		event EventHandler<BoolEventArgs> OnAudioBreakawayEnabledChanged; 
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.AUDIO_BREAKAWAY_ENABLED, null, SwitcherTelemetryNames.AUDIO_BREAKAWAY_ENABLED_CHANGED)]
		bool AudioBreakawayEnabled { get; }

		[EventTelemetry(SwitcherTelemetryNames.USB_BREAKAWAY_ENABLED_CHANGED)]
		event EventHandler<BoolEventArgs> OnUsbBreakawayEnabledChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.USB_BREAKAWAY_ENABLED, null, SwitcherTelemetryNames.USB_BREAKAWAY_ENABLED_CHANGED)]
		bool UsbBreakawayEnabled { get; }

		[EventTelemetry(SwitcherTelemetryNames.IP_ADDRESS_CHANGED)]
		event EventHandler<StringEventArgs> OnIpAddressChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.IP_ADDRESS, null, SwitcherTelemetryNames.IP_ADDRESS_CHANGED)]
		string IpAddress { get; }

		[EventTelemetry(SwitcherTelemetryNames.HOSTNAME_CHANGED)]
		event EventHandler<StringEventArgs> OnHostnameChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.HOSTNAME, null, SwitcherTelemetryNames.HOSTNAME_CHANGED)]
		string Hostname { get; }

		[EventTelemetry(SwitcherTelemetryNames.SUBNET_MASK_CHANGED)]
		event EventHandler<StringEventArgs> OnSubnetMaskChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.SUBNET_MASK, null, SwitcherTelemetryNames.SUBNET_MASK_CHANGED)]
		string SubnetMask { get; }

		[EventTelemetry(SwitcherTelemetryNames.MAC_ADDRESS_CHANGED)]
		event EventHandler<StringEventArgs> OnMacAddressChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.MAC_ADDRESS, null, SwitcherTelemetryNames.MAC_ADDRESS_CHANGED)]
		string MacAddress { get; }

		[EventTelemetry(SwitcherTelemetryNames.DEFAULT_ROUTER_CHANGED)]
		event EventHandler<StringEventArgs> OnDefaultRouterChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.DEFAULT_ROUTER, null, SwitcherTelemetryNames.DEFAULT_ROUTER_CHANGED)]
		string DefaultRouter { get; }

		[CollectionTelemetry(SwitcherTelemetryNames.INPUT_PORTS)]
		IEnumerable<InputPort> SwitcherInputPorts { get; }

		[CollectionTelemetry(SwitcherTelemetryNames.OUTPUT_PORTS)]
		IEnumerable<OutputPort> SwitcherOutputPorts { get; }
		
		[MethodTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_MUTE_COMMAND)]
		void Mute(int outputNumber);
		
		[MethodTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_UNMUTE_COMMAND)]
		void Unmute(int outputNumber);

		[MethodTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_VOLUME_COMMAND)]
		void SetVolume(int outputNumber);
	}
}