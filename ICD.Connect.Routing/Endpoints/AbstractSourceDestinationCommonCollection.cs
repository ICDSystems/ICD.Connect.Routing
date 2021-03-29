using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Comparers;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings.Originators;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Endpoints
{
	public abstract class AbstractSourceDestinationCommonCollection<T> : AbstractOriginatorCollection<T>,
	                                                                   ISourceDestinationCommonCollection<T>
		where T : class, ISourceDestinationCommon
	{
		/// <summary>
		/// Raised when the disabled state of a source destination base changes.
		/// </summary>
		public event EventHandler<SourceDestinationBaseDisabledStateChangedEventArgs> OnSourceDestinationBaseDisabledStateChanged;

		private readonly IcdSortedDictionary<int, List<T>> m_DeviceCache;
		private readonly IcdSortedDictionary<EndpointInfo, List<T>> m_EndpointCache;
		private readonly IcdSortedDictionary<EndpointInfo, IcdSortedDictionary<eConnectionType, List<T>>> m_EndpointTypeCache;
		private readonly SafeCriticalSection m_CacheSection;

		private readonly PredicateComparer<T, int> m_ChildIdComparer;

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractSourceDestinationCommonCollection()
		{
			m_DeviceCache = new IcdSortedDictionary<int, List<T>>();
			m_EndpointCache = new IcdSortedDictionary<EndpointInfo, List<T>>();
			m_EndpointTypeCache = new IcdSortedDictionary<EndpointInfo, IcdSortedDictionary<eConnectionType, List<T>>>();
			m_CacheSection = new SafeCriticalSection();
			m_ChildIdComparer = new PredicateComparer<T, int>(c => c.Id);
		}

		/// <summary>
		/// Gets the child with the given endpoint info.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <returns></returns>
		public IEnumerable<T> GetChildren(EndpointInfo endpoint)
		{
			m_CacheSection.Enter();

			try
			{
				List<T> children;
				return m_EndpointCache.TryGetValue(endpoint, out children)
					       ? children.ToArray(children.Count)
					       : Enumerable.Empty<T>();
			}
			finally
			{
				m_CacheSection.Leave();
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
			m_CacheSection.Enter();

			try
			{
				IcdSortedDictionary<eConnectionType, List<T>> types;
				if (!m_EndpointTypeCache.TryGetValue(endpoint, out types))
					return Enumerable.Empty<T>();

				List<T> children;
				return types.TryGetValue(type, out children)
					       ? children.ToArray(children.Count)
					       : Enumerable.Empty<T>();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Gets the children with the given device id.
		/// </summary>
		/// <param name="deviceId"></param>
		/// <returns></returns>
		public IEnumerable<T> GetChildrenForDevice(int deviceId)
		{
			m_CacheSection.Enter();

			try
			{
				List<T> children;
				return m_DeviceCache.TryGetValue(deviceId, out children)
					       ? children.ToArray(children.Count)
					       : Enumerable.Empty<T>();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		#region Private Methods

		/// <summary>
		/// Called when children are added to the collection before any events are raised.
		/// </summary>
		/// <param name="children"></param>
		protected override void ChildrenAdded(IEnumerable<T> children)
		{
			IList<T> childList = children as IList<T> ?? children.ToArray();

			base.ChildrenRemoved(childList);

			foreach (T child in childList)
				Subscribe(child);

			RebuildCache();
		}

		/// <summary>
		/// Called when children are removed from the collection before any events are raised.
		/// </summary>
		/// <param name="children"></param>
		protected override void ChildrenRemoved(IEnumerable<T> children)
		{
			IList<T> childList = children as IList<T> ?? children.ToArray();

			base.ChildrenRemoved(childList);

			foreach (T child in childList)
				Unsubscribe(child);

			RebuildCache();
		}

		private void RebuildCache()
		{
			m_CacheSection.Enter();

			try
			{
				m_DeviceCache.Clear();
				m_EndpointCache.Clear();
				m_EndpointTypeCache.Clear();

				foreach (T child in GetChildren())
				{
					// Device Cache
					m_DeviceCache.GetOrAddNew(child.Device).InsertSorted(child, m_ChildIdComparer);

					foreach (EndpointInfo endpoint in child.GetEndpoints())
					{
						// Endpoint Cache
						m_EndpointCache.GetOrAddNew(endpoint).InsertSorted(child, m_ChildIdComparer);

						// Endpoint Typed Cache
						IcdSortedDictionary<eConnectionType, List<T>> types = m_EndpointTypeCache.GetOrAddNew(endpoint);
						foreach (eConnectionType combination in EnumUtils.GetAllFlagCombinationsExceptNone(child.ConnectionType))
							types.GetOrAddNew(combination).InsertSorted(child, m_ChildIdComparer);
					}

					Subscribe(child);
				}
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		#endregion

		#region Child Callbacks

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
				       new SourceDestinationBaseDisabledStateChangedEventArgs((ISourceDestinationCommon)sender,
				                                                              args.Data));
		}

		#endregion
	}
}
