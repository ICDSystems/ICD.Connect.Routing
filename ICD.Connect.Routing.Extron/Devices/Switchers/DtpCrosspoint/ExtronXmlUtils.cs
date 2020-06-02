using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.Xml;
using ICD.Connect.Audio.Controls.Volume;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.Utils;
using ICD.Connect.Routing.Extron.Controls.Volume;

namespace ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint
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
				Guid uuid;

				try
				{
					uuid = XmlUtils.GetAttributeAsGuid(controlElement, "uuid");
				}
				catch (Exception)
				{
					uuid = DeviceControlUtils.GenerateUuid(parent, id);
				}

				AbstractVolumeDeviceControl<IDtpCrosspointDevice> control;

				switch (type)
				{
					case "Volume":
						control = GetVolumeControl(controlElement, id, uuid, name, parent);
						break;
					case "GroupVolume":
						control = GetGroupVolumeControl(controlElement, id, uuid, name, parent);
						break;
					default:
						string message = string.Format("{0} is not a valid Extron control type", type);
						throw new FormatException(message);
				}

				yield return control;
			}
		}

		[NotNull]
		private static ExtronVolumeDeviceControl GetVolumeControl(string xml, int id, Guid uuid, string name, IDtpCrosspointDevice parent)
		{
			eExtronVolumeObject volumeObject =
				XmlUtils.ReadChildElementContentAsEnum<eExtronVolumeObject>(xml, "VolumeObject", true);

			return new ExtronVolumeDeviceControl(parent, id, uuid, name, volumeObject);
		}

		[NotNull]
		private static ExtronGroupVolumeDeviceControl GetGroupVolumeControl(string xml, int id, Guid uuid, string name, IDtpCrosspointDevice parent)
		{
			eExtronVolumeType volumeType = XmlUtils.ReadChildElementContentAsEnum<eExtronVolumeType>(xml, "VolumeType", true);
			int? volumeGroupId = XmlUtils.TryReadChildElementContentAsInt(xml, "VolumeGroupId");
			int? muteGroupId = XmlUtils.TryReadChildElementContentAsInt(xml, "MuteGroupId");

			return new ExtronGroupVolumeDeviceControl(parent, id, uuid, name, volumeType, volumeGroupId, muteGroupId);
		}
	}
}
