﻿using ICD.Common.Properties;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Microphone;
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

	public static class Dmps3XmlUtils
	{
		public static IEnumerable<IDeviceControl> GetControlsFromXml(string xml, ControlSystemDevice parent)
		{
			foreach (string childElement in XmlUtils.GetChildElementsAsString(xml))
			{
				IDeviceControl output;

				try
				{
					output = InstantiateControlFromXml(childElement, parent);
				}
				catch (Exception e)
				{
					parent.Log(eSeverity.Error, "Failed to instantiate control from XML - {0}", e.Message);
					continue;
				}

				yield return output;
			}
		}

		[NotNull]
		private static IDeviceControl InstantiateControlFromXml(string controlElement, ControlSystemDevice parent)
		{
			int id = XmlUtils.GetAttributeAsInt(controlElement, "id");
			eDmps3ControlType type = XmlUtils.GetAttributeAsEnum<eDmps3ControlType>(controlElement, "type", true);
			string name = XmlUtils.GetAttribute(controlElement, "name");

			switch (type)
			{
				case eDmps3ControlType.Volume:
				{
					IDmps3Crosspoint crosspoint = InstantiateCrosspointFromXml(controlElement, parent);
					Dmps3CrosspointVolumeControl output = new Dmps3CrosspointVolumeControl(parent, id, name, crosspoint);
					SetVolumeDefaultsFromXml(output, controlElement);
					return output;
				}

				case eDmps3ControlType.Microphone:
				{
					uint micInputId = XmlUtils.ReadChildElementContentAsUint(controlElement, "Address");
					Dmps3MicrophoneDeviceControl output = new Dmps3MicrophoneDeviceControl(parent, id, name, micInputId);
					SetMicrophoneDefaultsFromXml(output, controlElement);
					return output;
				}

				default:
				{
					string message = string.Format("{0} is not a valid Dmps3 control type", type);
					throw new FormatException(message);
				}
			}
		}

		[NotNull]
		private static IDmps3Crosspoint InstantiateCrosspointFromXml(string controlElement, ControlSystemDevice parent)
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

		private static void SetVolumeDefaultsFromXml(Dmps3CrosspointVolumeControl control, string controlElement)
		{
			bool? defaultMute = XmlUtils.TryReadChildElementContentAsBoolean(controlElement, "DefaultMute");
			float? defaultLevel = XmlUtils.TryReadChildElementContentAsFloat(controlElement, "DefaultLevel");

			if (defaultLevel.HasValue)
				control.SetVolumeLevel(defaultLevel.Value);

			if (defaultMute.HasValue)
				control.SetVolumeMute(defaultMute.Value);
		}

		private static void SetMicrophoneDefaultsFromXml(Dmps3MicrophoneDeviceControl control, string controlElement)
		{
			bool? defaultMute = XmlUtils.TryReadChildElementContentAsBoolean(controlElement, "DefaultMute");
			float? defaultGain = XmlUtils.TryReadChildElementContentAsFloat(controlElement, "DefaultGain");
			bool? defaultPower = XmlUtils.TryReadChildElementContentAsBoolean(controlElement, "DefaultPower");

			if (defaultMute.HasValue)
				control.SetMuted(defaultMute.Value);

			if (defaultGain.HasValue)
				control.SetGainLevel(defaultGain.Value);

			if (defaultPower.HasValue)
				control.SetPhantomPower(defaultPower.Value);
		}
	}
}