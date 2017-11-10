using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Endpoints.Groups
{
	public abstract class AbstractDestinationGroupSettings : AbstractSettings
	{
		private const string DESTINATIONGROUP_ELEMENT = "DestinationGroup";
		private const string DESTINATIONS_ELEMENT = "Destinations";
		private const string DESTINATION_ELEMENT = "Destination";
		private const string ORDER_ELEMENT = "Order";
		private const string DISABLE_ELEMENT = "Disable";

		/// <summary>
		/// Gets the xml element.
		/// </summary>
		protected override string Element { get { return DESTINATIONGROUP_ELEMENT; } }

		/// <summary>
		/// Gets the endpoint device.
		/// </summary>
		public IEnumerable<int> Destinations { get; set; }

		/// <summary>
		/// Specifies custom ordering of the instance to the end user.
		/// </summary>
		public int Order { get; set; }

		/// <summary>
		/// Shorthand for disabling an instance in the system.
		/// </summary>
		public bool Disable { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteStartElement(DESTINATIONS_ELEMENT);
			foreach (int d in Destinations)
				writer.WriteElementString(DESTINATION_ELEMENT, IcdXmlConvert.ToString(d));
			writer.WriteEndElement();

			if (Order != int.MaxValue)
				writer.WriteElementString(ORDER_ELEMENT, IcdXmlConvert.ToString(Order));

			if (Disable)
				writer.WriteElementString(DISABLE_ELEMENT, IcdXmlConvert.ToString(Disable));
		}

		protected static void ParseXml(AbstractDestinationGroupSettings instance, string xml)
		{
			IEnumerable<int> destinations = null;
			string destinationsElement;
			if (XmlUtils.TryGetChildElementAsString(xml, DESTINATIONS_ELEMENT, out destinationsElement))
			{
				destinations =
					XmlUtils.GetChildElementsAsString(destinationsElement, DESTINATION_ELEMENT)
					        .Select(d => XmlUtils.TryReadElementContentAsInt(d))
					        .ExceptNulls();
			}

			instance.Destinations = destinations;
			instance.Order = XmlUtils.TryReadChildElementContentAsInt(xml, ORDER_ELEMENT) ?? int.MaxValue;
			instance.Disable = XmlUtils.TryReadChildElementContentAsBoolean(xml, DISABLE_ELEMENT) ?? false;

			AbstractSettings.ParseXml(instance, xml);
		}
	}
}
