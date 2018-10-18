using System;
using System.Collections.Generic;
using ICD.Common.Utils.Xml;
using ICD.Connect.Audio.Controls;
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
				int id = XmlUtils.GetAttributeAsInt(controlElement, "id");
				string type = XmlUtils.GetAttributeAsString(controlElement, "type");
				string name = XmlUtils.TryReadChildElementContentAsString(controlElement, "Name");

				AbstractVolumeLevelDeviceControl<IDtpCrosspointDevice> control;

				switch (type)
				{
					case "Volume":
						control = GetVolumeControl(controlElement, id, name, parent);
						break;
					case "GroupVolume":
						control = GetGroupVolumeControl(controlElement, id, name, parent);
						break;
					default:
						string message = string.Format("{0} is not a valid Extron control type", type);
						throw new FormatException(message);
				}

				if (control == null)
					continue;

				control.VolumeRawMin = XmlUtils.TryReadChildElementContentAsFloat(controlElement, "VolumeMin");
				control.VolumeRawMax = XmlUtils.TryReadChildElementContentAsFloat(controlElement, "VolumeMax");

				yield return control;
			}
		}

		private static ExtronVolumeDeviceControl GetVolumeControl(string xml, int id, string name, IDtpCrosspointDevice parent)
		{
			eExtronVolumeObject volumeObject =
				XmlUtils.ReadChildElementContentAsEnum<eExtronVolumeObject>(xml, "VolumeObject", true);

			return new ExtronVolumeDeviceControl(parent, id, name, volumeObject);
		}

		private static ExtronGroupVolumeDeviceControl GetGroupVolumeControl(string xml, int id, string name, IDtpCrosspointDevice parent)
		{
			eExtronVolumeType volumeType = XmlUtils.ReadChildElementContentAsEnum<eExtronVolumeType>(xml, "VolumeType", true);
			int? volumeGroupId = XmlUtils.TryReadChildElementContentAsInt(xml, "VolumeGroupId");
			int? muteGroupId = XmlUtils.TryReadChildElementContentAsInt(xml, "MuteGroupId");

			return new ExtronGroupVolumeDeviceControl(parent, id, name, volumeType, volumeGroupId, muteGroupId);
		}
	}
}
