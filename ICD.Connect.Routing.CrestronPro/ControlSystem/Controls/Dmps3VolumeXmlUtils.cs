using ICD.Common.Properties;
using ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Input.Microphone;
using ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls
{
	public enum eDmps3OutputType
	{
		HdmiDm,
		Program,
		AuxOut1,
		AuxOut2
	}

	public enum eDmps3InputType
	{
		Master,
		Microphone,
		MicrophoneMaster,
		Source,
		Codec1,
		Codec2
	}

	public enum eDmps3ControlType
	{
		Volume,
		Microphone
	}

	public static class Dmps3VolumeXmlUtils
	{
		public static IEnumerable<IDeviceControl> GetControlsFromXml(string xml, ControlSystemDevice parent)
		{
			return XmlUtils.GetChildElementsAsString(xml)
				.Select(controlElement => InstantiateControlFromXml(controlElement, parent));
		}

		[NotNull]
		public static IDeviceControl InstantiateControlFromXml(string controlElement, ControlSystemDevice parent)
		{
			int id = XmlUtils.GetAttributeAsInt(controlElement, "id");
			eDmps3ControlType type = XmlUtils.GetAttributeAsEnum<eDmps3ControlType>(controlElement, "type", true);
			string name = XmlUtils.TryReadChildElementContentAsString(controlElement, "Name");

			switch (type)
			{
				case eDmps3ControlType.Volume:
					IDmps3Crosspoint crosspoint = InstantiateCrosspointFromXml(controlElement, parent);
					Dmps3CrosspointVolumeControl output = new Dmps3CrosspointVolumeControl(parent, id, name, crosspoint);
					SetVolumeDefaultsFromXml(output, controlElement);
					return output;

				case eDmps3ControlType.Microphone:
					uint micInputId = XmlUtils.ReadChildElementContentAsUint(controlElement, "Address");
					return new Dmps3MicrophoneDeviceControl(parent, id, name, micInputId, controlElement);
				
				default:
				{
					string message = string.Format("{0} is not a valid Dmps3 control type", type);
					throw new FormatException(message);
				}
			}
		}

		[NotNull]
		public static IDmps3Crosspoint InstantiateCrosspointFromXml(string controlElement, ControlSystemDevice parent)
		{
			eDmps3OutputType outputType = XmlUtils.ReadChildElementContentAsEnum<eDmps3OutputType>(controlElement, "OutputType",
				true);
			eDmps3InputType inputType = XmlUtils.TryReadChildElementContentAsEnum<eDmps3InputType>(controlElement, "InputType",
				true) ?? eDmps3InputType.Master;
			uint outputAddress = XmlUtils.TryReadChildElementContentAsUInt(controlElement, "OutputAddress") ?? 0;
			uint inputAddress = XmlUtils.TryReadChildElementContentAsUInt(controlElement, "InputAddress") ?? 0;

			switch (outputType)
			{
#if SIMPLSHARP
				case eDmps3OutputType.HdmiDm:

					Card.Dmps3OutputBase card = parent.ControlSystem.SwitcherOutputs[outputAddress] as Card.Dmps3OutputBase;
					bool outputBackend = card is Card.Dmps3DmOutputBackend || card is Card.Dmps3HdmiOutputBackend;
					if (outputBackend)
						return new Dmps3HdmiDmBackEndCrosspoint(parent, card, inputType, inputAddress);

					return new Dmps3HdmiDmCrosspoint(parent, card, inputType, inputAddress);

				case eDmps3OutputType.Program:
					ICardInputOutputType progOutCard =
						parent.ControlSystem.SwitcherOutputs.Values.FirstOrDefault(
							r => r.CardInputOutputType == eCardInputOutputType.Dmps3ProgramOutput);
					Card.Dmps3ProgramOutput progOut = progOutCard as Card.Dmps3ProgramOutput;
					return new Dmps3ProgramCrosspoint(parent, progOut, inputType, inputAddress);

				case eDmps3OutputType.AuxOut1:
					ICardInputOutputType auxOut1Card =
						parent.ControlSystem.SwitcherOutputs.Values.FirstOrDefault(
							r => r.CardInputOutputType == eCardInputOutputType.Dmps3Aux1Output);
					Card.Dmps3Aux1Output auxOut1 = auxOut1Card as Card.Dmps3Aux1Output;
					return new Dmps3AuxOut1Crosspoint(parent, auxOut1, inputType, inputAddress);

				case eDmps3OutputType.AuxOut2:
					ICardInputOutputType auxOut2Card =
						parent.ControlSystem.SwitcherOutputs.Values.FirstOrDefault(
							r => r.CardInputOutputType == eCardInputOutputType.Dmps3Aux2Output);
					Card.Dmps3Aux2Output auxOut2 = auxOut2Card as Card.Dmps3Aux2Output;
					return new Dmps3AuxOut2Crosspoint(parent, auxOut2, inputType, inputAddress);
#endif
				default:
					string message = string.Format("{0} is not a valid Dmps3 crosspoint type", outputType);
					throw new FormatException(message);
			}
		}

		public static void SetVolumeDefaultsFromXml(Dmps3CrosspointVolumeControl control, string controlElement)
		{
			bool? defaultMute = XmlUtils.TryReadChildElementContentAsBoolean(controlElement, "DefaultMute");
			short? defaultLevel = XmlUtils.TryReadChildElementContentAsShort(controlElement, "DefaultLevel");

			if (defaultLevel.HasValue)
				control.SetVolumeLevel(defaultLevel.Value);

			if (defaultMute.HasValue)
				control.SetVolumeMute(defaultMute.Value);
		}
	}
}
