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
		private const string ELEMENT_ENABLE_WHEN_OFFLINE = "EnableWhenOffline";

		/// <summary>
		/// Masks the connection types inherited from the group items.
		/// </summary>
		public eConnectionType ConnectionTypeMask { get; set; }

		/// <summary>
		/// Indicates that the UI should enable this source/destination even when offline
		/// </summary>
		public bool EnableWhenOffline { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			if (ConnectionTypeMask != EnumUtils.GetFlagsAllValue<eConnectionType>())
				writer.WriteElementString(ELEMENT_CONNECTION_TYPE_MASK, IcdXmlConvert.ToString(ConnectionTypeMask));
			writer.WriteElementString(ELEMENT_ENABLE_WHEN_OFFLINE, IcdXmlConvert.ToString(EnableWhenOffline));
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

			EnableWhenOffline = XmlUtils.TryReadChildElementContentAsBoolean(xml, ELEMENT_ENABLE_WHEN_OFFLINE) ?? false;
		}
	}
}
