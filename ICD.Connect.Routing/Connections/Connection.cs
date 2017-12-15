using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Core;
using Newtonsoft.Json;

namespace ICD.Connect.Routing.Connections
{
	[Flags]
	public enum eConnectionType
	{
		None = 0,
		Audio = 1,
		Video = 2,
		Usb = 4
	}

	/// <summary>
	/// Represents a path between two devices.
	/// </summary>
	public sealed class Connection : AbstractOriginator<ConnectionSettings>
	{
		private readonly IcdHashSet<int> m_SourceDeviceRestrictions;
		private readonly SafeCriticalSection m_SourceDeviceRestrictionsSection;

		private readonly IcdHashSet<int> m_RoomRestrictions;
		private readonly SafeCriticalSection m_RoomRestrictionsSection;

		#region Properties

		public EndpointInfo Source { get; private set; }
		public EndpointInfo Destination { get; private set; }
		public eConnectionType ConnectionType { get; private set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		public Connection()
			: this(0, 0, 0, 0, 0, eConnectionType.None)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public Connection(int id,
		                  int sourceDeviceId, int sourceAddress,
		                  int destinationDeviceId, int destinationAddress,
		                  eConnectionType connectionType)
			: this(id,
			       sourceDeviceId, 0, sourceAddress,
			       destinationDeviceId, 0, destinationAddress,
			       connectionType)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="id"></param>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="connectionType"></param>
		[JsonConstructor]
		public Connection(int id, EndpointInfo source, EndpointInfo destination, eConnectionType connectionType)
		{
			m_RoomRestrictions = new IcdHashSet<int>();
			m_RoomRestrictionsSection = new SafeCriticalSection();

			m_SourceDeviceRestrictions = new IcdHashSet<int>();
			m_SourceDeviceRestrictionsSection = new SafeCriticalSection();

			Id = id;

			Source = source;
			Destination = destination;

			ConnectionType = connectionType;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public Connection(int id,
		                  int sourceDeviceId, int sourceControlId, int sourceAddress,
		                  int destinationDeviceId, int destinationControlId, int destinationAddress,
		                  eConnectionType connectionType)
			: this(id,
			       new EndpointInfo(sourceDeviceId, sourceControlId, sourceAddress),
			       new EndpointInfo(destinationDeviceId, destinationControlId, destinationAddress),
			       connectionType)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the source device ids that are allowed to use this connection
		/// </summary>
		public IEnumerable<int> GetSourceDeviceRestrictions()
		{
			return m_SourceDeviceRestrictionsSection.Execute(() => m_SourceDeviceRestrictions.ToArray(m_SourceDeviceRestrictions.Count));
		}

		/// <summary>
		/// Gets the room ids that are allowed to use this connection
		/// </summary>
		public IEnumerable<int> GetRoomRestrictions()
		{
			return m_RoomRestrictionsSection.Execute(() => m_RoomRestrictions.ToArray(m_RoomRestrictions.Count));
		}

		/// <summary>
		/// Returns whether or not the connection can be used to route a device for the given room.
		/// The connection is available to ALL rooms if no restrictions are specified.
		/// </summary>
		/// <param name="roomId">id of the room</param>
		/// <returns></returns>
		public bool IsAvailableToRoom(int roomId)
		{
			m_RoomRestrictionsSection.Enter();

			try
			{
				return m_RoomRestrictions.Count == 0 ||
				       m_RoomRestrictions.Contains(roomId);
			}
			finally
			{
				m_RoomRestrictionsSection.Leave();
			}
		}

		/// <summary>
		/// Returns whether or not the connection can be used to route a given device.
		/// The connection is available to ALL devices if no restrictions are specified.
		/// </summary>
		/// <param name="deviceId">id of the device</param>
		/// <returns></returns>
		public bool IsAvailableToSourceDevice(int deviceId)
		{
			m_SourceDeviceRestrictionsSection.Enter();

			try
			{
				return m_SourceDeviceRestrictions.Count == 0 ||
				       m_SourceDeviceRestrictions.Contains(deviceId);
			}
			finally
			{
				m_SourceDeviceRestrictionsSection.Leave();
			}
		}

		#endregion

		#region Private Methods

		private void SetSourceDeviceRestrictions(IEnumerable<int> sourceDeviceRestrictions)
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

		private void SetRoomRestrictions(IEnumerable<int> roomRestrictions)
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

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(ConnectionSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.SourceDeviceId = Source.Device;
			settings.SourceControlId = Source.Control;
			settings.SourceAddress = Source.Address;

			settings.DestinationDeviceId = Destination.Device;
			settings.DestinationControlId = Destination.Control;
			settings.DestinationAddress = Destination.Address;

			settings.ConnectionType = ConnectionType;

			settings.SetSourceDeviceRestrictions(GetSourceDeviceRestrictions());
			settings.SetRoomRestrictions(GetRoomRestrictions());
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			Source = default(EndpointInfo);
			Destination = default(EndpointInfo);

			ConnectionType = default(eConnectionType);

			SetSourceDeviceRestrictions(Enumerable.Empty<int>());
			SetRoomRestrictions(Enumerable.Empty<int>());
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(ConnectionSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			IDevice source = factory.GetOriginatorById<IDevice>(settings.SourceDeviceId);
			IDevice destination = factory.GetOriginatorById<IDevice>(settings.DestinationDeviceId);

			// Validate the source and destination controls
			source.Controls.GetControl<IRouteSourceControl>(settings.SourceControlId);
			destination.Controls.GetControl<IRouteDestinationControl>(settings.DestinationControlId);
	
			Source = new EndpointInfo(
				settings.SourceDeviceId,
				settings.SourceControlId,
				settings.SourceAddress);

			Destination = new EndpointInfo(
				settings.DestinationDeviceId,
				settings.DestinationControlId,
				settings.DestinationAddress);

			ConnectionType = settings.ConnectionType;

			SetSourceDeviceRestrictions(settings.GetSourceDeviceRestrictions());
			SetRoomRestrictions(settings.GetRoomRestrictions());
		}

		#endregion
	}
}
