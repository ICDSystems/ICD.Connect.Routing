using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ICD.Common.Utils.Extensions;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM.Cards;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif
using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Audio.Controls;
using ICD.Connect.Devices.Controls;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume
{
	public static class Dmps3VolumeXmlUtils
	{
		public static IEnumerable<IDeviceControl> GetControlsFromXml(string xml, ControlSystemDevice parent)
		{
			foreach (string controlElement in XmlUtils.GetChildElementsAsString(xml))
			{
				int id = XmlUtils.GetAttributeAsInt(controlElement, "id"); 
				string type = XmlUtils.GetAttributeAsString(controlElement, "type");
				string name = XmlUtils.TryReadChildElementContentAsString(controlElement, "Name");

				AbstractDmps3VolumeDeviceControl control;

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

#endif
					default:
						string message = string.Format("{0} is not a valid Dmps3 control type", type);
						throw new FormatException(message);
				}

				if (control == null)
					continue;

				yield return control;
			}
		}
	}
}
