using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings.Groups;

namespace ICD.Connect.Routing.Groups.Endpoints
{
	public abstract class AbstractSourceDestinationGroupCommonSettings : AbstractGroupSettings,
	                                                                     ISourceDestinationGroupCommonSettings
	{
		private const string ELEMENT_CONNECTION_TYPE_MASK = "ConnectionTypeMask";

		/// <summary>
		/// Masks the connection types inherited from the group items.
		/// </summary>
		public eConnectionType ConnectionTypeMask { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			if (ConnectionTypeMask != EnumUtils.GetFlagsAllValue<eConnectionType>())
				writer.WriteElementString(ELEMENT_CONNECTION_TYPE_MASK, IcdXmlConvert.ToString(ConnectionTypeMask));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			ConnectionTypeMask =
				XmlUtils.TryReadChildElementContentAsEnum<eConnectionType>(xml, ELEMENT_CONNECTION_TYPE_MASK, true) ??
				EnumUtils.GetFlagsAllValue<eConnectionType>();
		}
	}
}
