using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxBaseClass;
using ICD.Connect.Routing.Devices;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx
{
	public abstract class AbstractDmNvxStreamSwitcherDevice<TSettings> : AbstractRouteSwitcherDevice<TSettings>, IDmNvxStreamSwitcherDevice
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

		private ICore m_CachedCore;

		/// <summary>
		/// Gets the core.
		/// </summary>
		public ICore Core { get { return m_CachedCore = m_CachedCore ?? ServiceProvider.GetService<ICore>(); } }

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
			m_SwitcherCache.OnActiveInputsChanged += SwitcherCacheOnActiveInputsChanged;
			m_SwitcherCache.OnActiveTransmissionStateChanged += SwitcherCacheOnActiveTransmissionStateChanged;
			m_SwitcherCache.OnRouteChange += SwitcherCacheOnRouteChange;
			m_SwitcherCache.OnSourceDetectionStateChange += SwitcherCacheOnSourceDetectionStateChange;

			Controls.Add(new RouteSwitcherControl(this, 0));
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

		#endregion

		#region SwitcherCache Callbacks

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
			foreach (Connection inputConnection in GetInputConnections())
			{
				EndpointInfo sourceEndpoint = inputConnection.Source;

				DmNvxBaseClassSwitcherControl switcher = GetSourceControl(inputConnection);
				if (switcher == null)
				{
					Log(eSeverity.Error, "Unable to support connection from {0}", sourceEndpoint);
					continue;
				}

				NvxEndpointInfo info = new NvxEndpointInfo(inputConnection, switcher);
				if ((IsPrimaryStream && !info.IsPrimaryStream) ||
				    (!IsPrimaryStream && !info.IsSecondaryStream))
				{
					Log(eSeverity.Error, "Unable to support stream type from {0}", sourceEndpoint);
					continue;
				}

				IcdConsole.PrintLine(eConsoleColor.Magenta, "Input {0} - {1}", info.LocalStreamAddress, info);

				m_InputEndpoints.Add(info.LocalStreamAddress, info);
			}
		}

		/// <summary>
		/// Loops over the input connections to this switcher and builds a table of TX endpoint info.
		/// </summary>
		private void BuildOutputEndpointCache()
		{
			foreach (Connection outputConnection in GetOutputConnections())
			{
				EndpointInfo destinationEndpoint = outputConnection.Destination;

				DmNvxBaseClassSwitcherControl switcher = GetDestinationControl(outputConnection);
				if (switcher == null)
				{
					Log(eSeverity.Error, "Unable to support connection to {0}", destinationEndpoint);
					continue;
				}

				NvxEndpointInfo info = new NvxEndpointInfo(outputConnection, switcher);
				if ((IsPrimaryStream && !info.IsPrimaryStream) ||
				    (!IsPrimaryStream && !info.IsSecondaryStream))
				{
					Log(eSeverity.Error, "Unable to support stream type to {0}", destinationEndpoint);
					continue;
				}

				IcdConsole.PrintLine(eConsoleColor.Magenta, "Output {0} - {1}", info.LocalStreamAddress, info);

				m_OutputEndpoints.Add(info.LocalStreamAddress, info);
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
			           .GetChild<IDeviceBase>(inputConnection.Source.Device)
			           .Controls.GetControl<DmNvxBaseClassSwitcherControl>(0);
		}

		/// <summary>
		/// Hack - Getting controls direct from the core instead of the routing graph
		/// because we are still loading settings.
		/// </summary>
		/// <returns></returns>
		private DmNvxBaseClassSwitcherControl GetDestinationControl(Connection outputConnection)
		{
			return Core.Originators
					   .GetChild<IDeviceBase>(outputConnection.Destination.Device)
					   .Controls.GetControl<DmNvxBaseClassSwitcherControl>(0);
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
		private void UpdateRouting(DmNvxBaseClassSwitcherControl switcher)
		{
			if (switcher == null)
				throw new ArgumentNullException("switcher");

			string oldUrl = m_SwitcherToCachedMulticastAddress.GetDefault(switcher);
			string newUrl = switcher.ServerUrl;

			if (oldUrl == newUrl)
				return;

			m_SwitcherToCachedMulticastAddress[switcher] = switcher.ServerUrl;

			NvxEndpointInfo endpointInfo = m_ConnectorsSection.Execute(() => m_SwitcherToEndpoint.GetDefault(switcher));
			if (endpointInfo == null)
				return;

			bool tx = endpointInfo.Tx;

			// If the TX server url changed we need to unroute all of the RX with the old url
			// and route all of the RX with the new url.
			if (tx)
			{

			}
			// If the RX server url changed we need to route the TX to it.
			else
			{

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
		private void Subscribe(DmNvxBaseClassSwitcherControl switcher)
		{
			switcher.OnServerUrlChange += SwitcherOnServerUrlChange;
		}

		/// <summary>
		/// Unsubscribe from the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		private void Unsubscribe(DmNvxBaseClassSwitcherControl switcher)
		{
			switcher.OnServerUrlChange -= SwitcherOnServerUrlChange;
		}

		/// <summary>
		/// Called when an endpoint url changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="stringEventArgs"></param>
		private void SwitcherOnServerUrlChange(object sender, StringEventArgs stringEventArgs)
		{
			DmNvxBaseClassSwitcherControl switcher = sender as DmNvxBaseClassSwitcherControl;
			if (switcher != null)
				UpdateRouting(switcher);
		}

		#endregion
	}
}
