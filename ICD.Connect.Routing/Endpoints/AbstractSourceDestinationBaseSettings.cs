using System.Collections.Generic;
using ICD.Common.Utils.Xml;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Endpoints
{
	public abstract class AbstractSourceDestinationBaseSettings : AbstractSettings
	{
		protected const string DEVICE_ELEMENT = "Device";
		private const string CONTROL_ELEMENT = "Control";
		private const string ADDRESS_ELEMENT = "Address";
		private const string CONNECTION_TYPE_ELEMENT = "ConnectionType";
		private const string ORDER_ELEMENT = "Order";
		private const string DISABLE_ELEMENT = "Disable";

		private int m_Order = int.MaxValue;

		#region Properties

		/// <summary>
		/// Gets the endpoint device.
		/// </summary>
		[SettingsProperty(SettingsProperty.ePropertyType.DeviceId)]
		public int Device { get; set; }

		/// <summary>
		/// Gets the endpoint device control.
		/// </summary>
		public int Control { get; set; }

		/// <summary>
		/// Gets the endpoint connector address.
		/// </summary>
		public int Address { get; set; }

		/// <summary>
		/// Specifies which media types to use for the source.
		/// </summary>
		[SettingsProperty(SettingsProperty.ePropertyType.Enum)]
		public eConnectionType ConnectionType { get; set; }

		/// <summary>
		/// Specifies custom ordering of the instance to the end user.
		/// </summary>
		public int Order { get { return m_Order; } set { m_Order = value; } }

		/// <summary>
		/// Shorthand for disabling an instance in the system.
		/// </summary>
		public bool Disable { get; set; }

		#endregion

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(DEVICE_ELEMENT, IcdXmlConvert.ToString(Device));
			writer.WriteElementString(CONTROL_ELEMENT, IcdXmlConvert.ToString(Control));
			writer.WriteElementString(ADDRESS_ELEMENT, IcdXmlConvert.ToString(Address));
			writer.WriteElementString(CONNECTION_TYPE_ELEMENT, ConnectionType.ToString());

			if (Order != int.MaxValue)
				writer.WriteElementString(ORDER_ELEMENT, IcdXmlConvert.ToString(Order));

			if (Disable)
				writer.WriteElementString(DISABLE_ELEMENT, IcdXmlConvert.ToString(Disable));
		}

		protected static void ParseXml(AbstractSourceDestinationBaseSettings instance, string xml)
		{
			instance.Device = XmlUtils.TryReadChildElementContentAsInt(xml, DEVICE_ELEMENT) ?? 0;
			instance.Control = XmlUtils.TryReadChildElementContentAsInt(xml, CONTROL_ELEMENT) ?? 0;
			instance.Address = XmlUtils.TryReadChildElementContentAsInt(xml, ADDRESS_ELEMENT) ?? 1;
			instance.ConnectionType =
				XmlUtils.TryReadChildElementContentAsEnum<eConnectionType>(xml, CONNECTION_TYPE_ELEMENT, true) ??
				eConnectionType.Audio | eConnectionType.Video;
			instance.Order = XmlUtils.TryReadChildElementContentAsInt(xml, ORDER_ELEMENT) ?? int.MaxValue;
			instance.Disable = XmlUtils.TryReadChildElementContentAsBoolean(xml, DISABLE_ELEMENT) ?? false;

			AbstractSettings.ParseXml(instance, xml);
		}

		/// <summary>
		/// Returns the collection of ids that the settings will depend on.
		/// For example, to instantiate an IR Port from settings, the device the physical port
		/// belongs to will need to be instantiated first.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<int> GetDeviceDependencies()
		{
			yield return Device;
		}
	}
}
