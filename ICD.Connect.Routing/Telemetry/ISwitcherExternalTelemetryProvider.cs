using System;
using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Telemetry;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.EventArguments;

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

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_SYNC_CHANGED)]
		event EventHandler<IndexedBooleanEventArgs> OnVideoInputSyncChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_SYNC, SwitcherTelemetryNames.VIDEO_INPUT_SYNC_CHANGED)]
		IEnumerable<bool> VideoInputSync { get; }

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC_CHANGED)]
		event EventHandler<IndexedBooleanEventArgs> OnVideoOutputSyncChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC, SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC_CHANGED)]
		IEnumerable<bool> VideoOutputSync { get; }

		[EventTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_MUTE_CHANGED)]
		event EventHandler<IndexedBooleanEventArgs> OnAudioOutputMuteChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_MUTE, SwitcherTelemetryNames.AUDIO_OUTPUT_MUTE_CHANGED)]
		IEnumerable<bool> AudioOutputMute { get; }

		[EventTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_VOLUME_CHANGED)]
		event EventHandler<IndexedUshortEventArgs> OnAudioOutputVolumeChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_VOLUME, SwitcherTelemetryNames.AUDIO_OUTPUT_VOLUME_CHANGED)]
		IEnumerable<ushort> AudioOutputVolume { get; }

		[EventTelemetry(SwitcherTelemetryNames.IP_ADDRESS_CHANGED)]
		event EventHandler<StringEventArgs> OnIpAddressChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.IP_ADDRESS,SwitcherTelemetryNames.IP_ADDRESS_CHANGED)]
		string IpAddress { get; }

		[EventTelemetry(SwitcherTelemetryNames.HOSTNAME_CHANGED)]
		event EventHandler<StringEventArgs> OnHostnameChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.HOSTNAME,SwitcherTelemetryNames.HOSTNAME_CHANGED)]
		string Hostname { get; }

		[EventTelemetry(SwitcherTelemetryNames.SUBNET_MASK_CHANGED)]
		event EventHandler<StringEventArgs> OnSubnetMaskChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.SUBNET_MASK,SwitcherTelemetryNames.SUBNET_MASK_CHANGED)]
		string SubnetMask { get; }

		[EventTelemetry(SwitcherTelemetryNames.MAC_ADDRESS_CHANGED)]
		event EventHandler<StringEventArgs> OnMacAddressChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.MAC_ADDRESS,SwitcherTelemetryNames.MAC_ADDRESS_CHANGED)]
		string MacAddress { get; }

		[EventTelemetry(SwitcherTelemetryNames.DEFAULT_ROUTER_CHANGED)]
		event EventHandler<StringEventArgs> OnDefaultRouterChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.DEFAULT_ROUTER,SwitcherTelemetryNames.DEFAULT_ROUTER_CHANGED)]
		string DefaultRouter { get; }

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_ID_CHANGED)]
		event EventHandler<IndexedStringEventArgs> OnVideoInputIdChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_ID,SwitcherTelemetryNames.VIDEO_INPUT_ID_CHANGED)]
		IEnumerable<string> VideoInputId { get; }

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_NAME_CHANGED)]
		event EventHandler<IndexedStringEventArgs> OnVideoInputNameChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_NAME,SwitcherTelemetryNames.VIDEO_INPUT_NAME_CHANGED)]
		IEnumerable<string> VideoInputName { get; }

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_SYNC_CHANGED)]
		event EventHandler<IndexedStringEventArgs> OnVideoInputSyncTypeChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_SYNC_TYPE,SwitcherTelemetryNames.VIDEO_INPUT_SYNC_TYPE_CHANGED)]
		IEnumerable<string> VideoInputSyncType { get; }

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_RESOLUTION_CHANGED)]
		event EventHandler<IndexedStringEventArgs> OnVideoInputResolutionChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_RESOLUTION,SwitcherTelemetryNames.VIDEO_INPUT_RESOLUTION_CHANGED)]
		IEnumerable<string> VideoInputResolution { get; }

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_ID_CHANGED)]
		event EventHandler<IndexedStringEventArgs> OnVideoOutputIdChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_ID,SwitcherTelemetryNames.VIDEO_OUTPUT_ID_CHANGED)]
		IEnumerable<string> VideoOutputId { get; }

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_NAME_CHANGED)]
		event EventHandler<IndexedStringEventArgs> OnVideoOutputNameChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_NAME,SwitcherTelemetryNames.VIDEO_OUTPUT_NAME_CHANGED)]
		IEnumerable<string> VideoOutputName { get; }

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC_TYPE_CHANGED)]
		event EventHandler<IndexedStringEventArgs> OnVideoOutputSyncTypeChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC_TYPE,SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC_TYPE_CHANGED)]
		IEnumerable<string> VideoOutputSyncType { get; }

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_RESOLUTION_CHANGED)]
		event EventHandler<IndexedStringEventArgs> OnVideoOutputResolutionChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_RESOLUTION,SwitcherTelemetryNames.VIDEO_OUTPUT_RESOLUTION_CHANGED)]
		IEnumerable<string> VideoOutputResolution { get; }

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_ENCODING_CHANGED)]
		event EventHandler<IndexedStringEventArgs> OnVideoOutputEncodingChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_ENCODING,SwitcherTelemetryNames.VIDEO_OUTPUT_ENCODING_CHANGED)]
		IEnumerable<string> VideoOutputEncoding { get; }

		[EventTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_NAME_CHANGED)]
		event EventHandler<IndexedStringEventArgs> OnAudioOutputNameChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_NAME,SwitcherTelemetryNames.AUDIO_OUTPUT_NAME_CHANGED)]
		IEnumerable<string> AudioOutputName { get; }

		[EventTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_FORMAT_CHANGED)]
		event EventHandler<IndexedStringEventArgs> OnAudioOutputFormatChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_FORMAT,SwitcherTelemetryNames.AUDIO_OUTPUT_FORMAT_CHANGED)]
		IEnumerable<string> AudioOutputFormat { get; }

		[EventTelemetry(SwitcherTelemetryNames.USB_OUTPUT_ID_CHANGED)]
		event EventHandler<IndexedStringEventArgs> OnUsbOutputIdChanged;
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.USB_OUTPUT_ID,SwitcherTelemetryNames.USB_OUTPUT_ID_CHANGED)]
		IEnumerable<string> UsbOutputId { get; }

		[MethodTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_MUTE_COMMAND)]
		void Mute(int outputNumber);
		
		[MethodTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_UNMUTE_COMMAND)]
		void Unmute(int outputNumber);

		[MethodTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_VOLUME_COMMAND)]
		void SetVolume(int outputNumber);
	}
}