using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Devices.Streaming
{
	[KrangSettings("StreamSourceDevice", typeof(StreamSourceDevice))]
	public sealed class StreamSourceDeviceSettings : AbstractDeviceSettings
	{
		private const string STREAM_URI_ELEMENT = "StreamUri";

		public string StreamUri { get; set; }

		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			StreamUri = XmlUtils.TryReadChildElementContentAsString(xml, STREAM_URI_ELEMENT);
		}

		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(STREAM_URI_ELEMENT, StreamUri);
		}
	}
}
