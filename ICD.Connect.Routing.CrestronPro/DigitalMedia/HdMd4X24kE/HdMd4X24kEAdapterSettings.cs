using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd4X24kE
{
	public sealed class HdMd4X24kEAdapterSettings : AbstractDmSwitcherAdapterSettings
	{
		private const string FACTORY_NAME = "HdMd4X24kE";

		private const string ADDRESS_ELEMENT = "Address";

		public string Address { get; set; }

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(HdMd4X24kEAdapter); } }

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
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static HdMd4X24kEAdapterSettings FromXml(string xml)
		{
			string address = XmlUtils.TryReadChildElementContentAsString(xml, ADDRESS_ELEMENT);

			HdMd4X24kEAdapterSettings output = new HdMd4X24kEAdapterSettings
			{
				Address = address
			};

			output.ParseXml(xml);
			return output;
		}
	}
}
