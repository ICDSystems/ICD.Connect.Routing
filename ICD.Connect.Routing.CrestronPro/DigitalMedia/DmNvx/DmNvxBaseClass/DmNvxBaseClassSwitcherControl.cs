using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Streaming;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.Dm100xStrBase;
#endif
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxBaseClass
{
	/// <summary>
	/// In TX mode we treat the NVX as a splitter with independent AV: Each selected input for Audio/Video
	/// is distributed to all outputs.
	/// 
	/// In RX mode we treat the NVX as a switch with multiple inputs which may be routed independently to
	/// the HDMI and Analog Audio outputs.
	/// </summary>
	public sealed class DmNvxBaseClassSwitcherControl : AbstractRouteSwitcherControl<IDmNvxBaseClassAdapter>
	{
		private const int INPUT_HDMI_1 = 1;
		private const int INPUT_HDMI_2 = 2;
		private const int INPUT_ANALOG_AUDIO = 3;
		public const int INPUT_STREAM = 4;
		public const int INPUT_SECONDARY_AUDIO_STREAM = 5;

		private const int OUTPUT_HDMI = 1;
		private const int OUTPUT_ANALOG_AUDIO = 2;
		public const int OUTPUT_STREAM = 3;
		public const int OUTPUT_SECONDARY_AUDIO_STREAM = 4;

		private static readonly IcdOrderedDictionary<int, ConnectorInfo> s_InputConnectors =
			new IcdOrderedDictionary<int, ConnectorInfo>
			{
				{INPUT_HDMI_1, new ConnectorInfo(INPUT_HDMI_1, eConnectionType.Audio | eConnectionType.Video)},
				{INPUT_HDMI_2, new ConnectorInfo(INPUT_HDMI_2, eConnectionType.Audio | eConnectionType.Video)},
				{INPUT_ANALOG_AUDIO, new ConnectorInfo(INPUT_ANALOG_AUDIO, eConnectionType.Audio)},
				{INPUT_STREAM, new ConnectorInfo(INPUT_STREAM, eConnectionType.Audio | eConnectionType.Video)},
				{INPUT_SECONDARY_AUDIO_STREAM, new ConnectorInfo(INPUT_SECONDARY_AUDIO_STREAM, eConnectionType.Audio)}
			};

		private static readonly IcdOrderedDictionary<int, ConnectorInfo> s_OutputConnectors =
			new IcdOrderedDictionary<int, ConnectorInfo>
			{
				{OUTPUT_HDMI, new ConnectorInfo(OUTPUT_HDMI, eConnectionType.Audio | eConnectionType.Video)},
				{OUTPUT_ANALOG_AUDIO, new ConnectorInfo(OUTPUT_ANALOG_AUDIO, eConnectionType.Audio)},
				{OUTPUT_STREAM, new ConnectorInfo(OUTPUT_STREAM, eConnectionType.Audio | eConnectionType.Video)},
				{OUTPUT_SECONDARY_AUDIO_STREAM, new ConnectorInfo(OUTPUT_SECONDARY_AUDIO_STREAM, eConnectionType.Audio)}
			};

#if SIMPLSHARP
		private static readonly BiDictionary<int, DmNvxControl.eAudioSource> s_AudioSources =
			new BiDictionary<int, DmNvxControl.eAudioSource>
			{
				{INPUT_HDMI_1, DmNvxControl.eAudioSource.Input1},
				{INPUT_HDMI_2, DmNvxControl.eAudioSource.Input2},
				{INPUT_ANALOG_AUDIO, DmNvxControl.eAudioSource.AnalogAudio},
				{INPUT_STREAM, DmNvxControl.eAudioSource.PrimaryStreamAudio},
				{INPUT_SECONDARY_AUDIO_STREAM, DmNvxControl.eAudioSource.SecondaryStreamAudio},
			};

		private static readonly BiDictionary<int, eSfpVideoSourceTypes> s_VideoSources =
			new BiDictionary<int, eSfpVideoSourceTypes>
			{
				{INPUT_HDMI_1, eSfpVideoSourceTypes.Hdmi1},
				{INPUT_HDMI_2, eSfpVideoSourceTypes.Hdmi2},
				{INPUT_STREAM, eSfpVideoSourceTypes.Stream}
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

#if SIMPLSHARP
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
				if (value == m_ServerUrl)
					return;

				m_ServerUrl = value;

				OnServerUrlChange.Raise(this, new StringEventArgs(m_ServerUrl));
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

				OnLastKnownSecondaryAudioMulticastAddressChange.Raise(this, new StringEventArgs(m_LastKnownSecondaryAudioMulticastAddress));
			}
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmNvxBaseClassSwitcherControl(IDmNvxBaseClassAdapter parent, int id)
			: base(parent, id)
		{
			m_Cache = new SwitcherCache();

			m_Cache.OnActiveInputsChanged += CacheOnActiveInputsChanged;
			m_Cache.OnActiveTransmissionStateChanged += CacheOnActiveTransmissionStateChanged;
			m_Cache.OnRouteChange += CacheOnRouteChange;
			m_Cache.OnSourceDetectionStateChange += CacheOnSourceDetectionStateChange;

#if SIMPLSHARP
			parent.OnStreamerChanged += ParentOnStreamerChanged;

			SetStreamer(parent.Streamer as Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass);
#endif
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

#if SIMPLSHARP
			Parent.OnStreamerChanged -= ParentOnStreamerChanged;

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
#if SIMPLSHARP
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
#if SIMPLSHARP
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
#if SIMPLSHARP
			if (m_Streamer == null)
				throw new InvalidOperationException("Wrapped streamer is null");

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
			if (Parent.DeviceMode == eDeviceMode.Transmitter)
			{
				switch (input)
				{
					case INPUT_STREAM:
					case INPUT_SECONDARY_AUDIO_STREAM:
						return false;
				}
			}

			return s_InputConnectors.ContainsKey(input);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			if (Parent.DeviceMode == eDeviceMode.Receiver)
			{
				switch (output)
				{
					case OUTPUT_STREAM:
					case OUTPUT_SECONDARY_AUDIO_STREAM:
						return false;
				}
			}

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

		/// <summary>
		/// Performs the given route operation.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public override bool Route(RouteOperation info)
		{
#if SIMPLSHARP
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
					m_Streamer.SecondaryAudio.Start();
					return SetActiveInput(input, eConnectionType.Audio);

				case eConnectionType.Video:
					m_NvxControl.VideoSource = s_VideoSources.GetValue(input);
					return SetActiveInput(input, eConnectionType.Video);

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
#if SIMPLSHARP
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
					m_Streamer.SecondaryAudio.Stop();
					m_NvxControl.AudioSource = DmNvxControl.eAudioSource.SecondaryStreamAudio;
					return SetActiveInput(null, eConnectionType.Audio);

				case eConnectionType.Video:
					m_NvxControl.VideoSource = eSfpVideoSourceTypes.Disable;
					return SetActiveInput(null, eConnectionType.Video);

				default:
					throw new ArgumentOutOfRangeException("type", string.Format("Unexpected value {0}", type));
			}
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

		#endregion

#if SIMPLSHARP
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

			UpdateAudioRouting();
			UpdateVideoRouting();
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
			streamer.SecondaryAudio.SecondaryAudioChange += SecondaryAudioOnSecondaryAudioChange;
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
			streamer.SecondaryAudio.SecondaryAudioChange -= SecondaryAudioOnSecondaryAudioChange;
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
							: m_Streamer.SecondaryAudio.MulticastAddressFeedback.StringValue;
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
							: m_NvxControl.ServerUrlFeedback.StringValue;
					break;

				case DMInputEventIds.MulticastAddressEventId:
					MulticastAddress =
						m_NvxControl == null
							? null
							: m_NvxControl.MulticastAddressFeedback.StringValue;
					break;

				case DMInputEventIds.AudioSourceEventId:
				case DMInputEventIds.ActiveAudioSourceEventId:
					UpdateAudioRouting();
					break;

				case DMInputEventIds.VideoSourceEventId:
				case DMInputEventIds.ActiveVideoSourceEventId:
					UpdateVideoRouting();
					break;
				/*
				case DMInputEventIds.ElapsedSecEventId:
					break;

				default:
					IcdConsole.PrintLine(eConsoleColor.Magenta, "{0} - {1}", this, args.EventId);
					break;
				 */
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
				m_Streamer.SecondaryAudio.StatusFeedback !=
				Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass.DmNvx35xSecondaryAudio.eDeviceStatus.StreamStopped;

			if (audioSource == DmNvxControl.eAudioSource.SecondaryStreamAudio && !secondaryAudioEnabled)
			{
				SetActiveInput(null, eConnectionType.Audio);
				return;
			}

			int audioInput;
			if (s_AudioSources.TryGetKey(audioSource, out audioInput))
				SetActiveInput(audioInput, eConnectionType.Audio);
			else
				SetActiveInput(null, eConnectionType.Audio);
		}

		/// <summary>
		/// Updates the cached state of the video routing to match the device.
		/// </summary>
		private void UpdateVideoRouting()
		{
			eSfpVideoSourceTypes videoSource =
				m_NvxControl == null
					? eSfpVideoSourceTypes.Disable
					: m_NvxControl.VideoSourceFeedback;

			int videoInput;
			if (s_VideoSources.TryGetKey(videoSource, out videoInput))
				SetActiveInput(videoInput, eConnectionType.Video);
			else
				SetActiveInput(null, eConnectionType.Video);
		}

		#endregion

#endif

		#region SwitcherCache Callbacks

		private void CacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs eventArgs)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(eventArgs));
		}

		private void CacheOnRouteChange(object sender, RouteChangeEventArgs eventArgs)
		{
			OnRouteChange.Raise(this, new RouteChangeEventArgs(eventArgs));
		}

		private void CacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs eventArgs)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(eventArgs));
		}

		private void CacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs eventArgs)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(eventArgs));
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
