using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints;

namespace ICD.Connect.Routing.ConnectionUsage
{
	/// <summary>
	/// Provides information about how a connection is being used.
	/// </summary>
	public sealed class ConnectionUsageInfo
	{
		private readonly Dictionary<eConnectionType, IcdHashSet<int>> m_Rooms;
		private readonly Dictionary<eConnectionType, EndpointInfo> m_Sources;
		private readonly SafeCriticalSection m_CriticalSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		public ConnectionUsageInfo()
		{
			m_Rooms = new Dictionary<eConnectionType, IcdHashSet<int>>();
			m_Sources = new Dictionary<eConnectionType, EndpointInfo>();
			m_CriticalSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Clears the connection usage info for the given type.
		/// Returns true if the ConnectionUsageInfo changed.
		/// </summary>
		/// <param name="type"></param>
		public bool Clear(eConnectionType type)
		{
			bool output = false;

			m_CriticalSection.Enter();

			try
			{
				// Clear the rooms
				IcdHashSet<int> rooms;
				if (m_Rooms.TryGetValue(type, out rooms))
				{
					if (rooms.Count > 0)
						output = true;
					rooms.Clear();
				}

				// Clear the source
				output |= m_Sources.Remove(type);
			}
			finally
			{
				m_CriticalSection.Leave();
			}

			return output;
		}

		/// <summary>
		/// Adds the room to the connection usage.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="type"></param>
		public void AddRoom(int room, eConnectionType type)
		{
			m_CriticalSection.Enter();

			try
			{
				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
				{
					if (!m_Rooms.ContainsKey(flag))
						m_Rooms[flag] = new IcdHashSet<int>();
					m_Rooms[flag].Add(room);
				}
			}
			finally
			{
				m_CriticalSection.Leave();
			}
		}

		/// <summary>
		/// Removes the room from the connection usage.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="type"></param>
		public void RemoveRoom(int room, eConnectionType type)
		{
			m_CriticalSection.Enter();

			try
			{
				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
				{
					if (m_Rooms.ContainsKey(flag))
						m_Rooms[flag].Remove(room);
				}
			}
			finally
			{
				m_CriticalSection.Leave();
			}
		}

		/// <summary>
		/// Gets the rooms currently using the connection.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Only one flag may be specified.</exception>
		public IEnumerable<int> GetRooms(eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				throw new InvalidOperationException(string.Format("{0} has more than one flag - {1}", typeof(eConnectionType).Name,
				                                                  type));
			}

			m_CriticalSection.Enter();

			try
			{
				if (!m_Rooms.ContainsKey(type))
					return Enumerable.Empty<int>();

				IcdHashSet<int> rooms = m_Rooms[type];
				
				// Copy for threadsafety
				return rooms.ToArray(rooms.Count);
			}
			finally
			{
				m_CriticalSection.Leave();
			}
		}

		/// <summary>
		/// Sets the source using the connection.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="type"></param>
		public void SetSource(EndpointInfo? source, eConnectionType type)
		{
			m_CriticalSection.Enter();

			try
			{
				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
				{
					if (source.HasValue)
						m_Sources[flag] = source.Value;
					else
						m_Sources.Remove(flag);
				}
			}
			finally
			{
				m_CriticalSection.Leave();
			}
		}

		/// <summary>
		/// Clears the source from using the connection.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="type"></param>
		public void ClearSource(EndpointInfo source, eConnectionType type)
		{
			m_CriticalSection.Enter();

			try
			{
				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
				{
					if (!m_Sources.ContainsKey(flag))
						continue;

					if (m_Sources[flag] == source)
						m_Sources.Remove(flag);
				}
			}
			finally
			{
				m_CriticalSection.Leave();
			}
		}

		/// <summary>
		/// Gets the source currently using the connection.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Only one flag may be specified.</exception>
		public EndpointInfo? GetSource(eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				throw new InvalidOperationException(string.Format("{0} has more than one flag - {1}", typeof(eConnectionType).Name,
				                                                  type));
			}

			m_CriticalSection.Enter();

			try
			{
				return m_Sources.ContainsKey(type) ? m_Sources[type] : (EndpointInfo?)null;
			}
			finally
			{
				m_CriticalSection.Leave();
			}
		}

		/// <summary>
		/// Updates this info so the connection is claimed by the given room.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="roomId"></param>
		/// <param name="connectionType"></param>
		public void Claim(EndpointInfo source, int roomId, eConnectionType connectionType)
		{
			m_CriticalSection.Enter();

			try
			{
				Claim(source, connectionType);

				if (roomId != 0)
					AddRoom(roomId, connectionType);
			}
			finally
			{
				m_CriticalSection.Leave();
			}
		}

		/// <summary>
		/// Updates the info so the connection is used by the given source.
		/// Rooms are cleared if the source changes.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="connectionType"></param>
		public void Claim(EndpointInfo source, eConnectionType connectionType)
		{
			m_CriticalSection.Enter();

			try
			{
				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(connectionType))
				{
					EndpointInfo? oldSource = GetSource(flag);
					if (oldSource != source)
						Clear(flag);

					SetSource(source, flag);
				}
			}
			finally
			{
				m_CriticalSection.Leave();
			}
		}

		/// <summary>
		/// Returns true if one or more of the following pass:
		/// 		The connection is unclaimed
		///			The connection is only claimed by the given room
		///			The connection is being used for the given source
		/// </summary>
		/// <param name="source"></param>
		/// <param name="roomId"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool CanRoute(EndpointInfo source, int roomId, eConnectionType type)
		{
			m_CriticalSection.Enter();

			try
			{
				if (EnumUtils.HasMultipleFlags(type))
				{
					return EnumUtils.GetFlagsExceptNone(type)
					                .Select(t => CanRoute(source, roomId, t))
					                .Unanimous(false);
				}

				EndpointInfo? currentSource = GetSource(type);
				if (currentSource.HasValue && currentSource.Value == source)
					return true;

				return CanRoute(roomId, type);
			}
			finally
			{
				m_CriticalSection.Leave();
			}
		}

		/// <summary>
		/// Returns true if one or more of the following pass:
		/// 		The connection is unclaimed
		///			The connection is only claimed by the given room
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool CanRoute(int roomId, eConnectionType type)
		{
			m_CriticalSection.Enter();

			try
			{
				if (EnumUtils.HasMultipleFlags(type))
				{
					return EnumUtils.GetFlagsExceptNone(type)
					                .Select(t => CanRoute(roomId, t))
					                .Unanimous(false);
				}

				int[] rooms = GetRooms(type).ToArray();

				if (rooms.Length == 0)
					return true;

				return rooms.Length == 1 && rooms[0] == roomId;
			}
			finally
			{
				m_CriticalSection.Leave();
			}
		}
	}
}
