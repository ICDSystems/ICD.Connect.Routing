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
		public event EventHandler<BoolEventArgs> OnVideoInputSyncChanged;

		[EventTelemetry(SwitcherTelemetryNames.INPUT_ID_CHANGED)]
		public event EventHandler<StringEventArgs> OnInputIdChanged;

		[EventTelemetry(SwitcherTelemetryNames.INPUT_NAME_CHANGED)]
		public event EventHandler<StringEventArgs> OnInputNameChanged;

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_SYNC_TYPE_CHANGED)]
		public event EventHandler<StringEventArgs> OnVideoInputSyncTypeChanged;

		[EventTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_RESOLUTION_CHANGED)]
		public event EventHandler<StringEventArgs> OnVideoInputResolutionChanged;

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

		[PropertyTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_SYNC, null, SwitcherTelemetryNames.VIDEO_INPUT_SYNC_CHANGED)]
		public bool VideoInputSync
		{
			get { return m_VideoInputSync; }
			set
			{
				if (m_VideoInputSync == value)
					return;
				m_VideoInputSync = value;
				OnVideoInputSyncChanged.Raise(this, new BoolEventArgs(m_VideoInputSync));
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

		[PropertyTelemetry(SwitcherTelemetryNames.INPUT_ID, null, SwitcherTelemetryNames.INPUT_ID_CHANGED)]
		public string InputId
		{
			get { return m_InputId; }
			set
			{
				if (m_InputId == value)
					return;
				m_InputId = value;
				OnInputIdChanged.Raise(this, new StringEventArgs(m_InputId));
			}
		}

		public bool InputIdFeedbackSupported
		{
			get { return m_InputIdFeedbackSupported; }
			set { m_InputIdFeedbackSupported = value; }
		}

		[PropertyTelemetry(SwitcherTelemetryNames.INPUT_NAME, null, SwitcherTelemetryNames.INPUT_NAME_CHANGED)]
		public string InputName
		{
			get { return m_InputName; }
			set
			{
				if (m_InputName == value)
					return;
				m_InputName = value;
				OnInputNameChanged.Raise(this, new StringEventArgs(m_InputName));
			}
		}

		public bool InputNameFeedbackSupported
		{
			get { return m_InputNameFeedbackSupported; }
			set { m_InputNameFeedbackSupported = value; }
		}

		[PropertyTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_SYNC_TYPE, null, SwitcherTelemetryNames.VIDEO_INPUT_SYNC_TYPE_CHANGED)]
		public string VideoInputSyncType
		{
			get { return m_VideoInputSyncType; }
			set
			{
				if (m_VideoInputSyncType == value)
					return;
				m_VideoInputSyncType = value;
				OnVideoInputSyncTypeChanged.Raise(this, new StringEventArgs(m_VideoInputSyncType));
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

		[PropertyTelemetry(SwitcherTelemetryNames.VIDEO_INPUT_RESOLUTION, null, SwitcherTelemetryNames.VIDEO_INPUT_RESOLUTION_CHANGED)]
		public string VideoInputResolution
		{
			get { return m_VideoInputResolution; }
			set
			{
				if (m_VideoInputResolution == value)
					return;
				m_VideoInputResolution = value;
				OnVideoInputResolutionChanged.Raise(this, new StringEventArgs(m_VideoInputResolution));
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

		/// <summary>
		/// Constructor.
		/// </summary>
		public InputPort()
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="connector"></param>
		public InputPort(ConnectorInfo connector)
			: base(connector)
		{
		}
	}
}