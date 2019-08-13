using System.Linq;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Json;
using Newtonsoft.Json;

namespace ICD.Connect.Routing.SPlus.SPlusSwitcher.State
{
	public sealed class SPlusSwitcherStateConverter : AbstractGenericJsonConverter<SPlusSwitcherState>
	{

		private const string ATTR_INPUTS_DETECTED = "inputsDetected";
		private const string ATTR_AUDIO_OUTPUT_ROUTING = "audioOutRouting";
		private const string ATTR_VIDEO_OUTPUT_ROUTING = "videoOutRouting";
		private const string ATTR_USB_OUTPUT_ROUTING = "usbOutRouting";

		/// <summary>
		/// Override to write properties to the writer.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, SPlusSwitcherState value, JsonSerializer serializer)
		{
			base.WriteProperties(writer, value, serializer);

			if (value.DetectedInputs != null && value.DetectedInputs.Count > 0)
			{
				writer.WritePropertyName(ATTR_INPUTS_DETECTED);
				serializer.SerializeArray(writer, value.DetectedInputs);
			}

			if (value.AudioOutputRouting != null && value.AudioOutputRouting.Count > 0)
			{
				writer.WritePropertyName(ATTR_AUDIO_OUTPUT_ROUTING);
				serializer.SerializeDict(writer, value.AudioOutputRouting);
			}

			if (value.VideoOutputRouting != null && value.VideoOutputRouting.Count > 0)
			{
				writer.WritePropertyName(ATTR_VIDEO_OUTPUT_ROUTING);
				serializer.SerializeDict(writer, value.VideoOutputRouting);
			}

			if (value.UsbOutputRouting != null && value.UsbOutputRouting.Count > 0)
			{
				writer.WritePropertyName(ATTR_USB_OUTPUT_ROUTING);
				serializer.SerializeDict(writer, value.UsbOutputRouting);
			}
		}

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected override void ReadProperty(string property, JsonReader reader, SPlusSwitcherState instance, JsonSerializer serializer)
		{
			switch (property)
			{
				case ATTR_INPUTS_DETECTED:
					instance.AddDetectedInputsRange(serializer.DeserializeArray<int>(reader));
					break;
				case ATTR_AUDIO_OUTPUT_ROUTING:
					instance.AddAudioOutputRoutingRange(serializer.DeserializeDict<int, int>(reader));
					break;
				case ATTR_VIDEO_OUTPUT_ROUTING:
					instance.AddVideoOutputRoutingRange(serializer.DeserializeDict<int, int>(reader));
					break;
				case ATTR_USB_OUTPUT_ROUTING:
					instance.AddUsbOutputRoutingRange(serializer.DeserializeDict<int, int>(reader));
					break;
				default:
					base.ReadProperty(property, reader, instance, serializer);
					break;
			}
		}
	}
}