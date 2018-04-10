using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Comparers;

namespace ICD.Connect.Routing.Endpoints
{
	public abstract class AbstractSourceDestinationBaseCollection<T> : AbstractOriginatorCollection<T>,
	                                                                   ISourceDestinationBaseCollection<T>
		where T : ISourceDestinationBase
	{
		private readonly Dictionary<EndpointInfo, Dictionary<eConnectionType, List<T>>> m_EndpointCache;
		private readonly SafeCriticalSection m_EndpointCacheSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractSourceDestinationBaseCollection()
		{
			m_EndpointCache = new Dictionary<EndpointInfo, Dictionary<eConnectionType, List<T>>>();
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
				if (!m_EndpointCache.ContainsKey(endpoint))
					return Enumerable.Empty<T>();
				
				if (!m_EndpointCache[endpoint].ContainsKey(type))
					return Enumerable.Empty<T>();

				return m_EndpointCache[endpoint][type];
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
				EndpointInfo endpoint = child.Endpoint;

				if (!m_EndpointCache.ContainsKey(endpoint))
					m_EndpointCache[endpoint] = new Dictionary<eConnectionType, List<T>>();

				foreach (eConnectionType flag in EnumUtils.GetAllFlagCombinationsExceptNone(child.ConnectionType))
				{
					if (!m_EndpointCache[endpoint].ContainsKey(flag))
						m_EndpointCache[endpoint].Add(flag, new List<T>());

					m_EndpointCache[endpoint][flag].AddSorted(child, new OriginatorIdComparer<T>());
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
				EndpointInfo endpoint = child.Endpoint;

				if (!m_EndpointCache.ContainsKey(endpoint))
					return;

				foreach (eConnectionType flag in EnumUtils.GetAllFlagCombinationsExceptNone(child.ConnectionType))
				{
					if (!m_EndpointCache[endpoint].ContainsKey(flag))
						continue;

					m_EndpointCache[endpoint][flag].Remove(child);
				}
			}
			finally
			{
				m_EndpointCacheSection.Leave();
			}
		}
	}
}
