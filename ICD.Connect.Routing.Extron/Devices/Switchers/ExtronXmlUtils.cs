using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Routing.Extron.Controls.Volume;

namespace ICD.Connect.Routing.Extron.Devices.Switchers
{
	public static class ExtronXmlUtils
	{
		public static IEnumerable<IDeviceControl> GetControlsFromXml(string xml, IDtpCrosspointDevice parent)
		{
			foreach (string controlElement in XmlUtils.GetChildElementsAsString(xml))
			{
				string type = XmlUtils.GetAttributeAsString(controlElement, "type");

				switch(type)
				{
					case "Volume":
						yield return GetVolumeControl(controlElement, parent);
						break;
					case "GroupVolume":
						yield return GetGroupVolumeControl(controlElement, parent);
						break;
					default:
						string message = string.Format("{0} is not a valid Extron control type", type);
						throw new InvalidDataException(message);
				}
				
			}
		}

		private static IDeviceControl GetVolumeControl(string xml, IDtpCrosspointDevice parent)
		{
			int id = XmlUtils.GetAttributeAsInt(xml, "id");
			eExtronVolumeObject volumeObject =
				XmlUtils.ReadChildElementContentAsEnum<eExtronVolumeObject>(xml, "VolumeObject", true);
			
			return new ExtronVolumeDeviceControl(parent, id, volumeObject);
		}

		private static ExtronGroupVolumeDeviceControl GetGroupVolumeControl(string xml, IDtpCrosspointDevice parent)
		{
			int id = XmlUtils.GetAttributeAsInt(xml, "id");
			eExtronVolumeType volumeType =
				XmlUtils.ReadChildElementContentAsEnum<eExtronVolumeType>(xml, "VolumeType", true);
			int volumeGroupId = XmlUtils.TryReadChildElementContentAsInt(xml, "VolumeGroupId") ?? 1;
			int? muteGroupId = XmlUtils.TryReadChildElementContentAsInt(xml, "MuteGroupId");
			
			return new ExtronGroupVolumeDeviceControl(parent, id, volumeType, volumeGroupId, muteGroupId);
		}
	}
}
