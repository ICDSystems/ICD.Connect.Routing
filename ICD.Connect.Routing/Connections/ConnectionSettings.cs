using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Attributes.Properties;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Connections
{
	/// <summary>
	/// Settings for a Connection.
	/// </summary>
	public sealed class ConnectionSettings : AbstractSettings
	{
		private const string CONNECTION_ELEMENT = "Connection";
		private const string FACTORY_NAME = "Connection";

		private const string SOURCE_DEVICE_ELEMENT = "SourceDevice";
		private const string SOURCE_CONTROL_ELEMENT = "SourceControl";
		private const string SOURCEADDRESS_ELEMENT = "SourceAddress";

		private const string DESTINATION_DEVICE_ELEMENT = "DestinationDevice";
		private const string DESTINATION_CONTROL_ELEMENT = "DestinationControl";
		private const string DESTINATIONADDRESS_ELEMENT = "DestinationAddress";

		private const string CONNECTION_TYPE_ELEMENT = "ConnectionType";

		private const string SOURCE_DEVICE_RESTRICTIONS_ELEMENT = "SourceDeviceRestrictions";
		private const string DEVICE_ELEMENT = "Device";
		private const string ROOM_RESTRICTIONS_ELEMENT = "RoomRestrictions";
		private const string ROOM_ELEMENT = "Room";

		private readonly IcdHashSet<int> m_SourceDeviceRestrictions;
		private readonly SafeCriticalSection m_SourceDeviceRestrictionsSection;

		private readonly IcdHashSet<int> m_RoomRestrictions;
		private readonly SafeCriticalSection m_RoomRestrictionsSection;

		private int m_SourceAddress = 1;
		private int m_DestinationAddress = 1;

		#region Properties

		/// <summary>
		/// Gets the xml element.
		/// </summary>
		protected override string Element { get { return CONNECTION_ELEMENT; } }

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		[SettingsProperty(SettingsProperty.ePropertyType.DeviceId)]
		public int SourceDeviceId { get; set; }

		public int SourceControlId { get; set; }

		public int SourceAddress { get { return m_SourceAddress; } set { m_SourceAddress = value; } }

		[SettingsProperty(SettingsProperty.ePropertyType.DeviceId)]
		public int DestinationDeviceId { get; set; }

		public int DestinationControlId { get; set; }

		public int DestinationAddress { get { return m_DestinationAddress; } set { m_DestinationAddress = value; } }

		[SettingsProperty(SettingsProperty.ePropertyType.Enum)]
		public eConnectionType ConnectionType { get; set; }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public ConnectionSettings()
		{
			m_RoomRestrictions = new IcdHashSet<int>();
			m_RoomRestrictionsSection = new SafeCriticalSection();

			m_SourceDeviceRestrictions = new IcdHashSet<int>();
			m_SourceDeviceRestrictionsSection = new SafeCriticalSection();
		}

		#region Methods

		public void SetSourceDeviceRestrictions(IEnumerable<int> sourceDeviceRestrictions)
		{
			m_SourceDeviceRestrictionsSection.Enter();

			try
			{
				m_SourceDeviceRestrictions.Clear();
				m_SourceDeviceRestrictions.AddRange(sourceDeviceRestrictions);
			}
			finally
			{
				m_SourceDeviceRestrictionsSection.Leave();
			}
		}

		public void SetRoomRestrictions(IEnumerable<int> roomRestrictions)
		{
			m_RoomRestrictionsSection.Enter();

			try
			{
				m_RoomRestrictions.Clear();
				m_RoomRestrictions.AddRange(roomRestrictions);
			}
			finally
			{
				m_RoomRestrictionsSection.Leave();
			}
		}

		public IEnumerable<int> GetSourceDeviceRestrictions()
		{
			return m_SourceDeviceRestrictionsSection.Execute(() => m_SourceDeviceRestrictions.Order().ToArray());
		}

		public IEnumerable<int> GetRoomRestrictions()
		{
			return m_RoomRestrictionsSection.Execute(() => m_RoomRestrictions.Order().ToArray());
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(CONNECTION_TYPE_ELEMENT, ConnectionType.ToString());

			writer.WriteElementString(SOURCE_DEVICE_ELEMENT, IcdXmlConvert.ToString(SourceDeviceId));
			writer.WriteElementString(SOURCE_CONTROL_ELEMENT, IcdXmlConvert.ToString(SourceControlId));
			writer.WriteElementString(SOURCEADDRESS_ELEMENT, IcdXmlConvert.ToString(SourceAddress));

			writer.WriteElementString(DESTINATION_DEVICE_ELEMENT, IcdXmlConvert.ToString(DestinationDeviceId));
			writer.WriteElementString(DESTINATION_CONTROL_ELEMENT, IcdXmlConvert.ToString(DestinationControlId));
			writer.WriteElementString(DESTINATIONADDRESS_ELEMENT, IcdXmlConvert.ToString(DestinationAddress));

			if (m_SourceDeviceRestrictions.Count > 0)
				XmlUtils.WriteListToXml(writer, GetSourceDeviceRestrictions(), SOURCE_DEVICE_RESTRICTIONS_ELEMENT, DEVICE_ELEMENT);
			if (m_RoomRestrictions.Count > 0)
				XmlUtils.WriteListToXml(writer, GetRoomRestrictions(), ROOM_RESTRICTIONS_ELEMENT, ROOM_ELEMENT);
		}

		/// <summary>
		/// Creates a new originator instance from the settings.
		/// </summary>
		/// <param name="factory"></param>
		/// <returns></returns>
		public override IOriginator ToOriginator(IDeviceFactory factory)
		{
			Connection connection = new Connection();
			connection.ApplySettings(this, factory);
			return connection;
		}

		/// <summary>
		/// Returns the collection of ids that the settings will depend on.
		/// For example, to instantiate an IR Port from settings, the device the physical port
		/// belongs to will need to be instantiated first.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<int> GetDeviceDependencies()
		{
			if (SourceDeviceId != 0)
				yield return SourceDeviceId;
			if (DestinationDeviceId != 0)
				yield return DestinationDeviceId;
		}

		/// <summary>
		/// Instantiates Connection settings from an xml element.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlConnectionSettingsFactoryMethod(FACTORY_NAME)]
		public static ConnectionSettings FromXml(string xml)
		{
			eConnectionType connectionType;
			if (!XmlUtils.TryReadChildElementContentAsEnum(xml, CONNECTION_TYPE_ELEMENT, true, out connectionType))
				connectionType = eConnectionType.Audio | eConnectionType.Video;

			int? sourceDeviceId = XmlUtils.TryReadChildElementContentAsInt(xml, SOURCE_DEVICE_ELEMENT);
			int? sourceControlId = XmlUtils.TryReadChildElementContentAsInt(xml, SOURCE_CONTROL_ELEMENT);
			int? sourceAddress = XmlUtils.TryReadChildElementContentAsInt(xml, SOURCEADDRESS_ELEMENT);

			int? destinationDeviceId = XmlUtils.TryReadChildElementContentAsInt(xml, DESTINATION_DEVICE_ELEMENT);
			int? destinationControlId = XmlUtils.TryReadChildElementContentAsInt(xml, DESTINATION_CONTROL_ELEMENT);
			int? destinationAddress = XmlUtils.TryReadChildElementContentAsInt(xml, DESTINATIONADDRESS_ELEMENT);

			IEnumerable<int> deviceRestictions = GetRestrictionsFromXml(xml, SOURCE_DEVICE_RESTRICTIONS_ELEMENT, DEVICE_ELEMENT);
			IEnumerable<int> roomRestrictions = GetRestrictionsFromXml(xml, ROOM_RESTRICTIONS_ELEMENT, ROOM_ELEMENT);

			ConnectionSettings output = new ConnectionSettings
			{
				SourceDeviceId = sourceDeviceId ?? 0,
				SourceControlId = sourceControlId ?? 0,
				SourceAddress = sourceAddress ?? 1,
				DestinationDeviceId = destinationDeviceId ?? 0,
				DestinationControlId = destinationControlId ?? 0,
				DestinationAddress = destinationAddress ?? 1,
				ConnectionType = connectionType,
			};

			output.SetRoomRestrictions(roomRestrictions);
			output.SetSourceDeviceRestrictions(deviceRestictions);

			ParseXml(output, xml);
			return output;
		}

		private static IEnumerable<int> GetRestrictionsFromXml(string xml, string parentElement, string childElement)
		{
			Func<string, int> callback = XmlUtils.ReadElementContentAsInt;
			return XmlUtils.ReadListFromXml(xml, parentElement, childElement, callback);
		}

		#endregion
	}
}
