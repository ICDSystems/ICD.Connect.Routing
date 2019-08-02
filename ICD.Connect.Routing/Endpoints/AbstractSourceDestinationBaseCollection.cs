using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Comparers;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.RoutingGraphs;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Endpoints
{
	public abstract class AbstractSourceDestinationBaseCollection<T> : AbstractOriginatorCollection<T>,
	                                                                   ISourceDestinationBaseCollection<T>
		where T : class, ISourceDestinationBase
	{
		/// <summary>
		/// Raised when the disabled state of a source destination base changes.
		/// </summary>
		public event EventHandler<SourceDestinationBaseDisabledStateChangedEventArgs> OnSourceDestinationBaseDisabledStateChanged;

		private readonly IcdOrderedDictionary<EndpointInfo, List<T>> m_EndpointCache;
		private readonly IcdOrderedDictionary<EndpointInfo, IcdOrderedDictionary<eConnectionType, List<T>>> m_EndpointTypeCache;
		private readonly SafeCriticalSection m_EndpointCacheSection;

		private readonly PredicateComparer<T, int> m_ChildIdComparer;


		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractSourceDestinationBaseCollection()
		{
			m_EndpointCache = new IcdOrderedDictionary<EndpointInfo, List<T>>();
			m_EndpointTypeCache = new IcdOrderedDictionary<EndpointInfo, IcdOrderedDictionary<eConnectionType, List<T>>>();
			m_EndpointCacheSection = new SafeCriticalSection();
			m_ChildIdComparer = new PredicateComparer<T, int>(c => c.Id);
		}

		/// <summary>
		/// Gets the child with the given endpoint info.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <returns></returns>
		public IEnumerable<T> GetChildren(EndpointInfo endpoint)
		{
			m_EndpointCacheSection.Enter();

			try
			{
				List<T> children;
				return m_EndpointCache.TryGetValue(endpoint, out children)
					       ? children.ToArray(children.Count)
					       : Enumerable.Empty<T>();
			}
			finally
			{
				m_EndpointCacheSection.Leave();
			}
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
				IcdOrderedDictionary<eConnectionType, List<T>> types;
				if (!m_EndpointTypeCache.TryGetValue(endpoint, out types))
					return Enumerable.Empty<T>();

				List<T> children;
				return types.TryGetValue(type, out children) ? children.ToArray(children.Count) : Enumerable.Empty<T>();
			}
			finally
			{
				m_EndpointCacheSection.Leave();
			}
		}

		/// <summary>
		/// Called when children are added to the collection before any events are raised.
		/// </summary>
		/// <param name="children"></param>
		protected override void ChildrenAdded(IEnumerable<T> children)
		{
			m_EndpointCacheSection.Enter();

			try
			{
				foreach (T child in children)
				{
					foreach (EndpointInfo endpoint in child.GetEndpoints())
					{
						// Add to the cache
						List<T> childCache;
						if (!m_EndpointCache.TryGetValue(endpoint, out childCache))
						{
							childCache = new List<T>();
							m_EndpointCache[endpoint] = childCache;
						}

						childCache.AddSorted(child, m_ChildIdComparer);

						// Add to the typed cache
						IcdOrderedDictionary<eConnectionType, List<T>> types;
						if (!m_EndpointTypeCache.TryGetValue(endpoint, out types))
						{
							types = new IcdOrderedDictionary<eConnectionType, List<T>>();
							m_EndpointTypeCache[endpoint] = types;
						}

						foreach (eConnectionType combination in EnumUtils.GetAllFlagCombinationsExceptNone(child.ConnectionType))
						{
							List<T> childTypeCache;
							if (!types.TryGetValue(combination, out childTypeCache))
							{
								childTypeCache = new List<T>();
								types[combination] = childTypeCache;
							}

							childTypeCache.AddSorted(child, m_ChildIdComparer);
						}
					}

					Subscribe(child);
				}
			}
			finally
			{
				m_EndpointCacheSection.Leave();
			}
		}

		/// <summary>
		/// Called when children are removed from the collection before any events are raised.
		/// </summary>
		/// <param name="children"></param>
		protected override void ChildrenRemoved(IEnumerable<T> children)
		{
			m_EndpointCacheSection.Enter();

			try
			{
				foreach (T child in children)
				{
					foreach (EndpointInfo endpoint in child.GetEndpoints())
					{
						// Remove from the cache
						List<T> childCache;
						if (m_EndpointCache.TryGetValue(endpoint, out childCache))
							childCache.Remove(child);

						// Remove from the typed cache
						IcdOrderedDictionary<eConnectionType, List<T>> types;
						if (m_EndpointTypeCache.TryGetValue(endpoint, out types))
						{
							foreach (KeyValuePair<eConnectionType, List<T>> kvp in types)
								kvp.Value.Remove(child);
						}
					}

					Unsubscribe(child);
				}
			}
			finally
			{
				m_EndpointCacheSection.Leave();
			}
		}

		private void Subscribe(T child)
		{
			child.OnDisableStateChanged += ChildOnDisableStateChanged;
		}

		private void Unsubscribe(T child)
		{
			child.OnDisableStateChanged -= ChildOnDisableStateChanged;
		}

		private void ChildOnDisableStateChanged(object sender, BoolEventArgs args)
		{
			OnSourceDestinationBaseDisabledStateChanged
				.Raise(this,
				       new SourceDestinationBaseDisabledStateChangedEventArgs((ISourceDestinationBase)sender,
				                                                              args.Data));
		}
	}
}
