using ICD.Common.Utils.Xml;

namespace ICD.Connect.Routing.Endpoints.Destinations
{
	public abstract class AbstractDestinationSettings : AbstractSourceDestinationCommonSettings, IDestinationSettings
	{
		private const string GROUP_ELEMENT = "Group";

		/// <summary>
		/// Gets/sets the group name that is used when the core loads to generate destination groups.
		/// </summary>
		public string Group { get; set; }

		#region XML

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Group = XmlUtils.TryReadChildElementContentAsString(xml, GROUP_ELEMENT);
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(GROUP_ELEMENT, IcdXmlConvert.ToString(Group));
		}

		#endregion
	}
}
