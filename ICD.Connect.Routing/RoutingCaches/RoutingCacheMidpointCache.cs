using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;

namespace ICD.Connect.Routing.RoutingCaches
{
	public sealed class RoutingCacheMidpointCache
	{
		/// <summary>
		/// Maps last known midpoint configurations: Output -> Flag -> Input
		/// </summary>
		private readonly Dictionary<EndpointInfo, Dictionary<eConnectionType, EndpointInfo>> m_MidpointOutputConfiguration;

		/// <summary>
		/// Maps last known midpoint configurations: Input -> Flag -> Output
		/// </summary>
		private readonly Dictionary<EndpointInfo, Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>> m_MidpointInputConfiguration;

		private readonly SafeCriticalSection m_CacheSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		public RoutingCacheMidpointCache()
		{
			m_MidpointOutputConfiguration = new Dictionary<EndpointInfo, Dictionary<eConnectionType, EndpointInfo>>();
			m_MidpointInputConfiguration = new Dictionary<EndpointInfo, Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>>();

			m_CacheSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Clears the cached values.
		/// </summary>
		public void Clear()
		{
			m_CacheSection.Enter();

			try
			{
				m_MidpointInputConfiguration.Clear();
				m_MidpointOutputConfiguration.Clear();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Updates the cache of known midpoint Output -> Type -> Input mapping.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="oldInput"></param>
		/// <param name="newInput"></param>
		/// <param name="output"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		public bool SetCachedInputForOutput(IRouteMidpointControl control, int? oldInput, int? newInput, int output,
											 eConnectionType flag)
		{
			if (control == null)
				throw new ArgumentNullException("control");

			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("Connection type must be a single flag", "flag");

			// No change
			if (newInput == oldInput)
				return false;

			// Update the midpoint input/output mapping
			EndpointInfo outputEndpoint = control.GetOutputEndpointInfo(output);

			bool change = false;

			m_CacheSection.Enter();

			try
			{
				// Output configuration
				Dictionary<eConnectionType, EndpointInfo> flagCache;
				if (!m_MidpointOutputConfiguration.TryGetValue(outputEndpoint, out flagCache))
				{
					flagCache = new Dictionary<eConnectionType, EndpointInfo>();
					m_MidpointOutputConfiguration.Add(outputEndpoint, flagCache);
				}

				if (newInput.HasValue)
				{
					EndpointInfo inputEndpoint = control.GetInputEndpointInfo(newInput.Value);

					EndpointInfo existing;
					if (!(flagCache.TryGetValue(flag, out existing) && existing == inputEndpoint))
					{
						flagCache[flag] = inputEndpoint;
						change = true;
					}
				}
				else
				{
					change = flagCache.Remove(flag);
				}

				// Input configuration
				if (oldInput.HasValue)
				{
					EndpointInfo oldInputEndpoint = control.GetInputEndpointInfo(oldInput.Value);

					Dictionary<eConnectionType, IcdHashSet<EndpointInfo>> inputFlagCache;
					IcdHashSet<EndpointInfo> outputEndpoints;
					if (m_MidpointInputConfiguration.TryGetValue(oldInputEndpoint, out inputFlagCache) &&
						inputFlagCache.TryGetValue(flag, out outputEndpoints))
						change |= outputEndpoints.Remove(outputEndpoint);
				}

				if (newInput.HasValue)
				{
					EndpointInfo newInputEndpoint = control.GetInputEndpointInfo(newInput.Value);

					Dictionary<eConnectionType, IcdHashSet<EndpointInfo>> inputFlagCache;
					if (!m_MidpointInputConfiguration.TryGetValue(newInputEndpoint, out inputFlagCache))
					{
						inputFlagCache = new Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>();
						m_MidpointInputConfiguration.Add(newInputEndpoint, inputFlagCache);
					}

					IcdHashSet<EndpointInfo> outputEndpoints;
					if (!inputFlagCache.TryGetValue(flag, out outputEndpoints))
					{
						outputEndpoints = new IcdHashSet<EndpointInfo>();
						inputFlagCache.Add(flag, outputEndpoints);
					}

					change |= outputEndpoints.Add(outputEndpoint);
				}
			}
			finally
			{
				m_CacheSection.Leave();
			}

			return change;
		}

		public IEnumerable<EndpointInfo> GetCachedOutputsForInput(EndpointInfo inputEndpoint, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("Connection type must be a single flag", "flag");

			m_CacheSection.Enter();

			try
			{
				Dictionary<eConnectionType, IcdHashSet<EndpointInfo>> typeCache;
				if (!m_MidpointInputConfiguration.TryGetValue(inputEndpoint, out typeCache))
					return Enumerable.Empty<EndpointInfo>();

				IcdHashSet<EndpointInfo> output;
				return typeCache.TryGetValue(flag, out output) ? output.ToArray(output.Count) : Enumerable.Empty<EndpointInfo>();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		public EndpointInfo? GetCachedInputForOutput(EndpointInfo outputEndpoint, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("Connection type must be a single flag", "flag");

			m_CacheSection.Enter();

			try
			{
				Dictionary<eConnectionType, EndpointInfo> typeCache;
				if (!m_MidpointOutputConfiguration.TryGetValue(outputEndpoint, out typeCache))
					return null;

				EndpointInfo output;
				return typeCache.TryGetValue(flag, out output) ? output : (EndpointInfo?)null;
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}
	}
}
