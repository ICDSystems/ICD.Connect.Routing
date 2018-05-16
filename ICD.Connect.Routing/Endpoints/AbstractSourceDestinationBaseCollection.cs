using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings.Comparers;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Routing.Endpoints
{
	public abstract class AbstractSourceDestinationBaseCollection<T> : AbstractOriginatorCollection<T>,
	                                                                   ISourceDestinationBaseCollection<T>
		where T : ISourceDestinationBase
	{
		private readonly Dictionary<DeviceControlInfo, Dictionary<int, Dictionary<eConnectionType, List<T>>>> m_EndpointCache;
		private readonly SafeCriticalSection m_EndpointCacheSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractSourceDestinationBaseCollection()
		{
			m_EndpointCache = new Dictionary<DeviceControlInfo, Dictionary<int, Dictionary<eConnectionType, List<T>>>>();
			m_EndpointCacheSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Gets the child with the given endpoint info.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<T> GetChildren(EndpointInfo endpoint, eConnectionType type)
		{
			m_EndpointCacheSection.Enter();

			try
			{
				DeviceControlInfo deviceControl = endpoint.GetDeviceControlInfo();

				if (!m_EndpointCache.ContainsKey(deviceControl))
					return Enumerable.Empty<T>();
				
				if (!m_EndpointCache[deviceControl].ContainsKey(endpoint.Address))
					return Enumerable.Empty<T>();

				if (!m_EndpointCache[deviceControl][endpoint.Address].ContainsKey(type))
					return Enumerable.Empty<T>();

				return m_EndpointCache[deviceControl][endpoint.Address][type];
			}
			finally
			{
				m_EndpointCacheSection.Leave();
			}
		}

		/// <summary>
		/// Called each time a child is added to the collection before any events are raised.
		/// </summary>
		/// <param name="child"></param>
		protected override void ChildAdded(T child)
		{
			base.ChildAdded(child);

			m_EndpointCacheSection.Enter();

			try
			{
				DeviceControlInfo deviceControl = child.GetDeviceControlInfo();

				if (!m_EndpointCache.ContainsKey(deviceControl))
					m_EndpointCache[deviceControl] = new Dictionary<int, Dictionary<eConnectionType, List<T>>>();

				foreach (int address in child.GetAddresses())
				{
					if (!m_EndpointCache[deviceControl].ContainsKey(address))
						m_EndpointCache[deviceControl].Add(address, new Dictionary<eConnectionType, List<T>>());

					foreach (eConnectionType combination in EnumUtils.GetAllFlagCombinationsExceptNone(child.ConnectionType))
					{
						if (!m_EndpointCache[deviceControl][address].ContainsKey(combination))
							m_EndpointCache[deviceControl][address].Add(combination, new List<T>());

						m_EndpointCache[deviceControl][address][combination].AddSorted(child, new OriginatorIdComparer<T>());
					}
				}
			}
			finally
			{
				m_EndpointCacheSection.Leave();
			}
		}

		/// <summary>
		/// Called each time a child is removed from the collection before any events are raised.
		/// </summary>
		/// <param name="child"></param>
		protected override void ChildRemoved(T child)
		{
			base.ChildRemoved(child);

			m_EndpointCacheSection.Enter();

			try
			{
				DeviceControlInfo deviceControl = child.GetDeviceControlInfo();

				if (!m_EndpointCache.ContainsKey(deviceControl))
					return;

				foreach (int address in child.GetAddresses())
				{
					if (!m_EndpointCache[deviceControl].ContainsKey(address))
						continue;

					foreach (eConnectionType combination in EnumUtils.GetAllFlagCombinationsExceptNone(child.ConnectionType))
					{
						if (!m_EndpointCache[deviceControl][address].ContainsKey(combination))
							continue;

						m_EndpointCache[deviceControl][address][combination].Remove(child);
					}
				}
			}
			finally
			{
				m_EndpointCacheSection.Leave();
			}
		}
	}
}
