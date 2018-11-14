#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Input.Microphone;
using ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls
{
	public static class Dmps3VolumeXmlUtils
	{
		private static readonly IcdHashSet<string> s_OutputInputDefaultTypes = new IcdHashSet<string> { "Mic", "MicMaster", "Source", "Codec1", "Codec2", "VolMaster" };

		public static IEnumerable<IDeviceControl> GetControlsFromXml(string xml, ControlSystemDevice parent)
		{
			foreach (string controlElement in XmlUtils.GetChildElementsAsString(xml))
			{
				int id = XmlUtils.GetAttributeAsInt(controlElement, "id"); 
				string type = XmlUtils.GetAttributeAsString(controlElement, "type");
				string name = XmlUtils.TryReadChildElementContentAsString(controlElement, "Name");

				AbstractDmps3VolumeDeviceControl control = null;
				AbstractDmps3MicrophoneDeviceControl controlMicInput = null;

				switch (type)
				{
#if SIMPLSHARP					
					case "HdmiDm":
						uint outputAddress = XmlUtils.ReadChildElementContentAsUint(controlElement, "OutputAddress");
						Card.Dmps3OutputBase card = parent.ControlSystem.SwitcherOutputs[outputAddress] as Card.Dmps3OutputBase;
						bool outputBackend = card is Card.Dmps3DmOutputBackend || card is Card.Dmps3HdmiOutputBackend;
						if (outputBackend)
							control = new Dmps3HdmiDmBackEndVolumeDeviceControl(parent, id, name, outputAddress);
						else
							control = new Dmps3HdmiDmVolumeDeviceControl(parent, id, name, outputAddress);
					break;

					case "Program":
						int progOutIndex = parent.ControlSystem.SwitcherOutputs.Values.FindIndex(r => r.CardInputOutputType == eCardInputOutputType.Dmps3ProgramOutput);
						uint progOutId = parent.ControlSystem.SwitcherOutputs.Keys.ToArray()[progOutIndex];
						control = new Dmps3ProgramVolumeDeviceControl(parent, id, name, progOutId);
					break;

					case "AuxOut1":
						int auxOut1Index = parent.ControlSystem.SwitcherOutputs.Values.FindIndex(r => r.CardInputOutputType == eCardInputOutputType.Dmps3Aux1Output);
						uint auxOut1Id = parent.ControlSystem.SwitcherOutputs.Keys.ToArray()[auxOut1Index];
						control = new Dmps3AuxOut1VolumeDeviceControl(parent, id, name, auxOut1Id);
					break;

					case "AuxOut2":
						int auxOut2Index = parent.ControlSystem.SwitcherOutputs.Values.FindIndex(r => r.CardInputOutputType == eCardInputOutputType.Dmps3Aux2Output);
						uint auxOut2Id = parent.ControlSystem.SwitcherOutputs.Keys.ToArray()[auxOut2Index];
						control = new Dmps3AuxOut2VolumeDeviceControl(parent, id, name, auxOut2Id);
					break;

					case "MicInput":
						uint micInputId = XmlUtils.ReadChildElementContentAsUint(controlElement, "InputAddress");
						controlMicInput = new Dmps3MicrophoneDeviceControl(parent, id, name, micInputId);
					break;

#endif
					default:
						string message = string.Format("{0} is not a valid Dmps3 control type", type);
						throw new FormatException(message);
				}

				if (control != null)
				{
					foreach (string childElement in XmlUtils.GetChildElementsAsString(controlElement))
					{
						SetDefaultOnOutputInputs(childElement, type, control);
					}
					yield return control;
				}

				if (controlMicInput != null)
				{
					SetMicrophoneDefaults(controlElement, controlMicInput);
					yield return controlMicInput;
				}
					
			}
		}

		static void SetDefaultOnOutputInputs(string controlElement, string outputType, AbstractDmps3VolumeDeviceControl control)
		{
			string controlElementName = XmlUtils.ReadElementName(controlElement);
			ushort microphoneAddress = 0;

			if (!s_OutputInputDefaultTypes.Contains(controlElementName)) return;

			if (controlElementName == "Mic")
			{
				microphoneAddress = XmlUtils.ReadChildElementContentAsUShort(controlElement, "InputAddress");
			}

			foreach (string childElement in XmlUtils.GetChildElementsAsString(controlElement))
			{
				string childElementName = XmlUtils.ReadElementName(childElement);
				if (childElementName != "DefaultValue")
					continue;

				string defaultValueType = XmlUtils.GetAttributeAsString(childElement, "type");
				string defaultValue = XmlUtils.ReadElementContent(childElement);

				switch (controlElementName)
				{
					case "Mic":
						if (defaultValueType == "Level")
							control.SetMicrophoneLevel(microphoneAddress, short.Parse(defaultValue));
						if (defaultValueType == "Mute")
							control.SetMicrophoneMute(microphoneAddress, bool.Parse(defaultValue));
						break;

					case "MicMaster":
						if (defaultValueType == "Level")
							control.SetMicMasterLevel(short.Parse(defaultValue));
						if (defaultValueType == "Mute")
							control.SetMicMasterMute(bool.Parse(defaultValue));
						break;

					case "Source":
						if (defaultValueType == "Level")
							control.SetSourceLevel(short.Parse(defaultValue));
						if (defaultValueType == "Mute")
							control.SetSourceMute(bool.Parse(defaultValue));
						break;
#if SIMPLSHARP
					case "Codec1":
						if (defaultValueType == "Level")
						{
							if(outputType == "Program")
								((Dmps3ProgramVolumeDeviceControl) control).SetCodec1Level(short.Parse(defaultValue));

							if (outputType == "AuxOut2")
								((Dmps3AuxOut2VolumeDeviceControl)control).SetCodec1Level(short.Parse(defaultValue));
						}
						if (defaultValueType == "Mute")
						{
							if (outputType == "Program")
								((Dmps3ProgramVolumeDeviceControl)control).SetCodec1Mute(bool.Parse(defaultValue));

							if (outputType == "AuxOut2")
								((Dmps3AuxOut2VolumeDeviceControl)control).SetCodec1Mute(bool.Parse(defaultValue));
						}
						break;

					case "Codec2":
						if (defaultValueType == "Level")
						{
							if (outputType == "Program")
								((Dmps3ProgramVolumeDeviceControl)control).SetCodec2Level(short.Parse(defaultValue));

							if (outputType == "AuxOut1")
								((Dmps3AuxOut1VolumeDeviceControl)control).SetCodec2Level(short.Parse(defaultValue));
						}
						if (defaultValueType == "Mute")
						{
							if (outputType == "Program")
								((Dmps3ProgramVolumeDeviceControl)control).SetCodec2Mute(bool.Parse(defaultValue));

							if (outputType == "AuxOut1")
								((Dmps3AuxOut1VolumeDeviceControl)control).SetCodec2Mute(bool.Parse(defaultValue));
						}
						break;
#endif
					case "VolMaster":
						if (defaultValueType == "Level")
							control.SetVolumeLevel(short.Parse(defaultValue));
						if (defaultValueType == "Mute")
							control.SetVolumeMute(bool.Parse(defaultValue));
						break;

					default:
						string message = string.Format("{0} is not a valid Dmps3 input for this output", controlElementName);
						throw new FormatException(message);
				}
			}
		}

		static void SetMicrophoneDefaults(string controlElement, AbstractDmps3MicrophoneDeviceControl control)
		{
			foreach (string childElement in XmlUtils.GetChildElementsAsString(controlElement))
			{
				string childElementName = XmlUtils.ReadElementName(childElement);
				if (childElementName != "DefaultValue")
					continue;

				string defaultValueType = XmlUtils.GetAttributeAsString(childElement, "type");
				string defaultValue = XmlUtils.ReadElementContent(childElement);

				switch (defaultValueType)
				{
					case "Mute":
						control.SetMicrophoneMute(bool.Parse(defaultValue));
						break;

					case "Level":
						control.SetGainLevel(float.Parse(defaultValue));
						break;

					case "Power":
							control.SetPhantomPower(bool.Parse(defaultValue));
						break;

					default:
						string message = string.Format("{0} is not a valid default type", defaultValueType);
						throw new FormatException(message);
				}
			}
		}
	}
}
