using System;
using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Connections
{
	/// <summary>
	/// Settings for a Connection.
	/// </summary>
	[KrangSettings(FACTORY_NAME)]
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

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(Connection); } }

		[OriginatorIdSettingsProperty(typeof(IDeviceBase))]
		public int SourceDeviceId { get; set; }

		public int SourceControlId { get; set; }

		public int SourceAddress { get { return m_SourceAddress; } set { m_SourceAddress = value; } }

		[OriginatorIdSettingsProperty(typeof(IDeviceBase))]
		public int DestinationDeviceId { get; set; }

		public int DestinationControlId { get; set; }

		public int DestinationAddress { get { return m_DestinationAddress; } set { m_DestinationAddress = value; } }

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
			return m_SourceDeviceRestrictionsSection.Execute(() => m_SourceDeviceRestrictions.Order().ToArray(m_SourceDeviceRestrictions.Count));
		}

		public IEnumerable<int> GetRoomRestrictions()
		{
			return m_RoomRestrictionsSection.Execute(() => m_RoomRestrictions.Order().ToArray(m_RoomRestrictions.Count));
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
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

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

			SourceDeviceId = sourceDeviceId ?? 0;
			SourceControlId = sourceControlId ?? 0;
			SourceAddress = sourceAddress ?? 1;
			DestinationDeviceId = destinationDeviceId ?? 0;
			DestinationControlId = destinationControlId ?? 0;
			DestinationAddress = destinationAddress ?? 1;
			ConnectionType = connectionType;
			
			SetRoomRestrictions(roomRestrictions);
			SetSourceDeviceRestrictions(deviceRestictions);
		}

		/// <summary>
		/// Returns true if the settings depend on a device with the given ID.
		/// For example, to instantiate an IR Port from settings, the device the physical port
		/// belongs to will need to be instantiated first.
		/// </summary>
		/// <returns></returns>
		public override bool HasDeviceDependency(int id)
		{
			return id != 0 && (id == SourceDeviceId || id == DestinationDeviceId);
		}

		/// <summary>
		/// Returns the count from the collection of ids that the settings depends on.
		/// </summary>
		public override int DependencyCount
		{
			get
			{
				int count = 0;
				if (SourceDeviceId != 0)
					count++;
				if (DestinationDeviceId != 0)
					count++;
				return count;
			}
		}

		private static IEnumerable<int> GetRestrictionsFromXml(string xml, string parentElement, string childElement)
		{
			Func<string, int> callback = XmlUtils.ReadElementContentAsInt;
			return XmlUtils.ReadListFromXml(xml, parentElement, childElement, callback);
		}

		#endregion
	}
}
