using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.Network.Settings;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Atlona
{
	[KrangSettings("AtUhdHdvs300", typeof(AtUhdHdvs300Device))]
	public sealed class AtUhdHdvs300DeviceSettings : AbstractDeviceSettings, INetworkSettings
	{
		private const ushort DEFAULT_NETWORK_PORT = 23;

		private const string ELEMENT_PORT = "Port";
		private const string ELEMENT_USERNAME = "Username";
		private const string ELEMENT_PASSWORD = "Password";

		private readonly NetworkProperties m_NetworkProperties;

		#region Properties

		/// <summary>
		/// The port id.
		/// </summary>
		[OriginatorIdSettingsProperty(typeof(ISerialPort))]
		public int? Port { get; set; }

		public string Username { get; set; }

		public string Password { get; set; }

		#endregion

		#region Network

		/// <summary>
		/// Gets/sets the configurable network address.
		/// </summary>
		public string NetworkAddress
		{
			get { return m_NetworkProperties.NetworkAddress; }
			set { m_NetworkProperties.NetworkAddress = value; }
		}

		/// <summary>
		/// Gets/sets the configurable network port.
		/// </summary>
		public ushort? NetworkPort
		{
			get { return m_NetworkProperties.NetworkPort; }
			set { m_NetworkProperties.NetworkPort = value; }
		}

		/// <summary>
		/// Clears the configured values.
		/// </summary>
		void INetworkProperties.ClearNetworkProperties()
		{
			m_NetworkProperties.ClearNetworkProperties();
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public AtUhdHdvs300DeviceSettings()
		{
			m_NetworkProperties = new NetworkProperties();

			UpdateNetworkDefaults();
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(ELEMENT_PORT, IcdXmlConvert.ToString(Port));
			writer.WriteElementString(ELEMENT_USERNAME, Username);
			writer.WriteElementString(ELEMENT_PASSWORD, Password);

			m_NetworkProperties.WriteElements(writer);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Port = XmlUtils.TryReadChildElementContentAsInt(xml, ELEMENT_PORT);
			Username = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_USERNAME);
			Password = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_PASSWORD);

			m_NetworkProperties.ParseXml(xml);

			UpdateNetworkDefaults();
		}

		/// <summary>
		/// Sets default values for unconfigured network properties.
		/// </summary>
		private void UpdateNetworkDefaults()
		{
			m_NetworkProperties.ApplyDefaultValues(null, DEFAULT_NETWORK_PORT);
		}
	}
}
