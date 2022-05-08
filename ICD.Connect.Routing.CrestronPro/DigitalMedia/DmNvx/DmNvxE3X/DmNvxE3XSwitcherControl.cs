using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Logging.Activities;
using ICD.Common.Logging.LoggingContexts;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;
#if !NETSTANDARD
using System.Text;
using ICD.Common.Properties;
using ICD.Connect.Misc.CrestronPro.Extensions;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.Dm100xStrBase;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxBaseClass;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Streaming;
using eDeviceMode = Crestron.SimplSharpPro.DM.Streaming.eDeviceMode;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxE3X
{
	public sealed class DmNvxE3XSwitcherControl : AbstractRouteSwitcherControl<IDmNvxE3XAdapter>, IDmNvxSwitcherControl
	{
		private const int INPUT_HDMI_1 = 1;
		private const int INPUT_ANALOG_AUDIO = 3;

		private const int OUTPUT_ANALOG_AUDIO = 2;
		public const int OUTPUT_STREAM = 3;

		private static readonly IcdSortedDictionary<int, ConnectorInfo> s_InputConnectors =
			new IcdSortedDictionary<int, ConnectorInfo>
			{
				{INPUT_HDMI_1, new ConnectorInfo(INPUT_HDMI_1, eConnectionType.Audio | eConnectionType.Video)},
				{INPUT_ANALOG_AUDIO, new ConnectorInfo(INPUT_ANALOG_AUDIO, eConnectionType.Audio)},
			};

		private static readonly IcdSortedDictionary<int, string> s_InputConnectorInputTypes =
			new IcdSortedDictionary<int, string>
			{
				{INPUT_HDMI_1, "HDMI"},
				{INPUT_ANALOG_AUDIO, "Audio"},
			};

		private static readonly IcdSortedDictionary<int, ConnectorInfo> s_OutputConnectors =
			new IcdSortedDictionary<int, ConnectorInfo>
			{
				{OUTPUT_ANALOG_AUDIO, new ConnectorInfo(OUTPUT_ANALOG_AUDIO, eConnectionType.Audio)},
				{OUTPUT_STREAM, new ConnectorInfo(OUTPUT_STREAM, eConnectionType.Audio | eConnectionType.Video)}
			};

		private static readonly IcdSortedDictionary<int, string> s_OutputConnectorOutputTypes =
			new IcdSortedDictionary<int, string>
			{
				{OUTPUT_ANALOG_AUDIO, "Audio"},
				{OUTPUT_STREAM, "Streaming"},
			};

#if !NETSTANDARD
		private static readonly BiDictionary<int, DmNvxControl.eAudioSource> s_AudioSources =
			new BiDictionary<int, DmNvxControl.eAudioSource>
			{
				{INPUT_HDMI_1, DmNvxControl.eAudioSource.Input1},
				{INPUT_ANALOG_AUDIO, DmNvxControl.eAudioSource.AnalogAudio}
			};

		private static readonly BiDictionary<int, eSfpVideoSourceTypes> s_VideoSources =
			new BiDictionary<int, eSfpVideoSourceTypes>
			{
				{INPUT_HDMI_1, eSfpVideoSourceTypes.Hdmi1}
			};
#endif

		/// <summary>
		/// Raised when an input source status changes.
		/// </summary>
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;

		/// <summary>
		/// Raised when the device starts/stops actively using an input, e.g. unroutes an input.
		/// </summary>
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		/// <summary>
		/// Raised when a route changes.
		/// </summary>
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

		/// <summary>
		/// Raised when the server url changes.
		/// </summary>
		public event EventHandler<StringEventArgs> OnServerUrlChange;

		/// <summary>
		/// Raised when the multicast address changes.
		/// </summary>
		public event EventHandler<StringEventArgs> OnMulticastAddressChange;

		/// <summary>
		/// Raised when the secondary audio multicast address changes.
		/// </summary>
		public event EventHandler<StringEventArgs> OnSecondaryAudioMulticastAddressChange;

		/// <summary>
		/// Raised when the last known multicast address changes.
		/// </summary>
		public event EventHandler<StringEventArgs> OnLastKnownMulticastAddressChange;

		/// <summary>
		/// Raised when the last known secondary audio multicast address changes.
		/// </summary>
		public event EventHandler<StringEventArgs> OnLastKnownSecondaryAudioMulticastAddressChange;

		private readonly SwitcherCache m_Cache;

#if !NETSTANDARD
		[CanBeNull]
		private Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass m_Streamer;
		[CanBeNull]
		private DmNvxControl m_NvxControl;
#endif

		private string m_ServerUrl;
		private string m_MulticastAddress;
		private string m_SecondaryAudioMulticastAddress;
		private string m_LastKnownMulticastAddress;
		private string m_LastKnownSecondaryAudioMulticastAddress;

#region Properties

		/// <summary>
		/// Gets the URL for the stream.
		/// </summary>
		public string ServerUrl
		{
			get { return m_ServerUrl; }
			private set
			{
				try
				{
					if (value == m_ServerUrl)
						return;

					m_ServerUrl = value;

					Logger.LogSetTo(eSeverity.Informational, "ServerUrl", m_ServerUrl);

					OnServerUrlChange.Raise(this, new StringEventArgs(m_ServerUrl));
				}
				finally
				{
					Activities.LogActivity(string.IsNullOrEmpty(m_ServerUrl)
						                       ? new Activity(Activity.ePriority.Medium, "Server URL", "Streaming from " + m_ServerUrl,
						                                      eSeverity.Informational)
						                       : new Activity(Activity.ePriority.High, "Server URL", "No Server URL", eSeverity.Error));
				}
			}
		}

		/// <summary>
		/// Gets the multicast address for the stream.
		/// </summary>
		public string MulticastAddress
		{
			get { return m_MulticastAddress; }
			private set
			{
				if (value == m_MulticastAddress)
					return;

				m_MulticastAddress = value;

				if (!string.IsNullOrEmpty(m_MulticastAddress))
					LastKnownMulticastAddress = m_MulticastAddress;

				OnMulticastAddressChange.Raise(this, new StringEventArgs(m_MulticastAddress));
			}
		}

		/// <summary>
		/// Gets the multicast address for the secondary audio stream.
		/// </summary>
		public string SecondaryAudioMulticastAddress
		{
			get { return m_SecondaryAudioMulticastAddress; }
			private set
			{
				if (value == m_SecondaryAudioMulticastAddress)
					return;

				m_SecondaryAudioMulticastAddress = value;

				if (!string.IsNullOrEmpty(m_SecondaryAudioMulticastAddress))
					LastKnownSecondaryAudioMulticastAddress = m_SecondaryAudioMulticastAddress;

				OnSecondaryAudioMulticastAddressChange.Raise(this, new StringEventArgs(m_SecondaryAudioMulticastAddress));
			}
		}

		/// <summary>
		/// Gets the last known multicast address for the stream.
		/// </summary>
		public string LastKnownMulticastAddress
		{
			get { return m_LastKnownMulticastAddress; }
			private set
			{
				if (value == m_LastKnownMulticastAddress)
					return;

				m_LastKnownMulticastAddress = value;

				Logger.LogSetTo(eSeverity.Informational, "Last Known Multicast Address", m_LastKnownMulticastAddress);

				OnLastKnownMulticastAddressChange.Raise(this, new StringEventArgs(m_LastKnownMulticastAddress));
			}
		}

		/// <summary>
		/// Gets the last known multicast address for the secondary audio stream.
		/// </summary>
		public string LastKnownSecondaryAudioMulticastAddress
		{
			get { return m_LastKnownSecondaryAudioMulticastAddress; }
			private set
			{
				if (value == m_LastKnownSecondaryAudioMulticastAddress)
					return;

				m_LastKnownSecondaryAudioMulticastAddress = value;

				Logger.LogSetTo(eSeverity.Informational, "Last Known Secondary Audio Multicast Address", m_LastKnownSecondaryAudioMulticastAddress);

				OnLastKnownSecondaryAudioMulticastAddressChange.Raise(this, new StringEventArgs(m_LastKnownSecondaryAudioMulticastAddress));
			}
		}

#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmNvxE3XSwitcherControl(IDmNvxE3XAdapter parent, int id)
			: base(parent, id)
		{
			m_Cache = new SwitcherCache();
			Subscribe(m_Cache);

#if !NETSTANDARD
			parent.OnStreamerChanged += ParentOnStreamerChanged;

			SetStreamer(parent.Streamer as Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass);
#endif

			// Initialize activities
			ServerUrl = null;
			// Video input is always active
			SetActiveInput(INPUT_HDMI_1, eConnectionType.Video);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnSourceDetectionStateChange = null;
			OnActiveInputsChanged = null;
			OnActiveTransmissionStateChanged = null;
			OnRouteChange = null;
			OnServerUrlChange = null;
			OnMulticastAddressChange = null;
			OnSecondaryAudioMulticastAddressChange = null;
			OnLastKnownMulticastAddressChange = null;
			OnLastKnownSecondaryAudioMulticastAddressChange = null;

			base.DisposeFinal(disposing);

#if !NETSTANDARD
			Parent.OnStreamerChanged -= ParentOnStreamerChanged;

			Unsubscribe(m_Cache);
			SetStreamer(null);
#endif
		}

#region Methods

		/// <summary>
		/// Sets the URL for the stream.
		/// </summary>
		/// <param name="url"></param>
		public void SetServerUrl(string url)
		{
#if !NETSTANDARD
			if (m_NvxControl == null)
				throw new InvalidOperationException("Wrapped streamer is null");

			m_NvxControl.ServerUrl.StringValue = url;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Sets the multicast address for the stream.
		/// </summary>
		/// <param name="address"></param>
		public void SetMulticastAddress(string address)
		{
#if !NETSTANDARD
			if (m_NvxControl == null)
				throw new InvalidOperationException("Wrapped streamer is null");

			m_NvxControl.MulticastAddress.StringValue = address;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Sets the multicast address for the stream.
		/// </summary>
		/// <param name="address"></param>
		public void SetSecondaryAudioMulticastAddress(string address)
		{
#if !NETSTANDARD
			if (m_Streamer == null)
				throw new InvalidOperationException("Wrapped streamer is null");
			if (m_Streamer.SecondaryAudio == null)
				throw new NotSupportedException("Wrapped streamer doesn't support secondary audio");

			m_Streamer.SecondaryAudio.SecondaryAudioMode =
				Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass.DmNvx35xSecondaryAudio.eSecondaryAudioMode.Manual;
			m_Streamer.SecondaryAudio.MulticastAddress.StringValue = address;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			return m_Cache.GetSourceDetectedState(input, type);
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int input)
		{
			if (!ContainsInput(input))
				throw new ArgumentOutOfRangeException("input");

			return s_InputConnectors[input];
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			return s_InputConnectors.Where(kvp => ContainsInput(kvp.Key))
			                        .Select(kvp => kvp.Value);
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int address)
		{
			if (!ContainsOutput(address))
				throw new ArgumentOutOfRangeException("address");

			return s_OutputConnectors[address];
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return s_InputConnectors.ContainsKey(input);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return s_OutputConnectors.ContainsKey(output);
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			return s_OutputConnectors.Where(kvp => ContainsOutput(kvp.Key))
			                         .Select(kvp => kvp.Value);
		}

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			return m_Cache.GetOutputsForInput(input, type);
		}

		/// <summary>
		/// Gets the input routed to the given output matching the given type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Type has multiple flags.</exception>
		public override ConnectorInfo? GetInput(int output, eConnectionType type)
		{
			return m_Cache.GetInputConnectorInfoForOutput(output, type);
		}

		protected override InputPort CreateInputPort(ConnectorInfo input)
		{
			bool supportsVideo = input.ConnectionType.HasFlag(eConnectionType.Video);
			return new InputPort
			{
				Address = input.Address,
				ConnectionType = input.ConnectionType,
				InputId = GetInputId(input),
				InputIdFeedbackSupported = true,
				VideoInputResolution = supportsVideo ? GetVideoInputResolution(input) : null,
				VideoInputResolutionFeedbackSupport = supportsVideo,
				VideoInputSync = supportsVideo && GetVideoInputSyncState(input),
				VideoInputSyncFeedbackSupported = supportsVideo,
				VideoInputSyncType = supportsVideo ? GetVideoInputSyncType(input) : null,
				VideoInputSyncTypeFeedbackSupported = supportsVideo
			};
		}

		protected override OutputPort CreateOutputPort(ConnectorInfo output)
		{
			bool supportsVideo = output.ConnectionType.HasFlag(eConnectionType.Video);
			bool supportsAudio = output.ConnectionType.HasFlag(eConnectionType.Audio);
			return new OutputPort
			{
				Address = output.Address,
				ConnectionType = output.ConnectionType,
				OutputId = GetOutputId(output),
				OutputIdFeedbackSupport = true,
				VideoOutputSyncType = supportsVideo ? GetVideoOutputSyncType(output) : null,
				VideoOutputSyncTypeFeedbackSupport = supportsVideo,
				VideoOutputSource = supportsVideo ? GetActiveSourceIdName(output, eConnectionType.Video) : null,
				VideoOutputSourceFeedbackSupport = supportsVideo,
				AudioOutputSource = supportsAudio ? GetActiveSourceIdName(output, eConnectionType.Audio) : null,
				AudioOutputSourceFeedbackSupport = supportsAudio
			};
		}

		/// <summary>
		/// Performs the given route operation.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public override bool Route(RouteOperation info)
		{
#if !NETSTANDARD
			if (info == null)
				throw new ArgumentNullException("info");

			if (m_Streamer == null)
				throw new InvalidOperationException("Wrapped streamer is null");

			if (m_NvxControl == null)
				throw new InvalidOperationException("Wrapped control is null");

			eConnectionType type = info.ConnectionType;
			int input = info.LocalInput;
			int output = info.LocalOutput;

			if (!ContainsInput(input))
				throw new InvalidOperationException("No input at address");

			if (!ContainsOutput(output))
				throw new InvalidOperationException("No output at address");

			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .Select(f => this.Route(input, output, f))
				                .ToArray()
				                .Unanimous(false);
			}

			switch (type)
			{
				case eConnectionType.Audio:
					m_NvxControl.AudioSource = s_AudioSources.GetValue(input);
					if (m_Streamer.SecondaryAudio != null)
						m_Streamer.SecondaryAudio.Start();
					return SetActiveInput(input, eConnectionType.Audio);

				case eConnectionType.Video:
					// No video routing on this device
					return true;

				default:
					// ReSharper disable once NotResolvedInText
					throw new ArgumentOutOfRangeException("type", string.Format("Unexpected value {0}", type));
			}
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if successfully cleared.</returns>
		public override bool ClearOutput(int output, eConnectionType type)
		{
#if !NETSTANDARD
			if (m_Streamer == null)
				throw new InvalidOperationException("Wrapped streamer is null");

			if (m_NvxControl == null)
				throw new InvalidOperationException("Wrapped control is null");

			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .Select(f => ClearOutput(output, f))
				                .ToArray()
				                .Unanimous(false);
			}

			switch (type)
			{
				case eConnectionType.Audio:
					if (m_Streamer.SecondaryAudio != null)
						m_Streamer.SecondaryAudio.Stop();
					m_NvxControl.AudioSource = DmNvxControl.eAudioSource.SecondaryStreamAudio;
					return SetActiveInput(null, eConnectionType.Audio);

				case eConnectionType.Video:
					// Unable to clear outputs for video
					return false;

				default:
					throw new ArgumentOutOfRangeException("type", string.Format("Unexpected value {0}", type));
			}
#else
			throw new NotSupportedException();
#endif
		}

		private string GetInputId(ConnectorInfo info)
		{
			return string.Format("{0} {1}", s_InputConnectorInputTypes[info.Address], info);
		}

		private bool GetVideoInputSyncState(ConnectorInfo info)
		{
			return GetSignalDetectedState(info.Address, eConnectionType.Video);
		}

		private string GetVideoInputSyncType(ConnectorInfo info)
		{
			bool syncState = GetSignalDetectedState(info.Address, eConnectionType.Video);
			if (!syncState)
				return string.Empty;

			return s_InputConnectorInputTypes[info.Address];
		}

		private string GetVideoInputResolution(ConnectorInfo input)
		{
#if !NETSTANDARD
			if (m_Streamer == null || m_Streamer.HdmiIn == null)
				return null;

				bool syncState = GetSignalDetectedState(input.Address, eConnectionType.Video);
			if (!syncState)
				return string.Empty;

			ushort h; 
				ushort v;
				switch (input.Address)
				{
					case INPUT_HDMI_1:
						h = m_Streamer.HdmiIn[0].VideoAttributes.HorizontalResolutionFeedback.GetUShortValueOrDefault();
						v = m_Streamer.HdmiIn[0].VideoAttributes.VerticalResolutionFeedback.GetUShortValueOrDefault();
						break;
					default:
						h = 0;
						v = 0;
						break;
				}

			return DmInputOutputUtils.GetResolutionFormatted(h, v);
#else
			throw new NotSupportedException();
#endif
		}

		private string GetOutputId(ConnectorInfo output)
		{
			return string.Format("{0} {1}", s_OutputConnectorOutputTypes[output.Address], output.Address);
		}

		private string GetVideoOutputSyncType(ConnectorInfo info)
		{
#if !NETSTANDARD
			if (m_Streamer == null || m_Streamer.HdmiOut == null)
				return null;

			return info.Address == 1 ? m_Streamer.HdmiOut.SyncDetectedFeedback.GetBoolValueOrDefault() ? "HDMI" : "" : null;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Updates the cache with the active input for the given flag.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		private bool SetActiveInput(int? input, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentOutOfRangeException("flag");

			return GetOutputs().Where(c => c.ConnectionType.HasFlag(flag))
			                   .Select(c => m_Cache.SetInputForOutput(c.Address, input, flag))
			                   .ToArray()
			                   .Any(result => result);
		}

#if !NETSTANDARD
		private void SetHdcpTransmitterMode(HdmiOutWithColorSpaceMode.eHdcpTransmitterMode mode)
		{
			if (m_Streamer == null || m_Streamer.HdmiOut == null || mode == m_Streamer.HdmiOut.HdcpTransmitterModeFeedback)
				return;

			m_Streamer.HdmiOut.HdcpTransmitterMode = mode;
		}
#endif
#endregion

#if !NETSTANDARD
#region Streamer Callbacks

		/// <summary>
		/// Called when the parent wrapped streamer instance changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="streamer"></param>
		private void ParentOnStreamerChanged(IDm100XStrBaseAdapter sender,
		                                     Crestron.SimplSharpPro.DM.Streaming.Dm100xStrBase streamer)
		{
			SetStreamer(streamer as Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass);
		}

		/// <summary>
		/// Sets the wrapped streamer instance.
		/// </summary>
		/// <param name="streamer"></param>
		private void SetStreamer(Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass streamer)
		{
			if (streamer == m_Streamer)
				return;

			Unsubscribe(m_Streamer);

			m_Streamer = streamer;
			m_NvxControl = m_Streamer == null ? null : m_Streamer.Control;

			Subscribe(m_Streamer);

			ConfigureStreamer();

			UpdateAudioRouting();
		}

		/// <summary>
		/// Override to control how the assigned streamer behaves.
		/// </summary>
		private void ConfigureStreamer()
		{
			if (m_NvxControl == null)
				return;

			try
			{
				m_NvxControl.EnableAutomaticInitiation();
			}
			catch (InvalidOperationException)
			{
				// Some endpoints don't support this sig
			}

			try
			{
				m_NvxControl.DisableAutomaticInputRouting();
			}
			catch (InvalidOperationException)
			{
				// Some endpoints don't support this sig
			}
		}

		/// <summary>
		/// Subscribe to the streamer events.
		/// </summary>
		/// <param name="streamer"></param>
		private void Subscribe(Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass streamer)
		{
			if (streamer == null)
				return;

			streamer.BaseEvent += StreamerOnBaseEvent;
			if (streamer.SecondaryAudio != null)
				streamer.SecondaryAudio.SecondaryAudioChange += SecondaryAudioOnSecondaryAudioChange;
			if (streamer.HdmiOut != null)
				streamer.HdmiOut.StreamChange += HdmiOutOnStreamChange;
			if (streamer.HdmiIn != null)
				foreach (HdmiInWithColorSpaceMode input in streamer.HdmiIn)
					input.StreamChange += HdmiInOnStreamChange;
		}

		/// <summary>
		/// Unsubscribe from the streamer events.
		/// </summary>
		/// <param name="streamer"></param>
		private void Unsubscribe(Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass streamer)
		{
			if (streamer == null)
				return;

			streamer.BaseEvent -= StreamerOnBaseEvent;
			if (streamer.SecondaryAudio != null)
				streamer.SecondaryAudio.SecondaryAudioChange -= SecondaryAudioOnSecondaryAudioChange;
			if (streamer.HdmiOut != null)
				streamer.HdmiOut.StreamChange -= HdmiOutOnStreamChange;
			if (streamer.HdmiIn != null)
				foreach (HdmiInWithColorSpaceMode input in streamer.HdmiIn)
					input.StreamChange -= HdmiInOnStreamChange;
		}

		private void HdmiInOnStreamChange(Stream stream, StreamEventArgs args)
		{
			HdmiInWithColorSpaceMode input = stream as HdmiInWithColorSpaceMode;

			switch (args.EventId)
			{
				case DMInputEventIds.ResolutionEventId:
					Logger.Log(eSeverity.Debug, "HDMI Input {0} Resolution changed to {1}x{2}&{3}",
					           input.Name,
					           input.VideoAttributes.HorizontalResolutionFeedback.UShortValue,
					           input.VideoAttributes.VerticalResolutionFeedback.UShortValue,
					           input.VideoAttributes.FramesPerSecondFeedback);
					break;
			}
		}

		private void HdmiOutOnStreamChange(Stream stream, StreamEventArgs args)
		{
			switch (args.EventId)
			{
				case DMOutputEventIds.HdcpTransmitterModeFeedbackEventId:
					Logger.Log(eSeverity.Debug, "HDMI Output HDCP Transmitter Mode changed to {0}", m_Streamer.HdmiOut.HdcpTransmitterModeFeedback);
					break;

				case DMOutputEventIds.DisabledByHdcpEventId:
					Logger.Log(eSeverity.Debug, "HDMI Output Disabled By HDCP changed to {0}", m_Streamer.HdmiOut.DisabledByHdcpFeedback.BoolValue);
					break;

				case DMOutputEventIds.ResolutionEventId:
					Logger.Log(eSeverity.Debug, "HDMI Output Resolution changed to {0}", m_Streamer.HdmiOut.ResolutionFeedback);
					break;
			}
		}

		/// <summary>
		/// Called when secondary audio raises a base event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void SecondaryAudioOnSecondaryAudioChange(object sender, GenericEventArgs args)
		{
			switch (args.EventId)
			{
				case DMInputEventIds.MulticastAddressEventId:
					SecondaryAudioMulticastAddress =
						m_Streamer == null
							? null
							: m_Streamer.SecondaryAudio.MulticastAddressFeedback.GetSerialValueOrDefault();
					break;
			}
		}

		/// <summary>
		/// Called when the streamer raises a base event.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void StreamerOnBaseEvent(GenericBase device, BaseEventArgs args)
		{
			switch (args.EventId)
			{
				case DMInputEventIds.ServerUrlEventId:
					ServerUrl =
						m_NvxControl == null
							? null
							: m_NvxControl.ServerUrlFeedback.GetSerialValueOrDefault();
					break;

				case DMInputEventIds.MulticastAddressEventId:
					MulticastAddress =
						m_NvxControl == null
							? null
							: m_NvxControl.MulticastAddressFeedback.GetSerialValueOrDefault();
					break;

				case DMInputEventIds.AudioSourceEventId:
				case DMInputEventIds.ActiveAudioSourceEventId:
					UpdateAudioRouting();
					break;

				case DMInputEventIds.AutomaticInitiationDisabledEventId:
				case DMInputEventIds.AutomaticInputRoutingEnabledEventId:
					ConfigureStreamer();
					break;

				/*
				case DMInputEventIds.ElapsedSecEventId:
					break;

				default:
					IcdConsole.PrintLine(eConsoleColor.Magenta, "{0} - {1}", this, args.EventId);
					break;
				 */
			}

			// Nvx does not have specific event which allows us to see which port, so just update all of them
			if (DmInputOutputUtils.GetIsEventIdResolutionEventId(args.EventId))
			{
				foreach (InputPort input in GetInputPorts())
				{
					ConnectorInfo info = GetInput(input.Address);
					input.VideoInputResolution = GetVideoInputResolution(info);
				}
			}
		}

		/// <summary>
		/// Updates the cached state of the audio routing to match the device.
		/// </summary>
		private void UpdateAudioRouting()
		{
			DmNvxControl.eAudioSource audioSource =
				m_NvxControl == null
					? DmNvxControl.eAudioSource.NoAudioSelected
					: m_NvxControl.AudioSourceFeedback;

			bool secondaryAudioEnabled =
				m_Streamer != null &&
				m_Streamer.SecondaryAudio != null &&
				m_Streamer.SecondaryAudio.StatusFeedback !=
				Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass.DmNvx35xSecondaryAudio.eDeviceStatus.StreamStopped;

			if (audioSource == DmNvxControl.eAudioSource.SecondaryStreamAudio && !secondaryAudioEnabled)
			{
				SetActiveInput(null, eConnectionType.Audio);

				AudioBreakawayEnabled = m_NvxControl != null &&
									m_NvxControl.AudioSourceFeedback != DmNvxControl.eAudioSource.Automatic;
				return;
			}

			int audioInput;
			if (s_AudioSources.TryGetKey(audioSource, out audioInput))
				SetActiveInput(audioInput, eConnectionType.Audio);
			else
				SetActiveInput(null, eConnectionType.Audio);

			AudioBreakawayEnabled = m_NvxControl != null &&
									m_NvxControl.AudioSourceFeedback != DmNvxControl.eAudioSource.Automatic;
		}

#endregion

#endif

#region SwitcherCache Callbacks

		private void Subscribe(SwitcherCache cache)
		{
			cache.OnActiveInputsChanged += CacheOnActiveInputsChanged;
			cache.OnActiveTransmissionStateChanged += CacheOnActiveTransmissionStateChanged;
			cache.OnRouteChange += CacheOnRouteChange;
			cache.OnSourceDetectionStateChange += CacheOnSourceDetectionStateChange;
		}

		private void Unsubscribe(SwitcherCache cache)
		{
			cache.OnActiveInputsChanged -= CacheOnActiveInputsChanged;
			cache.OnActiveTransmissionStateChanged -= CacheOnActiveTransmissionStateChanged;
			cache.OnRouteChange -= CacheOnRouteChange;
			cache.OnSourceDetectionStateChange -= CacheOnSourceDetectionStateChange;
		}

		private void CacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs args)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(args));

			InputPort inputPort = GetInputPort(args.Input);
			ConnectorInfo info = GetInput(args.Input);
			inputPort.VideoInputSync = args.State;
			inputPort.VideoInputSyncType = GetVideoInputSyncType(info);
		}

		private void CacheOnRouteChange(object sender, RouteChangeEventArgs args)
		{
			OnRouteChange.Raise(this, new RouteChangeEventArgs(args));
			OutputPort outputPort = GetOutputPort(args.Output);
			ConnectorInfo info = GetOutput(args.Output);
			if (args.Type.HasFlag(eConnectionType.Video))
				outputPort.VideoOutputSource = GetActiveSourceIdName(info, eConnectionType.Video);
			if (args.Type.HasFlag(eConnectionType.Audio))
				outputPort.AudioOutputSource = GetActiveSourceIdName(info, eConnectionType.Audio);
		}

		private void CacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs args)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(args));
		}

		private void CacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs args)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(args));
		}

#endregion

#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Device Mode", Parent.DeviceMode);
			addRow("Server URL", ServerUrl);
			addRow("Multicast Address", MulticastAddress);
			addRow("Last Known Multicast Address", LastKnownMulticastAddress);
			addRow("Secondary Audio Multicast Address", SecondaryAudioMulticastAddress);
			addRow("Last Known Secondary Audio Multicast Address", SecondaryAudioMulticastAddress);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new GenericConsoleCommand<string>("SetServerUrl", "SetServerUrl <URL>", url => SetServerUrl(url));
			yield return new GenericConsoleCommand<string>("SetMulticastAddress", "SetMulticastAddress <ADDRESS>",
			                                               address => SetMulticastAddress(address));
			yield return new GenericConsoleCommand<string>("SetSecondaryAudioMulticastAddress",
			                                               "SetSecondaryAudioMulticastAddress <ADDRESS>",
			                                               adress => SetSecondaryAudioMulticastAddress(adress));
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

#endregion
	}
}