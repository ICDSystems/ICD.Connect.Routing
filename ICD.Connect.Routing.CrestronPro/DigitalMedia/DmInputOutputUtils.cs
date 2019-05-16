using System;
using System.Collections.Generic;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Blades;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using SimplSharpProDM;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia
{
	public static class DmInputOutputUtils
	{
		#region Input Information
		private static readonly Dictionary<Type, InputOutputInformation> s_InputInformationByType =
			new Dictionary<Type, InputOutputInformation>
			{
				{
					typeof(DMInput), new InputOutputInformation
					{
						GetLabel = i => "DM",
						GetSignalType = i => eConnectionType.None,
						GetResolution = ResolutionUnsupported
					}
				},
				{
					typeof(Card.DMICard), new InputOutputInformation
					{
						GetLabel = GetLabelForInputCard,
						GetSignalType = GetSignalTypeForInputCard,
						GetResolution = ResolutionInputCard
					}
				},
				{
					typeof(Card.Dmps3AirMediaInput), new InputOutputInformation
					{
						GetLabel = i => "AirMedia",
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmps3AirMediaInput
					}
				},
				{
					typeof(Card.Dmps3AirMediaNoStreamingInput), new InputOutputInformation
					{
						GetLabel = i => "AirMedia",
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmps3AirMediaNoStreamingInput
					}
				},
				{
					typeof(Card.Dmps3AnalogAudioInput), new InputOutputInformation
					{
						GetLabel = i => "Analog Audio",
						GetSignalType = i => eConnectionType.Audio,
						GetResolution = ResolutionUnsupported
					}
				},
				{
					typeof(Card.Dmps3DmInput), new InputOutputInformation
					{
						GetLabel = i => "DM",
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmps3DmInput
					}
				},
				{
					typeof(Card.Dmps3HdmiInputWithoutAnalogAudio), new InputOutputInformation
					{
						GetLabel = i => "HDMI",
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmps3HdmiInputWithoutAnalogAudio
					}
				},
				{
					typeof(Card.DmMd4kHdmiInput), new InputOutputInformation
					{
						GetLabel = i => "HDMI",
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmps3HdmiInputWithoutAnalogAudio
					}
				},
				{
					typeof(Card.Dmps3HdmiInput), new InputOutputInformation
					{
						GetLabel = i => "HDMI",
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmps3HdmiInputWithoutAnalogAudio
					}
				},
				{
					typeof(Card.Dmps3HdmiVgaBncInput), new InputOutputInformation
					{
						GetLabel = i => "HDMI+VGA+BNC",
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmps3HdmiVgaBncInput
					}
				},
				{
					typeof(Card.Dmps3HdmiVgaInput), new InputOutputInformation
					{
						GetLabel = i => "HDMI+VGA",
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmps3HdmiVgaInput
					}
				},
				{
					typeof(Card.Dmps3SipInput), new InputOutputInformation
					{
						GetLabel = i => "SIP",
						GetSignalType = i => eConnectionType.Audio,
						GetResolution = ResolutionUnsupported
					}
				},
				{
					typeof(Card.Dmps3StreamingReceive), new InputOutputInformation
					{
						GetLabel = i => "StreamingRx",
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionStreamingRecieve
					}
				},
				{
					typeof(Card.Dmps3VgaInput), new InputOutputInformation
					{
						GetLabel = i => "VGA",
						GetSignalType = i => eConnectionType.Video,
						GetResolution = ResolutionDmps3VgaInput
					}
				},
				{
					typeof(Card.DmMd4kVgaInput), new InputOutputInformation
					{
						GetLabel = i => "VGA",
						GetSignalType = i => eConnectionType.Video,
						GetResolution = ResolutionDmMd4kVgaInput
					}
				},
				{
					typeof(Card.DmsBncAnalogSpdifAdvanced), new InputOutputInformation
					{
						GetLabel = i => "BNC+Analog+SPDIF",
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmsBncAnalogSpdifAdvanced
					}
				},
				{
					typeof(Card.DmsCatHdAdvanced), new InputOutputInformation
					{
						GetLabel = i => "DM Cat",
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmsCatHdAdvanced
					}
				},
				{
					typeof(Card.DmsHdmiInAdvanced), new InputOutputInformation
					{
						GetLabel = i => "HDMI",
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmsHdmiInAdvanced
					}
				},
				{
					typeof(Card.DmsHdmiInUhpAdvanced), new InputOutputInformation
					{
						GetLabel = i => "HDMI",
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmsHdmiInUhpAdvanced
					}
				},
				{
					typeof(Card.DmsHdmiRxAdvanced), new InputOutputInformation
					{
						GetLabel = i => "HDMI", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmsHdmiRxAdvanced
					}
				},
				{
					typeof(Card.DmsVgaAnalogAdvanced), new InputOutputInformation
					{
						GetLabel = i => "VGA", 
						GetSignalType = i => eConnectionType.Video,
						GetResolution = ResolutionDmsVgaAnalogAdvanced
					}
				},
				{
					typeof(HdMdNxMHdmiInput), new InputOutputInformation 
					{
						GetLabel = i => "HDMI", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionHdMdNxMHdmiInput
					}
				},
				{
					typeof(HdMdNxMVgaInput), new InputOutputInformation
					{
						GetLabel = i => "VGA", 
						GetSignalType = i => eConnectionType.Video,
						GetResolution = ResolutionHdMdNxMVgaInput
					}
				},
				{
					typeof(Card.HdmiRxAdvanced), new InputOutputInformation
					{
						GetLabel = i => "HDMI", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionHdmiRxAdvanced
					}
				}
			};

		#endregion 

		#region Card Information

		private static readonly Dictionary<Type, CardInformation> s_CardInformationByType =
			new Dictionary<Type, CardInformation>
			{
				{
					typeof(DmC4kInputBladeCard), new CardInformation
					{
						GetLabel = i => "DM", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmC4kInputBladeCard
					}
				},
				{
					typeof(DmHdmi4kInputBladeCard), new CardInformation
					{
						GetLabel = i => "HDMI", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio ,
						GetResolution = ResolutionDmHdmi4kInputBladeCard
					}
				},
				{
					typeof(Dmc4kC), new CardInformation
					{
						GetLabel = i => "DM", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmc4kCBase
					}
				},
				{
					typeof(Dmc4kzC), new CardInformation
					{
						GetLabel = i => "DM", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmc4kCBase
					}
				},
				{
					typeof(Dmc4kCDsp), new CardInformation
					{
						GetLabel = i => "DM", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmc4kCDspBase
					}
				},
				{
					typeof(Dmc4kzCDsp), new CardInformation
					{
						GetLabel = i => "DM", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmc4kCDspBase
					}
				},
				{
					typeof(Dmc4kHd), new CardInformation
					{
						GetLabel = i => "HDMI", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmc4kHdBase
					}
				},
				{
					typeof(Dmc4kzHd), new CardInformation
					{
						GetLabel = i => "HDMI", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmc4kHdBase
					}
				},
				{
					typeof(Dmc4kHdDsp), new CardInformation
					{
						GetLabel = i => "HDMI", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmc4kHdDspBase
					}
				},
				{
					typeof(Dmc4kzHdDsp), new CardInformation
					{
						GetLabel = i => "HDMI", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmc4kHdDspBase
					}
				},
				{
					typeof(DmcC), new CardInformation
					{
						GetLabel = i => "DM", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmcC
					}
				},
				{
					typeof(DmcCat), new CardInformation
					{
						GetLabel = i => "DM CAT", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmcCat
					}
				},
				{
					typeof(DmcCatDsp), new CardInformation
					{
						GetLabel = i => "DM CAT", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmcCatDsp
					}
				},
				{
					typeof(DmcCDsp), new CardInformation
					{
						GetLabel = i => "DM", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmcCDsp
					}
				},
				{
					typeof(DmcDvi), new CardInformation
					{
						GetLabel = i => "DVI", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmcDvi
					}
				},
				{
					typeof(DmcF), new CardInformation
					{
						GetLabel = i => "DM Fiber", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmcF
					}
				},
				{
					typeof(DmcFDsp), new CardInformation
					{
						GetLabel = i => "DM Fiber", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmcFDsp
					}
				},
				{
					typeof(DmcHd), new CardInformation
					{
						GetLabel = i => "HDMI", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmcHd
					}
				},
				{
					typeof(DmcHdDsp), new CardInformation
					{
						GetLabel = i => "HDMI", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmcHdDsp
					}
				},
				{
					typeof(DmcS), new CardInformation
					{
						GetLabel = i => "DM 8G Fiber", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmcS
					}
				},
				{
					typeof(DmcSDsp), new CardInformation
					{
						GetLabel = i => "", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmcSDsp
					}
				},
				{
					typeof(DmcS2), new CardInformation
					{
						GetLabel = i => "DM 8G Fiber", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmcS2Base
					}
				},
				{
					typeof(DmcS2Dsp), new CardInformation
					{
						GetLabel = i => "DM 8G Fiber", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmcS2Base
					}
				},
				{
					typeof(DmcSdi), new CardInformation
					{
						GetLabel = i => "SDI", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmcSdi
					}
				},
				{
					typeof(DmcStr), new CardInformation
					{
						GetLabel = i => "Streaming", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmcStr
					}
				},
				{
					typeof(DmcStrCresnet), new CardInformation
					{
						GetLabel = i => "Streaming", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb,
						GetResolution = ResolutionDmcStr
					}
				},
				{
					typeof(DmcVga), new CardInformation
					{
						GetLabel = i => "VGA", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmcVga
					}
				},
				{
					typeof(DmcVid4), new CardInformation
					{
						GetLabel = i => "Composite", 
						GetSignalType = i => eConnectionType.Video,
						GetResolution = ResolutionUnsupported
					}
				},
				{
					typeof(DmcVidBnc), new CardInformation
					{
						GetLabel = i => "Bnc", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmcVidBase
					}
				},
				{
					typeof(DmcVidRcaA), new CardInformation
					{
						GetLabel = i => "RCA", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmcVidBase
					}
				},
				{
					typeof(DmcVidRcaD), new CardInformation
					{
						GetLabel = i => "RCA", 
						GetSignalType = i => eConnectionType.Video | eConnectionType.Audio,
						GetResolution = ResolutionDmcVidBase
					}
				},
			};

		private static string GetLabelForInputCard(DMInputOutputBase input)
		{
			if (input.Card == null)
				return "Unsupported";

			return s_CardInformationByType[input.Card.GetType()].GetLabel(input.Card);
		}

		private static eConnectionType GetSignalTypeForInputCard(DMInputOutputBase input)
		{
			if (input.Card == null)
				return eConnectionType.None;

			return s_CardInformationByType[input.Card.GetType()].GetSignalType(input.Card);
		}

		private static string ResolutionInputCard(DMInputOutputBase input)
		{
			if (input.Card == null)
				return "Unsupported";

			return s_CardInformationByType[input.Card.GetType()].GetResolution(input.Card);
		}
		#endregion

		#region Input Resolution Delegates

		private static string ResolutionUnsupported(object input)
		{
			return "Unsupported";
		}

		private static string ResolutionDmps3AirMediaInput(DMInputOutputBase input)
		{
			Card.Dmps3AirMediaInput castInput = (Card.Dmps3AirMediaInput)input;
			ushort h = castInput.StreamingInputPort.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.StreamingInputPort.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmps3AirMediaNoStreamingInput(DMInputOutputBase input)
		{
			Card.Dmps3AirMediaNoStreamingInput castInput = (Card.Dmps3AirMediaNoStreamingInput)input;
			ushort h;
			ushort v;
			switch (castInput.OutputResolutionFeedback)
			{
				case eAirMediaVideoResolution.Resolution800x600:
					h = 800;
					v = 600;
					break;
				case eAirMediaVideoResolution.Resolution1024x768:
					h = 1024;
					v = 768;
					break;
				case eAirMediaVideoResolution.Resolution1280x720:
					h = 1280;
					v = 720;
					break;
				case eAirMediaVideoResolution.Resolution1280x768:
					h = 1280;
					v = 768;
					break;
				case eAirMediaVideoResolution.Resolution1280x800:
					h = 1280;
					v = 800;
					break;
				case eAirMediaVideoResolution.Resolution1360x768:
					h = 1360;
					v = 768;
					break;
				case eAirMediaVideoResolution.Resolution1440x900:
					h = 1440;
					v = 900;
					break;
				case eAirMediaVideoResolution.Resolution1600x1200:
					h = 1600;
					v = 1200;
					break;
				case eAirMediaVideoResolution.Resolution1920x1080i:
					h = 1920;
					v = 1080;
					break;
				case eAirMediaVideoResolution.Resolution1920x1080p:
					h = 1920;
					v = 1080;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmps3DmInput(DMInputOutputBase input)
		{
			Dmps3DmInputPort port = ((Card.Dmps3DmInput)input).DmInputPort;
			ushort h = port.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = port.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmps3HdmiInputWithoutAnalogAudio(DMInputOutputBase input)
		{
			Card.Dmps3HdmiInputWithoutAnalogAudio cast = (Card.Dmps3HdmiInputWithoutAnalogAudio)input;
			ushort h = cast.HdmiInputPort.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = cast.HdmiInputPort.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmps3HdmiVgaBncInput(DMInputOutputBase input)
		{
			Card.Dmps3HdmiVgaBncInput cast = (Card.Dmps3HdmiVgaBncInput)input;
			ushort h;
			ushort v;
			switch (cast.VideoSourceFeedback)
			{
				case eDmps3InputVideoSource.Auto:
					GetComboPortAutoResolution(cast, out h, out v);
					break;
				case eDmps3InputVideoSource.Hdmi:
					GetComboPortHdmiResolution(cast, out h, out v);
					break;
				case eDmps3InputVideoSource.Vga:
					GetComboPortVgaResolution(cast, out h, out v);
					break;
				case eDmps3InputVideoSource.Bnc:
					GetComboPortBncResolution(cast, out h, out v);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return GetResolutionFormatted(h, v);
		}

		#region Dmps3HdmiVgaBncInput Helpers

		private static void GetComboPortAutoResolution(Card.Dmps3HdmiVgaBncInput cast, out ushort h, out ushort v)
		{
			if (cast.HdmiSyncDetected.BoolValue)
				GetComboPortHdmiResolution(cast, out h, out v);
			else if (cast.VgaSyncDetectedFeedback.BoolValue)
				GetComboPortVgaResolution(cast, out h, out v);
			else if (cast.BncSyncDetected.BoolValue)
				GetComboPortBncResolution(cast, out h, out v);
			else
			{
				h = 0;
				v = 0;
			}
		}

		private static void GetComboPortBncResolution(Card.Dmps3HdmiVgaBncInput cast, out ushort h, out ushort v)
		{
			h = cast.BncInputPort.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			v = cast.BncInputPort.VideoAttributes.VerticalResolutionFeedback.UShortValue;
		}

		private static void GetComboPortVgaResolution(Card.Dmps3HdmiVgaBncInput cast, out ushort h, out ushort v)
		{
			h = cast.VgaInputPort.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			v = cast.VgaInputPort.VideoAttributes.VerticalResolutionFeedback.UShortValue;
		}

		private static void GetComboPortHdmiResolution(Card.Dmps3HdmiVgaBncInput cast, out ushort h, out ushort v)
		{
			h = cast.HdmiInputPort.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			v = cast.HdmiInputPort.VideoAttributes.VerticalResolutionFeedback.UShortValue;
		}

		#endregion

		private static string ResolutionDmps3HdmiVgaInput(DMInputOutputBase input)
		{
			Card.Dmps3HdmiVgaInput cast = (Card.Dmps3HdmiVgaInput)input;
			ushort h;
			ushort v;
			switch (cast.VideoSourceFeedback)
			{
				case eDmps3InputVideoSource.Auto:
					GetComboPortAutoResolution(cast, out h, out v);
					break;
				case eDmps3InputVideoSource.Hdmi:
					GetComboPortHdmiResolution(cast, out h, out v);
					break;
				case eDmps3InputVideoSource.Vga:
					GetComboPortVgaResolution(cast, out h, out v);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return GetResolutionFormatted(h, v);
		}

		#region Dmps3HdmiVgaInput Helpers

		private static void GetComboPortAutoResolution(Card.Dmps3HdmiVgaInput cast, out ushort h, out ushort v)
		{
			if (cast.HdmiSyncDetected.BoolValue)
				GetComboPortHdmiResolution(cast, out h, out v);
			else if (cast.VgaSyncDetectedFeedback.BoolValue)
				GetComboPortVgaResolution(cast, out h, out v);
			else
			{
				h = 0;
				v = 0;
			}
		}

		private static void GetComboPortVgaResolution(Card.Dmps3HdmiVgaInput cast, out ushort h, out ushort v)
		{
			h = cast.VgaInputPort.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			v = cast.VgaInputPort.VideoAttributes.VerticalResolutionFeedback.UShortValue;
		}

		private static void GetComboPortHdmiResolution(Card.Dmps3HdmiVgaInput cast, out ushort h, out ushort v)
		{
			h = cast.HdmiInputPort.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			v = cast.HdmiInputPort.VideoAttributes.VerticalResolutionFeedback.UShortValue;
		}

		#endregion

		private static string ResolutionStreamingRecieve(DMInputOutputBase input)
		{
			Card.Dmps3StreamingReceive cast = (Card.Dmps3StreamingReceive)input;
			const string propertyName = "StreamingInputPort";

			PropertyInfo property = cast.GetType()
			                            .GetCType()
			                            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
			StreamingPort port = property != null ? property.GetValue(cast, null) as StreamingPort : null;

			if (port == null)
				return "Unsupported";

			ushort h = port.HorizontalResolutionFeedback.UShortValue;
			ushort v = port.VerticalResolutionFeedback.UShortValue;

			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmps3VgaInput(DMInputOutputBase input)
		{
			Card.Dmps3VgaInput cast = (Card.Dmps3VgaInput)input;
			ushort h = cast.VgaInputPort.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = cast.VgaInputPort.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmMd4kVgaInput(DMInputOutputBase input)
		{
			Card.DmMd4kVgaInput cast = (Card.DmMd4kVgaInput)input;
			ushort h = cast.VgaInputPort.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = cast.VgaInputPort.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmsBncAnalogSpdifAdvanced(DMInputOutputBase input)
		{
			Card.DmsBncAnalogSpdifAdvanced castInput = (Card.DmsBncAnalogSpdifAdvanced)input;
			ushort h = castInput.BncInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.BncInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmsCatHdAdvanced(DMInputOutputBase input)
		{
			Card.DmsCatHdAdvanced castInput = (Card.DmsCatHdAdvanced)input;
			ushort h = castInput.DmInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.DmInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmsHdmiInAdvanced(DMInputOutputBase input)
		{
			Card.DmsHdmiInAdvanced castInput = (Card.DmsHdmiInAdvanced)input;
			ushort h = castInput.HdmiInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.HdmiInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmsHdmiInUhpAdvanced(DMInputOutputBase input)
		{
			Card.DmsHdmiInUhpAdvanced castInput = (Card.DmsHdmiInUhpAdvanced)input;
			ushort h = castInput.HdmiInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.HdmiInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmsHdmiRxAdvanced(DMInputOutputBase input)
		{
			Card.DmsHdmiRxAdvanced castInput = (Card.DmsHdmiRxAdvanced)input;
			ushort h = castInput.HdmiInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.HdmiInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}


		private static string ResolutionDmsVgaAnalogAdvanced(DMInputOutputBase input)
		{
			Card.DmsVgaAnalogAdvanced castInput = (Card.DmsVgaAnalogAdvanced)input;
			ushort h = castInput.VgaInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.VgaInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionHdMdNxMHdmiInput(DMInputOutputBase input)
		{
			HdMdNxMHdmiInput castInput = (HdMdNxMHdmiInput)input;
			ushort h = castInput.HdmiInputPort.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.HdmiInputPort.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionHdMdNxMVgaInput(DMInputOutputBase input)
		{
			HdMdNxMVgaInput castInput = (HdMdNxMVgaInput)input;
			ushort h = castInput.VgaInputPort.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.VgaInputPort.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionHdmiRxAdvanced(DMInputOutputBase input)
		{
			Card.HdmiRxAdvanced castInput = (Card.HdmiRxAdvanced)input;
			ushort h = castInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmC4kInputBladeCard(CardDevice input)
		{
			var castInput = (DmC4kInputBladeCard)input;
			ushort h = castInput.DmInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.DmInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmHdmi4kInputBladeCard(CardDevice input)
		{
			var castInput = (DmHdmi4kInputBladeCard)input;
			ushort h = castInput.Hdmi4kInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.Hdmi4kInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmc4kCBase(CardDevice input)
		{
			var castInput = (Dmc4kCBase)input;
			ushort h = castInput.DmInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.DmInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmc4kCDspBase(CardDevice input)
		{
			var castInput = (Dmc4kCDspBase)input;
			ushort h = castInput.DmInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.DmInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmc4kHdBase(CardDevice input)
		{
			var castInput = (Dmc4kHdBase)input;
			ushort h = castInput.HdmiInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.HdmiInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmc4kHdDspBase(CardDevice input)
		{
			var castInput = (Dmc4kHdDspBase)input;
			ushort h = castInput.HdmiInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.HdmiInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmcC(CardDevice input)
		{
			var castInput = (DmcC)input;
			ushort h = castInput.DmInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.DmInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmcCatDsp(CardDevice input)
		{
			var castInput = (DmcCatDsp)input;
			ushort h = castInput.DmInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.DmInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmcCDsp(CardDevice input)
		{
			var castInput = (DmcCDsp)input;
			ushort h = castInput.DmInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.DmInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmcDvi(CardDevice input)
		{
			var castInput = (DmcDvi)input;
			ushort h = castInput.DviInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.DviInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmcCat(CardDevice input)
		{
			var castInput = (DmcCat)input;
			ushort h = castInput.DmInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.DmInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmcF(CardDevice input)
		{
			var castInput = (DmcF)input;
			ushort h = castInput.DmInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.DmInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmcFDsp(CardDevice input)
		{
			var castInput = (DmcFDsp)input;
			ushort h = castInput.DmInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.DmInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmcHd(CardDevice input)
		{
			var castInput = (DmcHd)input;
			ushort h = castInput.HdmiInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.HdmiInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmcHdDsp(CardDevice input)
		{
			var castInput = (DmcHdDsp)input;
			ushort h = castInput.HdmiInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.HdmiInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmcS(CardDevice input)
		{
			var castInput = (DmcS)input;
			ushort h = castInput.DmInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.DmInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmcSDsp(CardDevice input)
		{
			var castInput = (DmcSDsp)input;
			ushort h = castInput.DmInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.DmInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmcS2Base(CardDevice input)
		{
			var castInput = (DmcSBase)input;
			ushort h = castInput.DmInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.DmInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmcSdi(CardDevice input)
		{
			var castInput = (DmcSdi)input;
			ushort h = castInput.SdiInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.SdiInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmcStr(CardDevice input)
		{
			var castInput = (DmcStr)input;
			ushort h;
			ushort v;
			switch (castInput.ResolutionFeedback)
			{
				case DmcStr.eScreenResolutions.Auto:
				case DmcStr.eScreenResolutions.None:
					h = 0;
					v = 0;
					break;
				case DmcStr.eScreenResolutions.Resolution640x480At60Hz:
					h = 640;
					v = 480;
					break;
				case DmcStr.eScreenResolutions.Resolution480i:
				case DmcStr.eScreenResolutions.Resolution480p:
					h = 640;
					v = 480;
					break;
				case DmcStr.eScreenResolutions.Resolution576i:
				case DmcStr.eScreenResolutions.Resolution576p:
					h = 768;
					v = 576;
					break;
				case DmcStr.eScreenResolutions.Resolution800x600At60Hz:
					h = 800;
					v = 600;
					break;
				case DmcStr.eScreenResolutions.Resolution848x480At60Hz:
					h = 848;
					v = 480;
					break;
				case DmcStr.eScreenResolutions.Resolution1024x768At60Hz:
					h = 1024;
					v = 768;
					break;
				case DmcStr.eScreenResolutions.Resolution720p50Hz:
				case DmcStr.eScreenResolutions.Resolution720p60Hz:
					h = 1280;
					v = 720;
					break;
				case DmcStr.eScreenResolutions.Resolution1280x768At60Hz:
				case DmcStr.eScreenResolutions.Resolution1280x768At60HzRb:
					h = 1280;
					v = 768;
					break;
				case DmcStr.eScreenResolutions.Resolution1280x800At60Hz:
				case DmcStr.eScreenResolutions.Resolution1280x800At60HzRb:
					h = 1280;
					v = 800;
					break;
				case DmcStr.eScreenResolutions.Resolution1280x960At60Hz:
					h = 1280;
					v = 960;
					break;
				case DmcStr.eScreenResolutions.Resolution1280x1024At60Hz:
					h = 1280;
					v = 1024;
					break;
				case DmcStr.eScreenResolutions.Resolution1360x768At60Hz:
					h = 1360;
					v = 768;
					break;
				case DmcStr.eScreenResolutions.Resolution1366x768At60Hz:
				case DmcStr.eScreenResolutions.Resolution1366x768At60HzRb:
					h = 1366;
					v = 768;
					break;
				case DmcStr.eScreenResolutions.Resolution1400x1050At60Hz:
				case DmcStr.eScreenResolutions.Resolution1400x1050At60HzRb:
					h = 1400;
					v = 1050;
					break;
				case DmcStr.eScreenResolutions.Resolution1440x900At60Hz:
				case DmcStr.eScreenResolutions.Resolution1440x900At60HzRb:
					h = 1440;
					v = 900;
					break;
				case DmcStr.eScreenResolutions.Resolution1600x900At60HzRb:
				case DmcStr.eScreenResolutions.Resolution1600x1200At60Hz:
					h = 1600;
					v = 900;
					break;
				case DmcStr.eScreenResolutions.Resolution1680x720At24Hz:
				case DmcStr.eScreenResolutions.Resolution1680x720At25Hz:
				case DmcStr.eScreenResolutions.Resolution1680x720At30Hz:
				case DmcStr.eScreenResolutions.Resolution1680x720At50Hz:
				case DmcStr.eScreenResolutions.Resolution1680x720At60Hz:
					h = 1680;
					v = 720;
					break;
				case DmcStr.eScreenResolutions.Resolution1680x1050At60Hz:
				case DmcStr.eScreenResolutions.Resolution1680x1050At60HzRb:
					h = 1680;
					v = 1050;
					break;
				case DmcStr.eScreenResolutions.Resolution1792x1344At60Hz:
					h = 1792;
					v = 1344;
					break;
				case DmcStr.eScreenResolutions.Resolution1856x1392At60Hz:
					h = 1856;
					v = 1392;
					break;
				case DmcStr.eScreenResolutions.Resolution1080i25Hz:
				case DmcStr.eScreenResolutions.Resolution1080i30Hz:
				case DmcStr.eScreenResolutions.Resolution1080p24Hz:
				case DmcStr.eScreenResolutions.Resolution1080p50Hz:
				case DmcStr.eScreenResolutions.Resolution1080p60Hz:
				case DmcStr.eScreenResolutions.Resolution1920x1200At60HzRb:
					h = 1920;
					v = 1080;
					break;
				case DmcStr.eScreenResolutions.Resolution1920x1440At60Hz:
					h = 1920;
					v = 1440;
					break;
				case DmcStr.eScreenResolutions.Resolution2048x1080At24Hz:
				case DmcStr.eScreenResolutions.Resolution2048x1080At60Hz:
					h = 2048;
					v = 1080;
					break;
				case DmcStr.eScreenResolutions.Resolution2048x1152At60HzRb:
					h = 2048;
					v = 1150;
					break;
				case DmcStr.eScreenResolutions.Resolution2048x1536At60Hz:
					h = 2048;
					v = 1536;
					break;
				case DmcStr.eScreenResolutions.Resolution2560x1080At60HzPC:
				case DmcStr.eScreenResolutions.Resolution2560x1080At24Hz:
				case DmcStr.eScreenResolutions.Resolution2560x1080At25Hz:
				case DmcStr.eScreenResolutions.Resolution2560x1080At30Hz:
				case DmcStr.eScreenResolutions.Resolution2560x1080At50Hz:
				case DmcStr.eScreenResolutions.Resolution2560x1080At60Hz:
					h = 2560;
					v = 1080;
					break;
				case DmcStr.eScreenResolutions.Resolution2560x1440At60Hz:
					h = 2560;
					v = 1440;
					break;
				case DmcStr.eScreenResolutions.Resolution2560x1600At60HzRb:
					h = 2560;
					v = 1600;
					break;
				case DmcStr.eScreenResolutions.Resolution3840x2160At24Hz:
				case DmcStr.eScreenResolutions.Resolution3840x2160At25Hz:
				case DmcStr.eScreenResolutions.Resolution3840x2160At30Hz:
				case DmcStr.eScreenResolutions.Resolution3840x2160p50Hz:
				case DmcStr.eScreenResolutions.Resolution3840x2160p60Hz:
					h = 3840;
					v = 2160;
					break;
				case DmcStr.eScreenResolutions.Resolution4096x2160p24Hz:
				case DmcStr.eScreenResolutions.Resolution4096x2160p25Hz:
				case DmcStr.eScreenResolutions.Resolution4096x2160p30Hz:
				case DmcStr.eScreenResolutions.Resolution4096x2160p50Hz:
				case DmcStr.eScreenResolutions.Resolution4096x2160p60Hz:
				case DmcStr.eScreenResolutions.Resolution4096x2160p60HzVesaRbV2:
					h = 4096;
					v = 2160;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmcVga(CardDevice input)
		{
			var castInput = (DmcVga)input;
			ushort h = castInput.VgaInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.VgaInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		private static string ResolutionDmcVidBase(CardDevice input)
		{
			var castInput = (DmcVidBase)input;
			ushort h = castInput.VideoInput.VideoAttributes.HorizontalResolutionFeedback.UShortValue;
			ushort v = castInput.VideoInput.VideoAttributes.VerticalResolutionFeedback.UShortValue;
			return GetResolutionFormatted(h, v);
		}

		public static string GetResolutionFormatted(ushort h, ushort v)
		{
			if (h == 0 || v == 0)
				return string.Empty;
			return string.Format("{0} x {1}", h, v);
		}

		#endregion

		#region Output Information
		private static readonly Dictionary<Type, InputOutputInformation> s_OutputInformationByType =
			new Dictionary<Type, InputOutputInformation>
			{
				{
					typeof(Card.DMOCard), new InputOutputInformation
					{
						GetLabel = o => "DM",
						GetSignalType = o => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb
					}
				},
				{
					typeof(Card.DmHdmiOutput), new InputOutputInformation
					{
						GetLabel = o => "HDMI", 
						GetSignalType = o => eConnectionType.Video | eConnectionType.Audio,
					}
				},
				{
					typeof(Card.DmsCatOHd), new InputOutputInformation
					{
						GetLabel = o => "DM CAT",
						GetSignalType = o => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb
					}
				},
				{
					typeof(Card.DmsDmOutAdvanced), new InputOutputInformation
					{
						GetLabel = o => "DM",
						GetSignalType = o => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb
					}
				},
				{
					typeof(Card.Dmps3AecOutput), new InputOutputInformation
					{
						GetLabel = o => "AEC", 
						GetSignalType = o => eConnectionType.Audio
					}
				},
				{
					typeof(Card.Dmps3CodecOutput), new InputOutputInformation
					{
						GetLabel = o => "CODEC", 
						GetSignalType = o => eConnectionType.Audio
					}
				},
				{
					typeof(Card.Dmps3HdmiAudioOutput), new InputOutputInformation
					{
						GetLabel = o => "HDMI", 
						GetSignalType = o => eConnectionType.Video | eConnectionType.Audio
					}
				},
				{
					typeof(DmMd4kHdmiAudioOutput), new InputOutputInformation
					{
						GetLabel = o => "HDMI", 
						GetSignalType = o => eConnectionType.Video | eConnectionType.Audio
					}
				},
				{
					typeof(Card.Dmps3DmHdmiAudioOutput), new InputOutputInformation
					{
						GetLabel = o => "HDMI", 
						GetSignalType = o => eConnectionType.Video | eConnectionType.Audio
					}
				},
				{
					typeof(DmMd4kDmHdmiAudioOutput), new InputOutputInformation
					{
						GetLabel = o => "HDMI", 
						GetSignalType = o => eConnectionType.Video | eConnectionType.Audio
					}
				},
				{
					typeof(Card.Dmps3Aux1Output), new InputOutputInformation
					{
						GetLabel = o => "AUX", 
						GetSignalType = o => eConnectionType.Audio
					}
				},
				{
					typeof(Card.Dmps3Aux2Output), new InputOutputInformation
					{
						GetLabel = o => "AUX", 
						GetSignalType = o => eConnectionType.Audio
					}
				},
				{
					typeof(Card.Dmps3DialerOutput), new InputOutputInformation
					{
						GetLabel = o => "DIALER", 
						GetSignalType = o => eConnectionType.Audio
					}
				},
				{
					typeof(Card.Dmps3DigitalMixOutput), new InputOutputInformation
					{
						GetLabel = o => "DigitalMixOut", 
						GetSignalType = o => eConnectionType.Audio
					}
				},
				{
					typeof(Card.Dmps3DmOutput), new InputOutputInformation
					{
						GetLabel = o => "DM",
						GetSignalType = o => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb
					}
				},
				{
					typeof(Card.Dmps3DmOutputBackend), new InputOutputInformation
					{
						GetLabel = o => "DM",
						GetSignalType = o => eConnectionType.Video | eConnectionType.Audio | eConnectionType.Usb
					}
				},
				{
					typeof(Card.Dmps3HdmiOutput), new InputOutputInformation
					{
						GetLabel = o => "HDMI", 
						GetSignalType = o => eConnectionType.Video | eConnectionType.Audio
					}
				},
				{
					typeof(Card.Dmps3HdmiOutputBackend), new InputOutputInformation
					{
						GetLabel = o => "HDMI", 
						GetSignalType = o => eConnectionType.Video | eConnectionType.Audio
					}
				},
				{
					typeof(Card.Dmps3ProgramOutput), new InputOutputInformation
					{
						GetLabel = o => "PROGRAM", 
						GetSignalType = o => eConnectionType.Audio
					}
				},
				{
					typeof(Card.Dmps3StreamingTransmit), new InputOutputInformation
					{
						GetLabel = o => "StreamingTx", 
						GetSignalType = o => eConnectionType.Video | eConnectionType.Audio
					}
				},
				{
					typeof(HdMdNxMHdmiOutput), new InputOutputInformation 
					{
						GetLabel = o => "HDMI", 
						GetSignalType = o => eConnectionType.Video | eConnectionType.Audio
					}
				},
				{
					typeof(Card.HdmiTx), new InputOutputInformation
					{
						GetLabel = o => "HDMI", 
						GetSignalType = o => eConnectionType.Video | eConnectionType.Audio
					}
				}
			};

		#endregion

		public static string GetInputTypeStringForInput(DMInput input)
		{
			return s_InputInformationByType[input.GetType()].GetLabel(input);
		}

		public static bool GetIsInputVideoInput(DMInput input)
		{
			return s_InputInformationByType[input.GetType()].GetSignalType(input).HasFlag(eConnectionType.Video);
		}

		public static bool GetIsInputAudioInput(DMInput input)
		{
			return s_InputInformationByType[input.GetType()].GetSignalType(input).HasFlag(eConnectionType.Audio);
		}

		public static bool GetIsInputUsbInput(DMInput input)
		{
			return s_InputInformationByType[input.GetType()].GetSignalType(input).HasFlag(eConnectionType.Usb);
		}

		public static string GetResolutionStringForVideoInput(DMInput input)
		{
			return s_InputInformationByType[input.GetType()].GetResolution(input);
		}

		public static string GetOutputTypeStringForOutput(DMOutput output)
		{
			return s_OutputInformationByType[output.GetType()].GetLabel(output);
		}

		public static bool GetIsOutputVideoOutput(DMOutput output)
		{
			return s_OutputInformationByType[output.GetType()].GetSignalType(output).HasFlag(eConnectionType.Video);
		}

		public static bool GetIsOutputAudioOutput(DMOutput output)
		{
			return s_OutputInformationByType[output.GetType()].GetSignalType(output).HasFlag(eConnectionType.Audio);
		}

		public static bool GetIsOutputUsbOutput(DMOutput output)
		{
			return s_OutputInformationByType[output.GetType()].GetSignalType(output).HasFlag(eConnectionType.Usb);
		}

		public static bool GetIsEventIdResolutionEventId(int eventId)
		{
			return eventId == DMInputEventIds.ResolutionEventId
			       || eventId == DMInputEventIds.HorizontalResolutionFeedbackEventId
			       || eventId == DMInputEventIds.VerticalResolutionFeedbackEventId
			       || eventId == DMInputEventIds.EncodingResolutionFeedbackEventId
			       || eventId == DMInputEventIds.StreamHorizontalResolutionEventId
			       || eventId == DMInputEventIds.StreamVerticalResolutionEventId
			       || eventId == DMInputEventIds.AirMediaOutputResolutionEventId;
		}

		private sealed class InputOutputInformation
		{
			public delegate string GetLabelDelegate(DMInputOutputBase input);
			public delegate eConnectionType GetSignalTypeDelegate(DMInputOutputBase input);
			public delegate string GetResolutionDelegate(DMInputOutputBase input);

			public GetLabelDelegate GetLabel { get; set; }
			public GetSignalTypeDelegate GetSignalType { get; set; }
			public GetResolutionDelegate GetResolution { get; set; }
		}

		private sealed class CardInformation
		{
			public delegate string GetLabelDelegate(CardDevice input);
			public delegate eConnectionType GetSignalTypeDelegate(CardDevice input);
			public delegate string GetResolutionDelegate(CardDevice input);

			public GetLabelDelegate GetLabel { get; set; }
			public GetSignalTypeDelegate GetSignalType { get; set; }
			public GetResolutionDelegate GetResolution { get; set; }

		}
	}
}
