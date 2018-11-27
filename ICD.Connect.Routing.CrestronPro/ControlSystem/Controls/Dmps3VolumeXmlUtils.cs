using ICD.Common.Properties;
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
		public static IEnumerable<IDeviceControl> GetControlsFromXml(string xml, ControlSystemDevice parent)
		{
			return XmlUtils.GetChildElementsAsString(xml)
				.Select(controlElement => InstantiateControlFromXml(controlElement, parent));
		}

		[NotNull]
		public static IDeviceControl InstantiateControlFromXml(string controlElement, ControlSystemDevice parent)
		{
			int id = XmlUtils.GetAttributeAsInt(controlElement, "id");
			string type = XmlUtils.GetAttributeAsString(controlElement, "type");
			string name = XmlUtils.TryReadChildElementContentAsString(controlElement, "Name");

			IDeviceControl control;

			switch (type)
			{
				case "Volume":
				{
					string outputType = XmlUtils.TryReadChildElementContentAsString(controlElement, "OutputType");

					switch (outputType)
					{
#if SIMPLSHARP
						case "HdmiDm":
							uint outputAddress = XmlUtils.ReadChildElementContentAsUint(controlElement, "OutputAddress");
							Card.Dmps3OutputBase card = parent.ControlSystem.SwitcherOutputs[outputAddress] as Card.Dmps3OutputBase;
							bool outputBackend = card is Card.Dmps3DmOutputBackend || card is Card.Dmps3HdmiOutputBackend;
							if (outputBackend)
								control = new Dmps3HdmiDmBackEndVolumeDeviceControl(parent, id, name, outputAddress, controlElement);
							else
								control = new Dmps3HdmiDmVolumeDeviceControl(parent, id, name, outputAddress, controlElement);
							break;

						case "Program":
							ICardInputOutputType progOutCard =
								parent.ControlSystem.SwitcherOutputs.Values.FirstOrDefault(
									r => r.CardInputOutputType == eCardInputOutputType.Dmps3ProgramOutput);
							Card.Dmps3ProgramOutput progOut = progOutCard as Card.Dmps3ProgramOutput;
							control = new Dmps3ProgramVolumeDeviceControl(parent, id, name, progOut, controlElement);
							break;

						case "AuxOut1":
							ICardInputOutputType auxOut1Card =
								parent.ControlSystem.SwitcherOutputs.Values.FirstOrDefault(
									r => r.CardInputOutputType == eCardInputOutputType.Dmps3Aux1Output);
							Card.Dmps3Aux1Output auxOut1 = auxOut1Card as Card.Dmps3Aux1Output;
							control = new Dmps3AuxOut1VolumeDeviceControl(parent, id, name, auxOut1, controlElement);
							break;

						case "AuxOut2":
							ICardInputOutputType auxOut2Card =
								parent.ControlSystem.SwitcherOutputs.Values.FirstOrDefault(
									r => r.CardInputOutputType == eCardInputOutputType.Dmps3Aux2Output);
							Card.Dmps3Aux2Output auxOut2 = auxOut2Card as Card.Dmps3Aux2Output;
							control = new Dmps3AuxOut2VolumeDeviceControl(parent, id, name, auxOut2, controlElement);
							break;
#endif
						default:
							string message = string.Format("{0} is not a valid Dmps3 crosspoint type", outputType);
							throw new FormatException(message);
					}
				}
					break;
				case "Input":
				{
					string inputType = XmlUtils.TryReadChildElementContentAsString(controlElement, "InputType");

					switch (inputType)
					{
						case "Microphone":
							uint micInputId = XmlUtils.ReadChildElementContentAsUint(controlElement, "InputAddress");
							control = new Dmps3MicrophoneDeviceControl(parent, id, name, micInputId, controlElement);
							break;

						default:
							string message = string.Format("{0} is not a valid Dmps3 input type", inputType);
							throw new FormatException(message);
					}
				}
					break;
				default:
				{
					string message = string.Format("{0} is not a valid Dmps3 control type", type);
					throw new FormatException(message);
				}
			}

			return control;
		}
	}
}
