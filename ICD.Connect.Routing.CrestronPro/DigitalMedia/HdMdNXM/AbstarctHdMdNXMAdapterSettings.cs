﻿using ICD.Common.Utils.Xml;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMdNXM
{
	public abstract class AbstractHdMdNXMAdapterSettings : AbstractCrestronSwitchAdapterSettings, IHdMdNXMAdapterSettings
	{
		private const string ADDRESS_ELEMENT = "Address";

		public string Address { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(ADDRESS_ELEMENT, Address);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Address = XmlUtils.TryReadChildElementContentAsString(xml, ADDRESS_ELEMENT);
		}
	}
}