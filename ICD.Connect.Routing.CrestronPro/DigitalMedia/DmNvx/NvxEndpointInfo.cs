using ICD.Common.Utils;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxBaseClass;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx
{
	/// <summary>
	/// Describes the relationship between the virtual switcher and an NVX transmitter/receiver.
	/// </summary>
	public sealed class NvxEndpointInfo
	{
		private readonly Connection m_Connection;
		private readonly DmNvxBaseClassSwitcherControl m_Switcher;

		#region Properties

		/// <summary>
		/// Gets the RX/TX switcher control.
		/// </summary>
		public DmNvxBaseClassSwitcherControl Switcher { get { return m_Switcher; } }

		/// <summary>
		/// Gets the address on the virtual switcher for the endpoint stream connector.
		/// </summary>
		public int LocalStreamAddress { get { return Tx ? m_Connection.Destination.Address : m_Connection.Source.Address; } }

		/// <summary>
		/// Gets the address on the TX/RX switcher for the stream connector.
		/// </summary>
		public int RemoteStreamAddress { get { return Tx ? m_Connection.Source.Address : m_Connection.Destination.Address; } }

		/// <summary>
		/// Returns true if the switcher control is connected as a transmitter, otherwise false for receiver.
		/// </summary>
		public bool Tx { get { return m_Connection.Source.Device == m_Switcher.Parent.Id; } }

		/// <summary>
		/// Gets the stream stype.
		/// </summary>
		public eConnectionType StreamType
		{
			get { return IsPrimaryStream ? eConnectionType.Audio | eConnectionType.Video : eConnectionType.Audio; }
		}

		public bool IsPrimaryStream
		{
			get
			{
				return Tx
					       ? RemoteStreamAddress == DmNvxBaseClassSwitcherControl.OUTPUT_STREAM
					       : RemoteStreamAddress == DmNvxBaseClassSwitcherControl.INPUT_STREAM;
			}
		}

		public bool IsSecondaryStream
		{
			get
			{
				return Tx
						   ? RemoteStreamAddress == DmNvxBaseClassSwitcherControl.OUTPUT_SECONDARY_AUDIO_STREAM
						   : RemoteStreamAddress == DmNvxBaseClassSwitcherControl.INPUT_SECONDARY_AUDIO_STREAM;
			}
		}

		public ConnectorInfo LocalConnector { get { return new ConnectorInfo(LocalStreamAddress, StreamType); } }

		public string LastKnownMulticastAddress
		{
			get { return IsPrimaryStream ? m_Switcher.LastKnownMulticastAddress : m_Switcher.LastKnownSecondaryAudioMulticastAddress; }
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="switcher"></param>
		public NvxEndpointInfo(Connection connection,
		                        DmNvxBaseClassSwitcherControl switcher)
		{
			m_Connection = connection;
			m_Switcher = switcher;
		}

		public override string ToString()
		{
			ReprBuilder builder = new ReprBuilder(this);

			builder.AppendProperty("Switcher", Switcher);
			builder.AppendProperty("LocalStreamAddress", LocalStreamAddress);
			builder.AppendProperty("RemoteStreamAddress", LocalStreamAddress);
			builder.AppendProperty("Tx", Tx);
			builder.AppendProperty("StreamType", StreamType);
			builder.AppendProperty("IsPrimaryStream", IsPrimaryStream);
			builder.AppendProperty("LocalConnector", LocalConnector);

			return builder.ToString();
		}
	}
}
