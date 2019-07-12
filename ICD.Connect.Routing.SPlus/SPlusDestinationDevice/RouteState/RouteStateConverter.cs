using System.Linq;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Json;
using ICD.Connect.Routing.Connections;
using Newtonsoft.Json;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.RouteState
{
	public sealed class RouteStateConverter : AbstractGenericJsonConverter<RouteState>
	{

		private const string ATTR_INPUTS_DETECTED = "inputsDetected";
		private const string ATTR_INPUTS_ACTIVE = "inputsActive";


		/// <summary>
		/// Override to write properties to the writer.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, RouteState value, JsonSerializer serializer)
		{
			base.WriteProperties(writer, value, serializer);

			if (value.InputsDetected != null && value.InputsDetected.Length > 0)
			{
				writer.WritePropertyName(ATTR_INPUTS_DETECTED);
				serializer.SerializeArray(writer, value.InputsDetected);
			}
			if (value.InputsActive != null && value.InputsActive.Count > 0)
			{
				writer.WritePropertyName(ATTR_INPUTS_ACTIVE);
				serializer.SerializeDict(writer, value.InputsActive);
			}
		}

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected override void ReadProperty(string property, JsonReader reader, RouteState instance, JsonSerializer serializer)
		{
			switch (property)
			{
				case ATTR_INPUTS_DETECTED:
					instance.InputsDetected = serializer.DeserializeArray<int>(reader).ToArray();
					break;
				case ATTR_INPUTS_ACTIVE:
					instance.InputsActive = serializer.DeserializeDict<eConnectionType, int?>(reader).ToDictionary();
					break;
				default:
					base.ReadProperty(property, reader, instance, serializer);
					break;
			}
		}
	}
}