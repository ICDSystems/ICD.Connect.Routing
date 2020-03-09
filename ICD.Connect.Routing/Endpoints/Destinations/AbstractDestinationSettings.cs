using ICD.Common.Utils.Xml;

namespace ICD.Connect.Routing.Endpoints.Destinations
{
	public abstract class AbstractDestinationSettings : AbstractSourceDestinationCommonSettings, IDestinationSettings
	{
		private const string DESTINATION_GROUP_STRING_ELEMENT = "DestinationGroupString";

		public string DestinationGroupString { get; set; }

		#region XML

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			DestinationGroupString = XmlUtils.TryReadChildElementContentAsString(xml, DESTINATION_GROUP_STRING_ELEMENT);
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(DESTINATION_GROUP_STRING_ELEMENT, IcdXmlConvert.ToString(DestinationGroupString));
		}

		#endregion
	}
}
