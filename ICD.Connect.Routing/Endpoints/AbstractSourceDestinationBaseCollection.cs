using System;
using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Endpoints
{
	public abstract class AbstractSourceDestinationBaseCollection<T> : AbstractOriginatorCollection<T>,
	                                                                   ISourceDestinationBaseCollection<T>
		where T : ISourceDestinationBase
	{
		private readonly Dictionary<EndpointInfo, Dictionary<eConnectionType, T>> m_EndpointCache;
		private readonly SafeCriticalSection m_EndpointCacheSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractSourceDestinationBaseCollection()
		{
			m_EndpointCache = new Dictionary<EndpointInfo, Dictionary<eConnectionType, T>>();
			m_EndpointCacheSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Gets the child with the given endpoint info.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public T GetChild(EndpointInfo endpoint, eConnectionType type)
		{
			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Connection type has multiple flags");

			return m_EndpointCacheSection.Execute(() => m_EndpointCache[endpoint][type]);
		}

		/// <summary>
		/// Gets the child with the given endpoint info.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="type"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		public bool TryGetChild(EndpointInfo endpoint, eConnectionType type, out T output)
		{
			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Connection type has multiple flags");

			output = default(T);

			m_EndpointCacheSection.Enter();

			try
			{
				return m_EndpointCache.ContainsKey(endpoint) && m_EndpointCache[endpoint].TryGetValue(type, out output);
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

			UpdateEndpointCache();
		}

		/// <summary>
		/// Update the endpoint -> source/destination cache.
		/// </summary>
		private void UpdateEndpointCache()
		{
			m_EndpointCacheSection.Enter();

			try
			{
				m_EndpointCache.Clear();

				foreach (T item in GetChildren())
				{
					EndpointInfo endpoint = item.Endpoint;

					if (!m_EndpointCache.ContainsKey(endpoint))
						m_EndpointCache[endpoint] = new Dictionary<eConnectionType, T>();

					foreach (eConnectionType type in EnumUtils.GetFlagsExceptNone(item.ConnectionType))
						m_EndpointCache[endpoint].Add(type, item);
				}
			}
			finally
			{
				m_EndpointCacheSection.Leave();
			}
		}
	}
}
