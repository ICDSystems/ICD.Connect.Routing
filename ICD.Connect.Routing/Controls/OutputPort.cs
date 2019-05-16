using System;
using ICD.Common.Utils.Attributes;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Telemetry;
using ICD.Connect.Telemetry.Attributes;

namespace ICD.Connect.Routing.Controls
{
	public sealed class OutputPort : InputOutputPortBase
	{
		#region Events

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC_CHANGED)]
		event EventHandler<BoolEventArgs> VideoOutputSyncChanged;

		[EventTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_MUTE_CHANGED)]
		event EventHandler<BoolEventArgs> AudioOutputMuteChanged;

		[EventTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_VOLUME_CHANGED)]
		event EventHandler<FloatEventArgs> AudioOutputVolumeChanged;

		[EventTelemetry(SwitcherTelemetryNames.OUTPUT_ID_CHANGED)]
		event EventHandler<StringEventArgs> OutputIdChanged;

		[EventTelemetry(SwitcherTelemetryNames.OUTPUT_NAME_CHANGED)]
		event EventHandler<StringEventArgs> OutputNameChanged;

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC_TYPE_CHANGED)]
		event EventHandler<StringEventArgs> VideoOutputSyncTypeChanged;

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_RESOLUTION_CHANGED)]
		event EventHandler<StringEventArgs> VideoOutputResolutionChanged;

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_ENCODING_CHANGED)]
		event EventHandler<StringEventArgs> VideoOutputEncodingChanged;

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_SOURCE_CHANGED)]
		event EventHandler<StringEventArgs> VideoOutputSourceChanged; 

		[EventTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_SOURCE_CHANGED)]
		event EventHandler<StringEventArgs> AudioOutputSourceChanged;

		[EventTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_FORMAT_CHANGED)]
		event EventHandler<StringEventArgs> AudioOutputFormatChanged;

		[EventTelemetry(SwitcherTelemetryNames.USB_OUTPUT_ID_CHANGED)]
		event EventHandler<StringEventArgs> UsbOutputIdChanged;

		#endregion

		#region Private Members

		private bool m_VideoOutputSync;
		private bool m_AudioOutputMute;
		private float m_AudioOutputVolume;
		private string m_OutputId;
		private string m_OutputName;
		private string m_VideoOutputSyncType;
		private string m_VideoOutputResolution;
		private string m_VideoOutputEncoding;
		private string m_VideoOutputSource;
		private string m_AudioOutputSource;
		private string m_AudioOutputFormat;
		private string m_UsbOutputId;
		private bool m_VideoOutputSyncFeedbackSupported;
		private bool m_AudioOutputMuteFeedbackSupported;
		private bool m_AudioOutputVolumeFeedbackSupported;
		private bool m_OutputIdFeedbackSupported;
		private bool m_OutputNameFeedbackSupported;
		private bool m_VideoOutputSyncTypeFeedbackSupported;
		private bool m_VideoOutputResolutionFeedbackSupported;
		private bool m_VideoOutputEncodingFeedbackSupported;
		private bool m_VideoOutputSourceFeedbackSupported;
		private bool m_AudioOutputSourceFeedbackSupported;
		private bool m_AudioOutputFormatFeedbackSupported;
		private bool m_UsbOutputIdFeedbackSupported;

		#endregion

		#region Public Properties

		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC, SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC_CHANGED)]
		public bool VideoOutputSync
		{
			get { return m_VideoOutputSync; }
			set
			{
				if (m_VideoOutputSync == value)
					return;
				m_VideoOutputSync = value;
				VideoOutputSyncChanged.Raise(this, new BoolEventArgs(m_VideoOutputSync));
			}
		}

		public bool VideoOutputSyncFeedbackSupported
		{
			get
			{
				return m_VideoOutputSyncFeedbackSupported
					   && ConnectionType.HasFlag(eConnectionType.Video);
			}
			set { m_VideoOutputSyncFeedbackSupported = value; }
		}

		[DynamicPropertyTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_MUTE, SwitcherTelemetryNames.AUDIO_OUTPUT_MUTE_CHANGED)]
		public bool AudioOutputMute
		{
			get { return m_AudioOutputMute; }
			set
			{
				if (m_AudioOutputMute == value)
					return;
				m_AudioOutputMute = value;
				AudioOutputMuteChanged.Raise(this, new BoolEventArgs(m_AudioOutputMute));
			}
		}

		public bool AudioOutputMuteFeedbackSupported
		{
			get
			{
				return m_AudioOutputMuteFeedbackSupported
					   && ConnectionType.HasFlag(eConnectionType.Audio);
			}
			set { m_AudioOutputMuteFeedbackSupported = value; }
		}

		[Range(0.0f, 1.0f)]
		[DynamicPropertyTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_VOLUME, SwitcherTelemetryNames.AUDIO_OUTPUT_VOLUME_CHANGED)]
		public float AudioOutputVolume
		{
			get { return m_AudioOutputVolume; }
			set
			{
				if (Math.Abs(m_AudioOutputVolume - value) < 0.01f)
					return;
				m_AudioOutputVolume = value;
				AudioOutputVolumeChanged.Raise(this, new FloatEventArgs(m_AudioOutputVolume));
			}
		}

		public bool AudioOutputVolumeFeedbackSupported
		{
			get
			{
				return m_AudioOutputVolumeFeedbackSupported
					   && ConnectionType.HasFlag(eConnectionType.Audio);
			}
			set { m_AudioOutputVolumeFeedbackSupported = value; }
		}

		[DynamicPropertyTelemetry(SwitcherTelemetryNames.OUTPUT_ID, SwitcherTelemetryNames.OUTPUT_ID_CHANGED)]
		public string OutputId
		{
			get { return m_OutputId; }
			set
			{
				if (m_OutputId == value)
					return;
				m_OutputId = value;
				OutputIdChanged.Raise(this, new StringEventArgs(m_OutputId));
			}
		}

		public bool OutputIdFeedbackSupport
		{
			get { return m_OutputIdFeedbackSupported; }
			set { m_OutputIdFeedbackSupported = value; }
		}

		[DynamicPropertyTelemetry(SwitcherTelemetryNames.OUTPUT_NAME, SwitcherTelemetryNames.OUTPUT_NAME_CHANGED)]
		public string OutputName
		{
			get { return m_OutputName; }
			set
			{
				if (m_OutputName == value)
					return;
				m_OutputName = value;
				OutputNameChanged.Raise(this, new StringEventArgs(m_OutputName));
			}
		}

		public bool OutputNameFeedbackSupport
		{
			get { return m_OutputNameFeedbackSupported; }
			set { m_OutputNameFeedbackSupported = value; }
		}

		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC_TYPE, SwitcherTelemetryNames.VIDEO_OUTPUT_SYNC_TYPE_CHANGED)]
		public string VideoOutputSyncType
		{
			get { return m_VideoOutputSyncType; }
			set
			{
				if (m_VideoOutputSyncType == value)
					return;
				m_VideoOutputSyncType = value;
				VideoOutputSyncTypeChanged.Raise(this, new StringEventArgs(m_VideoOutputSyncType));
			}
		}

		public bool VideoOutputSyncTypeFeedbackSupport
		{
			get
			{
				return m_VideoOutputSyncTypeFeedbackSupported
					   && ConnectionType.HasFlag(eConnectionType.Video);
			}
			set { m_VideoOutputSyncTypeFeedbackSupported = value; }
		}

		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_RESOLUTION, SwitcherTelemetryNames.VIDEO_OUTPUT_RESOLUTION_CHANGED)]
		public string VideoOutputResolution
		{
			get { return m_VideoOutputResolution; }
			set
			{
				if (m_VideoOutputResolution == value)
					return;
				m_VideoOutputResolution = value;
				VideoOutputResolutionChanged.Raise(this, new StringEventArgs(m_VideoOutputResolution));
			}
		}

		public bool VideoOutputResolutionFeedbackSupport
		{
			get
			{
				return m_VideoOutputResolutionFeedbackSupported
					   && ConnectionType.HasFlag(eConnectionType.Video);
			}
			set { m_VideoOutputResolutionFeedbackSupported = value; }
		}

		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_ENCODING, SwitcherTelemetryNames.VIDEO_OUTPUT_ENCODING_CHANGED)]
		public string VideoOutputEncoding
		{
			get { return m_VideoOutputEncoding; }
			set
			{
				if (m_VideoOutputEncoding == value)
					return;
				m_VideoOutputEncoding = value;
				VideoOutputEncodingChanged.Raise(this, new StringEventArgs(m_VideoOutputEncoding));
			}
		}

		public bool VideoOutputEncodingFeedbackSupport
		{
			get
			{
				return m_VideoOutputEncodingFeedbackSupported
					   && ConnectionType.HasFlag(eConnectionType.Video);
			}
			set { m_VideoOutputEncodingFeedbackSupported = value; }
		}

		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_OUTPUT_SOURCE, SwitcherTelemetryNames.VIDEO_OUTPUT_SOURCE_CHANGED)]
		public string VideoOutputSource
		{
			get { return m_VideoOutputSource; }
			set
			{
				if (m_VideoOutputSource == value)
					return;
				m_VideoOutputSource = value;
				VideoOutputSourceChanged.Raise(this, new StringEventArgs(m_VideoOutputSource));
			}
		}

		public bool VideoOutputSourceFeedbackSupport
		{
			get
			{
				return m_VideoOutputSourceFeedbackSupported
					   && ConnectionType.HasFlag(eConnectionType.Video);
			}
			set { m_VideoOutputSourceFeedbackSupported = value; }
		}


		[DynamicPropertyTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_SOURCE, SwitcherTelemetryNames.AUDIO_OUTPUT_SOURCE_CHANGED)]
		public string AudioOutputSource
		{
			get { return m_AudioOutputSource; }
			set
			{
				if (m_AudioOutputSource == value)
					return;
				m_AudioOutputSource = value;
				AudioOutputSourceChanged.Raise(this, new StringEventArgs(m_AudioOutputSource));
			}
		}

		public bool AudioOutputSourceFeedbackSupport
		{
			get
			{
				return m_AudioOutputSourceFeedbackSupported
					   && ConnectionType.HasFlag(eConnectionType.Audio);
			}
			set { m_AudioOutputSourceFeedbackSupported = value; }
		}

		[DynamicPropertyTelemetry(SwitcherTelemetryNames.AUDIO_OUTPUT_FORMAT, SwitcherTelemetryNames.AUDIO_OUTPUT_FORMAT_CHANGED)]
		public string AudioOutputFormat
		{
			get { return m_AudioOutputFormat; }
			set
			{
				if (m_AudioOutputFormat == value)
					return;
				m_AudioOutputFormat = value;
				AudioOutputFormatChanged.Raise(this, new StringEventArgs(m_AudioOutputFormat));
			}
		}

		public bool AudioOutputFormatFeedbackSupport
		{
			get
			{
				return m_AudioOutputFormatFeedbackSupported
					   && ConnectionType.HasFlag(eConnectionType.Audio);
			}
			set { m_AudioOutputFormatFeedbackSupported = value; }
		}

		[DynamicPropertyTelemetry(SwitcherTelemetryNames.USB_OUTPUT_ID, SwitcherTelemetryNames.USB_OUTPUT_ID_CHANGED)]
		public string UsbOutputId
		{
			get { return m_UsbOutputId; }
			set
			{
				if (m_UsbOutputId == value)
					return;
				m_UsbOutputId = value;
				UsbOutputIdChanged.Raise(this, new StringEventArgs(m_UsbOutputId));
			}
		}

		public bool UsbOutputIdFeedbackSupported
		{
			get
			{
				return m_UsbOutputIdFeedbackSupported
					   && ConnectionType.HasFlag(eConnectionType.Usb);
			}
			set { m_UsbOutputIdFeedbackSupported = value; }
		}

		#endregion
	}
}