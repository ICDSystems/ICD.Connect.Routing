using System;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Telemetry;
using ICD.Connect.Telemetry.Attributes;

namespace ICD.Connect.Routing.Controls
{
	public sealed class InputPort : InputOutputPortBase
	{
		#region Events

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_SYNC_CHANGED)]
		event EventHandler<BoolEventArgs> VideoInputSyncChanged;

		[EventTelemetry(SwitcherTelemetryNames.INPUT_ID_CHANGED)]
		event EventHandler<StringEventArgs> InputIdChanged;

		[EventTelemetry(SwitcherTelemetryNames.INPUT_NAME_CHANGED)]
		event EventHandler<StringEventArgs> InputNameChanged;

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_SYNC_CHANGED)]
		event EventHandler<StringEventArgs> VideoInputSyncTypeChanged;

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_RESOLUTION_CHANGED)]
		event EventHandler<StringEventArgs> VideoInputResolutionChanged;

		#endregion

		#region Private Members

		private bool m_VideoInputSync;
		private string m_InputId;
		private string m_InputName;
		private string m_VideoInputSyncType;
		private string m_VideoInputResolution;
		private bool m_VideoInputSyncFeedbackSupported;
		private bool m_InputIdFeedbackSupported;
		private bool m_InputNameFeedbackSupported;
		private bool m_VideoInputSyncTypeFeedbackSupported;
		private bool m_VideoInputResolutionFeedbackSupported;

		#endregion

		#region Public Properties

		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_SYNC, SwitcherTelemetryNames.VIDEO_INPUT_SYNC_CHANGED)]
		public bool VideoInputSync
		{
			get { return m_VideoInputSync; }
			set
			{
				if (m_VideoInputSync == value)
					return;
				m_VideoInputSync = value;
				VideoInputSyncChanged.Raise(this, new BoolEventArgs(m_VideoInputSync));
			}
		}

		public bool VideoInputSyncFeedbackSupported
		{
			get
			{
				return m_VideoInputSyncFeedbackSupported
				       && ConnectionType.HasFlag(eConnectionType.Video);
			}
			set { m_VideoInputSyncFeedbackSupported = value; }
		}

		[DynamicPropertyTelemetry(SwitcherTelemetryNames.INPUT_ID, SwitcherTelemetryNames.INPUT_ID_CHANGED)]
		public string InputId
		{
			get { return m_InputId; }
			set
			{
				if (m_InputId == value)
					return;
				m_InputId = value;
				InputIdChanged.Raise(this, new StringEventArgs(m_InputId));
			}
		}

		public bool InputIdFeedbackSupported
		{
			get { return m_InputIdFeedbackSupported; }
			set { m_InputIdFeedbackSupported = value; }
		}

		[DynamicPropertyTelemetry(SwitcherTelemetryNames.INPUT_NAME, SwitcherTelemetryNames.INPUT_NAME_CHANGED)]
		public string InputName
		{
			get { return m_InputName; }
			set
			{
				if (m_InputName == value)
					return;
				m_InputName = value;
				InputNameChanged.Raise(this, new StringEventArgs(m_InputName));
			}
		}

		public bool InputNameFeedbackSupported
		{
			get { return m_InputNameFeedbackSupported; }
			set { m_InputNameFeedbackSupported = value; }
		}

		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_SYNC_TYPE, SwitcherTelemetryNames.VIDEO_INPUT_SYNC_TYPE_CHANGED)]
		public string VideoInputSyncType
		{
			get { return m_VideoInputSyncType; }
			set
			{
				if (m_VideoInputSyncType == value)
					return;
				m_VideoInputSyncType = value;
				VideoInputSyncTypeChanged.Raise(this, new StringEventArgs(m_VideoInputSyncType));
			}
		}

		public bool VideoInputSyncTypeFeedbackSupported
		{
			get
			{
				return m_VideoInputSyncTypeFeedbackSupported
				       && ConnectionType.HasFlag(eConnectionType.Video);
			}
			set { m_VideoInputSyncTypeFeedbackSupported = value; }
		}

		[DynamicPropertyTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_RESOLUTION, SwitcherTelemetryNames.VIDEO_INPUT_RESOLUTION_CHANGED)]
		public string VideoInputResolution
		{
			get { return m_VideoInputResolution; }
			set
			{
				if (m_VideoInputResolution == value)
					return;
				m_VideoInputResolution = value;
				VideoInputResolutionChanged.Raise(this, new StringEventArgs(m_VideoInputResolution));
			}
		}

		public bool VideoInputResolutionFeedbackSupport
		{
			get
			{
				return m_VideoInputResolutionFeedbackSupported
				       && ConnectionType.HasFlag(eConnectionType.Video);
			}
			set { m_VideoInputResolutionFeedbackSupported = value; }
		}



		#endregion
	}
}