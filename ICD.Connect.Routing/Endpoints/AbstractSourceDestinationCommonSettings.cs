using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Endpoints
{
	public abstract class AbstractSourceDestinationCommonSettings : AbstractSettings, ISourceDestinationCommonSettings
	{
		private const string DEVICE_ELEMENT = "Device";
		private const string CONTROL_ELEMENT = "Control";
		private const string ADDRESSES_ELEMENT = "Addresses";
		private const string ADDRESS_ELEMENT = "Address";
		private const string CONNECTION_TYPE_ELEMENT = "ConnectionType";
		private const string ENABLE_WHEN_OFFLINE_ELEMENT = "EnableWhenOffline";

		private readonly IcdHashSet<int> m_Addresses;

		#region Properties

		/// <summary>
		/// Gets the endpoint device.
		/// </summary>
		[OriginatorIdSettingsProperty(typeof(IDeviceBase))]
		public int Device { get; set; }

		/// <summary>
		/// Gets the endpoint device control.
		/// </summary>
		public int Control { get; set; }

		/// <summary>
		/// Specifies which media types to use for the source.
		/// </summary>
		public eConnectionType ConnectionType { get; set; }

		/// <summary>
		/// Indicates that the UI should enable this source/destination even when offline
		/// </summary>
		public bool EnableWhenOffline { get; set; }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractSourceDestinationCommonSettings()
		{
			m_Addresses = new IcdHashSet<int>();
		}

		/// <summary>
		/// Gets the addresses used by this source/destination.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<int> GetAddresses()
		{
			return m_Addresses.ToArray(m_Addresses.Count);
		}

		/// <summary>
		/// Sets the addresses used by this source/destination.
		/// </summary>
		/// <param name="addresses"></param>
		public void SetAddresses(IEnumerable<int> addresses)
		{
			m_Addresses.Clear();
			m_Addresses.AddRange(addresses);
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(DEVICE_ELEMENT, IcdXmlConvert.ToString(Device));
			writer.WriteElementString(CONTROL_ELEMENT, IcdXmlConvert.ToString(Control));

			XmlUtils.WriteListToXml(writer, GetAddresses().Order(), ADDRESSES_ELEMENT, ADDRESS_ELEMENT);

			writer.WriteElementString(CONNECTION_TYPE_ELEMENT, ConnectionType.ToString());

			writer.WriteElementString(ENABLE_WHEN_OFFLINE_ELEMENT, IcdXmlConvert.ToString(EnableWhenOffline));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Device = XmlUtils.TryReadChildElementContentAsInt(xml, DEVICE_ELEMENT) ?? 0;
			Control = XmlUtils.TryReadChildElementContentAsInt(xml, CONTROL_ELEMENT) ?? 0;
			ConnectionType =
				XmlUtils.TryReadChildElementContentAsEnum<eConnectionType>(xml, CONNECTION_TYPE_ELEMENT, true) ??
				eConnectionType.Audio | eConnectionType.Video;

			IEnumerable<int> addresses =
				XmlUtils.ReadListFromXml(xml, ADDRESSES_ELEMENT, ADDRESS_ELEMENT, e => XmlUtils.ReadElementContentAsInt(e));

			// Migration step
			int? oldAddress = XmlUtils.TryReadChildElementContentAsInt(xml, ADDRESS_ELEMENT);
			if (oldAddress.HasValue)
				addresses = addresses.Append(oldAddress.Value);

			SetAddresses(addresses);

			EnableWhenOffline = XmlUtils.TryReadChildElementContentAsBoolean(xml, ENABLE_WHEN_OFFLINE_ELEMENT) ?? false;
		}
	}
}
