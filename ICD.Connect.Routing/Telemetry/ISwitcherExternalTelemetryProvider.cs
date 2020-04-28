using System;
using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Telemetry;
using ICD.Connect.Telemetry.Attributes;

namespace ICD.Connect.Routing.Telemetry
{
	public interface ISwitcherExternalTelemetryProvider : IExternalTelemetryProvider
	{
		[EventTelemetry(SwitcherTelemetryNames.AUDIO_BREAKAWAY_ENABLED_CHANGED)]
		event EventHandler<BoolEventArgs> OnAudioBreakawayEnabledChanged; 
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.AUDIO_BREAKAWAY_ENABLED, SwitcherTelemetryNames.AUDIO_BREAKAWAY_ENABLED_CHANGED)]
		bool AudioBreakawayEnabled { get; }

		[EventTelemetry(SwitcherTelemetryNames.USB_BREAKAWAY_ENABLED_CHANGED)]
		event EventHandler<BoolEventArgs> OnUsbBreakawayEnabledChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.USB_BREAKAWAY_ENABLED, SwitcherTelemetryNames.USB_BREAKAWAY_ENABLED_CHANGED)]
		bool UsbBreakawayEnabled { get; }

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