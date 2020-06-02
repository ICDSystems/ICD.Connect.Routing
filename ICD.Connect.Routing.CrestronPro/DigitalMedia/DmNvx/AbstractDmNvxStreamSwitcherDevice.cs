using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxBaseClass;
using ICD.Connect.Routing.Devices;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx
{
	public abstract class AbstractDmNvxStreamSwitcherDevice<TSettings> : AbstractRouteSwitcherDevice<TSettings>,
	                                                                     IDmNvxStreamSwitcherDevice
		where TSettings : IDmNvxStreamSwitcherSettings, new()
	{
		/// <summary>
		/// Raised when an input source status changes.
		/// </summary>
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;

		/// <summary>
		/// Raised when the device starts/stops actively using an input, e.g. unroutes an input.
		/// </summary>
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		/// <summary>
		/// Raised when a route changes.
		/// </summary>
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

		private readonly IcdOrderedDictionary<int, NvxEndpointInfo> m_InputEndpoints;
		private readonly IcdOrderedDictionary<int, NvxEndpointInfo> m_OutputEndpoints;
		private readonly Dictionary<DmNvxBaseClassSwitcherControl, NvxEndpointInfo> m_SwitcherToEndpoint;
		private readonly Dictionary<DmNvxBaseClassSwitcherControl, string> m_SwitcherToCachedMulticastAddress;
		private readonly Dictionary<string, IcdHashSet<DmNvxBaseClassSwitcherControl>> m_MulticastAddressToRx;
		private readonly Dictionary<string, DmNvxBaseClassSwitcherControl> m_MulticastAddressToTx;

		private readonly SafeCriticalSection m_ConnectorsSection;

		private readonly SwitcherCache m_SwitcherCache;

		#region Properties

		/// <summary>
		/// Returns true if this switcher handles the primary stream, false for secondary stream.
		/// </summary>
		protected abstract bool IsPrimaryStream { get; }

		/// <summary>
		/// Gets the stream type for this switcher.
		/// </summary>
		private eConnectionType StreamType
		{
			get
			{
				return IsPrimaryStream
					       ? eConnectionType.Audio | eConnectionType.Video
					       : eConnectionType.Audio;
			}
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractDmNvxStreamSwitcherDevice()
		{
			m_InputEndpoints = new IcdOrderedDictionary<int, NvxEndpointInfo>();
			m_OutputEndpoints = new IcdOrderedDictionary<int, NvxEndpointInfo>();
			m_SwitcherToEndpoint = new Dictionary<DmNvxBaseClassSwitcherControl, NvxEndpointInfo>();
			m_SwitcherToCachedMulticastAddress = new Dictionary<DmNvxBaseClassSwitcherControl, string>();
			m_MulticastAddressToRx = new Dictionary<string, IcdHashSet<DmNvxBaseClassSwitcherControl>>();
			m_MulticastAddressToTx = new Dictionary<string, DmNvxBaseClassSwitcherControl>();

			m_ConnectorsSection = new SafeCriticalSection();

			m_SwitcherCache = new SwitcherCache();
			Subscribe(m_SwitcherCache);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnSourceDetectionStateChange = null;
			OnActiveInputsChanged = null;
			OnActiveTransmissionStateChanged = null;
			OnRouteChange = null;

			base.DisposeFinal(disposing);

			Unsubscribe(m_SwitcherCache);
		}

		#region Methods

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			return m_SwitcherCache.GetSourceDetectedState(input, type);
		}

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			return m_SwitcherCache.GetOutputsForInput(input, type);
		}

		/// <summary>
		/// Gets the input routed to the given output matching the given type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Type has multiple flags.</exception>
		public override ConnectorInfo? GetInput(int output, eConnectionType type)
		{
			return m_SwitcherCache.GetInputConnectorInfoForOutput(output, type);
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int input)
		{
			return m_ConnectorsSection.Execute(() => m_InputEndpoints[input].LocalConnector);
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return m_ConnectorsSection.Execute(() => m_InputEndpoints.ContainsKey(input));
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			m_ConnectorsSection.Enter();

			try
			{
				return m_InputEndpoints.Values
				                       .Select(e => e.LocalConnector)
				                       .ToArray(m_InputEndpoints.Count);
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int address)
		{
			return m_ConnectorsSection.Execute(() => m_OutputEndpoints[address].LocalConnector);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return m_ConnectorsSection.Execute(() => m_OutputEndpoints.ContainsKey(output));
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			m_ConnectorsSection.Enter();

			try
			{
				return m_OutputEndpoints.Values
				                        .Select(e => e.LocalConnector)
				                        .ToArray(m_OutputEndpoints.Count);
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}
		}

		/// <summary>
		/// Performs the given route operation.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public override bool Route(RouteOperation info)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			eConnectionType type = info.ConnectionType;
			if (!StreamType.HasFlags(type))
				throw new InvalidOperationException("Unable to route type");

			NvxEndpointInfo inputEndpoint;
			NvxEndpointInfo outputEndpoint;

			m_ConnectorsSection.Enter();

			try
			{
				if (!m_InputEndpoints.TryGetValue(info.LocalInput, out inputEndpoint))
					throw new InvalidOperationException("No input at address");

				if (!m_OutputEndpoints.TryGetValue(info.LocalOutput, out outputEndpoint))
					throw new InvalidOperationException("No output at address");
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}

			return SetInputForOutput(inputEndpoint, outputEndpoint);
		}

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if successfully cleared.</returns>
		public override bool ClearOutput(int output, eConnectionType type)
		{
			if (!StreamType.HasFlags(type))
				throw new InvalidOperationException("Unable to route type");

			NvxEndpointInfo outputEndpoint;

			m_ConnectorsSection.Enter();

			try
			{
				if (!m_OutputEndpoints.TryGetValue(output, out outputEndpoint))
					throw new InvalidOperationException("No output at address");
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}

			return ClearOutput(outputEndpoint);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return true;
		}

		/// <summary>
		/// Routes the input endpoint to the output endpoint.
		/// </summary>
		/// <param name="inputEndpoint"></param>
		/// <param name="outputEndpoint"></param>
		/// <returns></returns>
		protected abstract bool SetInputForOutput(NvxEndpointInfo inputEndpoint, NvxEndpointInfo outputEndpoint);

		/// <summary>
		/// Clears the routing to the given output endpoint.
		/// </summary>
		/// <param name="outputEndpoint"></param>
		/// <returns></returns>
		protected abstract bool ClearOutput(NvxEndpointInfo outputEndpoint);

		#endregion

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			ClearEndpointCache();
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			// Load all of the NVX endpoints first
			factory.LoadOriginators<IDmNvxBaseClassAdapter>();
			factory.LoadOriginators<Connection>();

			base.ApplySettingsFinal(settings, factory);

			BuildEndpointCache();
		}

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(TSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new RouteSwitcherControl(this, 0));
		}

		#endregion

		#region SwitcherCache Callbacks

		private void Subscribe(SwitcherCache cache)
		{
			cache.OnActiveInputsChanged += SwitcherCacheOnActiveInputsChanged;
			cache.OnActiveTransmissionStateChanged += SwitcherCacheOnActiveTransmissionStateChanged;
			cache.OnRouteChange += SwitcherCacheOnRouteChange;
			cache.OnSourceDetectionStateChange += SwitcherCacheOnSourceDetectionStateChange;
		}

		private void Unsubscribe(SwitcherCache cache)
		{
			cache.OnActiveInputsChanged -= SwitcherCacheOnActiveInputsChanged;
			cache.OnActiveTransmissionStateChanged -= SwitcherCacheOnActiveTransmissionStateChanged;
			cache.OnRouteChange -= SwitcherCacheOnRouteChange;
			cache.OnSourceDetectionStateChange -= SwitcherCacheOnSourceDetectionStateChange;
		}

		private void SwitcherCacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs eventArgs)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(eventArgs));
		}

		private void SwitcherCacheOnRouteChange(object sender, RouteChangeEventArgs eventArgs)
		{
			OnRouteChange.Raise(this, new RouteChangeEventArgs(eventArgs));
		}

		private void SwitcherCacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs eventArgs)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(eventArgs));
		}

		private void SwitcherCacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs eventArgs)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(eventArgs));
		}

		#endregion

		#region Endpoint Caching

		/// <summary>
		/// Clears the cached endpoints.
		/// </summary>
		private void ClearEndpointCache()
		{
			m_ConnectorsSection.Enter();

			try
			{
				UnsubscribeSwitchers();

				m_InputEndpoints.Clear();
				m_InputEndpoints.Clear();
				m_SwitcherToEndpoint.Clear();

				m_SwitcherToCachedMulticastAddress.Clear();
				m_MulticastAddressToRx.Clear();
				m_MulticastAddressToTx.Clear();

				// Finally clear routing states
				m_SwitcherCache.Clear();
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}
		}

		/// <summary>
		/// Builds the endpoint cache.
		/// </summary>
		private void BuildEndpointCache()
		{
			m_ConnectorsSection.Enter();

			try
			{
				ClearEndpointCache();

				BuildInputEndpointCache();
				BuildOutputEndpointCache();

				SubscribeSwitchers();

				UpdateRouting();
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}
		}

		/// <summary>
		/// Loops over the input connections to this switcher and builds a table of TX endpoint info.
		/// </summary>
		private void BuildInputEndpointCache()
		{
			m_ConnectorsSection.Enter();

			try
			{
				foreach (Connection inputConnection in GetInputConnections())
				{
					EndpointInfo sourceEndpoint = inputConnection.Source;

					DmNvxBaseClassSwitcherControl switcher = GetSourceControl(inputConnection);
					if (switcher == null)
					{
						Logger.Log(eSeverity.Error, "Unable to support connection from {0}", sourceEndpoint);
						continue;
					}

					NvxEndpointInfo info = new NvxEndpointInfo(inputConnection, switcher);
					if ((IsPrimaryStream && !info.IsPrimaryStream) ||
					    (!IsPrimaryStream && !info.IsSecondaryStream))
					{
						Logger.Log(eSeverity.Error, "Unable to support stream type from {0}", sourceEndpoint);
						continue;
					}

					m_InputEndpoints.Add(info.LocalStreamAddress, info);
					m_SwitcherToEndpoint.Add(switcher, info);
				}
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}
		}

		/// <summary>
		/// Loops over the input connections to this switcher and builds a table of TX endpoint info.
		/// </summary>
		private void BuildOutputEndpointCache()
		{
			m_ConnectorsSection.Enter();

			try
			{
				foreach (Connection outputConnection in GetOutputConnections())
				{
					EndpointInfo destinationEndpoint = outputConnection.Destination;

					DmNvxBaseClassSwitcherControl switcher = GetDestinationControl(outputConnection);
					if (switcher == null)
					{
						Logger.Log(eSeverity.Error, "Unable to support connection to {0}", destinationEndpoint);
						continue;
					}

					NvxEndpointInfo info = new NvxEndpointInfo(outputConnection, switcher);
					if ((IsPrimaryStream && !info.IsPrimaryStream) ||
					    (!IsPrimaryStream && !info.IsSecondaryStream))
					{
						Logger.Log(eSeverity.Error, "Unable to support stream type to {0}", destinationEndpoint);
						continue;
					}

					m_OutputEndpoints.Add(info.LocalStreamAddress, info);
					m_SwitcherToEndpoint.Add(switcher, info);
				}
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}
		}

		/// <summary>
		/// Hack - Getting connections direct from the core instead of the routing graph
		/// because we are still loading settings.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<Connection> GetInputConnections()
		{
			return Core.Originators.GetChildren<Connection>().Where(c => c.Destination.Device == Id);
		}

		/// <summary>
		/// Hack - Getting connections direct from the core instead of the routing graph
		/// because we are still loading settings.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<Connection> GetOutputConnections()
		{
			return Core.Originators.GetChildren<Connection>().Where(c => c.Source.Device == Id);
		}

		/// <summary>
		/// Hack - Getting controls direct from the core instead of the routing graph
		/// because we are still loading settings.
		/// </summary>
		/// <returns></returns>
		private DmNvxBaseClassSwitcherControl GetSourceControl(Connection inputConnection)
		{
			return Core.Originators
			           .GetChild<IDevice>(inputConnection.Source.Device)
			           .Controls
			           .GetControl<DmNvxBaseClassSwitcherControl>(0);
		}

		/// <summary>
		/// Hack - Getting controls direct from the core instead of the routing graph
		/// because we are still loading settings.
		/// </summary>
		/// <returns></returns>
		private DmNvxBaseClassSwitcherControl GetDestinationControl(Connection outputConnection)
		{
			return Core.Originators
			           .GetChild<IDevice>(outputConnection.Destination.Device)
			           .Controls
			           .GetControl<DmNvxBaseClassSwitcherControl>(0);
		}

		/// <summary>
		/// Updates the routing states for the switchers.
		/// </summary>
		private void UpdateRouting()
		{
			foreach (DmNvxBaseClassSwitcherControl switcher in GetCachedSwitchers())
				UpdateRouting(switcher);
		}

		/// <summary>
		/// Updates the routing states for the given switcher.
		/// </summary>
		/// <param name="switcher"></param>
		protected void UpdateRouting(DmNvxBaseClassSwitcherControl switcher)
		{
			if (switcher == null)
				throw new ArgumentNullException("switcher");

			m_ConnectorsSection.Enter();

			try
			{
				NvxEndpointInfo endpointInfo = m_SwitcherToEndpoint.GetDefault(switcher);
				if (endpointInfo == null)
					return;

				string oldAddress = m_SwitcherToCachedMulticastAddress.GetDefault(switcher);
				string newAddress = endpointInfo.LastKnownMulticastAddress;
				if (oldAddress == newAddress)
					return;

				m_SwitcherToCachedMulticastAddress[switcher] = newAddress;

				if (endpointInfo.Tx)
					UpdateRoutingTx(endpointInfo, oldAddress, newAddress);
				else
					UpdateRoutingRx(endpointInfo, oldAddress, newAddress);
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}
		}

		/// <summary>
		/// If the TX multicast address changed we need to unroute all of the RX with the old address
		/// and route all of the RX with the new address.
		/// </summary>
		/// <param name="endpointInfo"></param>
		/// <param name="oldAddress"></param>
		/// <param name="newAddress"></param>
		private void UpdateRoutingTx(NvxEndpointInfo endpointInfo, string oldAddress, string newAddress)
		{
			if (endpointInfo == null)
				throw new ArgumentNullException("endpointInfo");

			if (newAddress == oldAddress)
				return;

			m_ConnectorsSection.Enter();

			try
			{
				// Update the cache
				if (oldAddress != null)
					m_MulticastAddressToTx.Remove(oldAddress);
				if (newAddress != null)
					m_MulticastAddressToTx[newAddress] = endpointInfo.Switcher;

				// Unroute all of the RXs pointing at the old address
				IcdHashSet<DmNvxBaseClassSwitcherControl> rxs;
				if (oldAddress != null && m_MulticastAddressToRx.TryGetValue(oldAddress, out rxs))
				{
					foreach (DmNvxBaseClassSwitcherControl rx in rxs)
					{
						// Get the RX address
						NvxEndpointInfo rxEndpoint = rx == null ? null : m_SwitcherToEndpoint.GetDefault(rx);
						if (rxEndpoint == null)
							continue;

						int outputAddress = rxEndpoint.LocalStreamAddress;

						// Set input for output
						m_SwitcherCache.SetInputForOutput(outputAddress, null, StreamType);
					}
				}

				// Route all of the RXs pointing at the new address
				if (newAddress != null && m_MulticastAddressToRx.TryGetValue(newAddress, out rxs))
				{
					foreach (DmNvxBaseClassSwitcherControl rx in rxs)
					{
						// Get the RX address
						NvxEndpointInfo rxEndpoint = rx == null ? null : m_SwitcherToEndpoint.GetDefault(rx);
						if (rxEndpoint == null)
							continue;

						int outputAddress = rxEndpoint.LocalStreamAddress;

						// Set input for output
						m_SwitcherCache.SetInputForOutput(outputAddress, endpointInfo.LocalStreamAddress, StreamType);
					}
				}
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}
		}

		/// <summary>
		/// If the RX multicast address changed we need to route the TX to it.
		/// </summary>
		/// <param name="endpointInfo"></param>
		/// <param name="oldAddress"></param>
		/// <param name="newAddress"></param>
		private void UpdateRoutingRx(NvxEndpointInfo endpointInfo, string oldAddress, string newAddress)
		{
			if (endpointInfo == null)
				throw new ArgumentNullException("endpointInfo");

			if (newAddress == oldAddress)
				return;

			m_ConnectorsSection.Enter();

			try
			{
				// Update the cache
				IcdHashSet<DmNvxBaseClassSwitcherControl> switchers;
				if (oldAddress != null && m_MulticastAddressToRx.TryGetValue(oldAddress, out switchers))
					switchers.Remove(endpointInfo.Switcher);

				if (newAddress != null)
				{
					if (!m_MulticastAddressToRx.TryGetValue(newAddress, out switchers))
					{
						switchers = new IcdHashSet<DmNvxBaseClassSwitcherControl>();
						m_MulticastAddressToRx.Add(newAddress, switchers);
					}

					switchers.Add(endpointInfo.Switcher);
				}

				// Get the RX address
				int outputAddress = endpointInfo.LocalStreamAddress;

				// Get the TX address
				DmNvxBaseClassSwitcherControl tx = newAddress == null ? null : m_MulticastAddressToTx.GetDefault(newAddress);
				NvxEndpointInfo txEndpoint = tx == null ? null : m_SwitcherToEndpoint.GetDefault(tx);
				int? inputAddress = txEndpoint == null ? (int?)null : txEndpoint.LocalStreamAddress;

				// Set input for output
				m_SwitcherCache.SetInputForOutput(outputAddress, inputAddress, StreamType);
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}
		}

		#endregion

		#region Switcher Callbacks

		/// <summary>
		/// Subscribe to the connected switcher events.
		/// </summary>
		private void SubscribeSwitchers()
		{
			foreach (DmNvxBaseClassSwitcherControl switcher in GetCachedSwitchers())
				Subscribe(switcher);
		}

		/// <summary>
		/// Unsubscribe from the connected switcher events.
		/// </summary>
		private void UnsubscribeSwitchers()
		{
			foreach (DmNvxBaseClassSwitcherControl switcher in GetCachedSwitchers())
				Unsubscribe(switcher);
		}

		/// <summary>
		/// Gets all of the TX and RX switchers that have been cached.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<DmNvxBaseClassSwitcherControl> GetCachedSwitchers()
		{
			m_ConnectorsSection.Enter();

			try
			{
				IEnumerable<DmNvxBaseClassSwitcherControl> txs = m_InputEndpoints.Select(e => e.Value.Switcher);
				IEnumerable<DmNvxBaseClassSwitcherControl> rxs = m_OutputEndpoints.Select(e => e.Value.Switcher);

				return txs.Concat(rxs).ToArray();
			}
			finally
			{
				m_ConnectorsSection.Leave();
			}
		}

		/// <summary>
		/// Subscribe to the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		protected virtual void Subscribe(DmNvxBaseClassSwitcherControl switcher)
		{
		}

		/// <summary>
		/// Unsubscribe from the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		protected virtual void Unsubscribe(DmNvxBaseClassSwitcherControl switcher)
		{
		}

		#endregion
	}
}
