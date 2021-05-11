using System;
using System.Linq;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Extensions;
using ICD.Connect.Routing.RoutingGraphs;
using ICD.Connect.Routing.Utils;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.CrestronPro.Transmitters
{
#if SIMPLSHARP
	public abstract class AbstractEndpointTransmitterSwitcherBaseAdapter<TTransmitter, TSettings> : AbstractEndpointTransmitterBaseAdapter<TTransmitter, TSettings>, IEndpointTransmitterSwitcherBaseAdapter
		where TTransmitter : Crestron.SimplSharpPro.DM.Endpoints.Transmitters.EndpointTransmitterBase
#else
	public abstract class AbstractEndpointTransmitterSwitcherBaseAdapter<TSettings> : AbstractEndpointTransmitterBaseAdapter<TSettings>, IEndpointTransmitterSwitcherBaseAdapter
#endif
		where TSettings : IEndpointTransmitterSwitcherBaseAdapterSettings, new()
	{
		#region Events

		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

		#endregion

		#region Fields

		private readonly SwitcherCache m_Cache;

		#endregion

		#region Properties

		protected SwitcherCache SwitcherCache { get { return m_Cache; } }
        
		/// <summary>
		/// Used to determine if transmitters should enable auto routing.
		/// Set in start settings final.
		/// </summary>
		protected bool UseAutoRouting { get; private set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractEndpointTransmitterSwitcherBaseAdapter()
		{
			m_Cache = new SwitcherCache();
			Subscribe(m_Cache);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			Unsubscribe(SwitcherCache);
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

		/// <summary>
		/// Performs the given route operation.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public abstract bool Route(RouteOperation info);

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if successfully cleared.</returns>
		public abstract bool ClearOutput(int output, eConnectionType type);


		/// <summary>
		/// Sets the transmitter AutoRouting state if appropriate 
		/// </summary>
		private void SetTransmitterAutoRouting()
		{
#if SIMPLSHARP
			if (UseAutoRouting && Transmitter != null && Transmitter.IsOnline)
				SetTransmitterAutoRoutingFinal();
#endif
		}


#if SIMPLSHARP
		/// <summary>
		/// Override to implement AutoRouting on the transmitter
		/// </summary>
		protected abstract void SetTransmitterAutoRoutingFinal();

#endif

		#endregion

		#region Settings

		protected override void StartSettingsFinal()
		{
			base.StartSettingsFinal();

			// If there are no input connections in the routing graph transmitters should use auto routing.
			IRoutingGraph routingGraph;
			if (Core.TryGetRoutingGraph(out routingGraph) && routingGraph != null)
				UseAutoRouting = routingGraph.Connections.All(c => c.Destination.Device != Id);
			else
				UseAutoRouting = true;

			SetTransmitterAutoRouting();
		}

		#endregion

		#region Console

		public override void BuildConsoleStatus(API.Nodes.AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("UseAutoRouting", UseAutoRouting);
		}

		#endregion

		#region Transmitter Callbacks
#if SIMPLSHARP
		protected override void TransmitterOnlineStatusChange(Crestron.SimplSharpPro.GenericBase currentDevice, Crestron.SimplSharpPro.OnlineOfflineEventArgs args)
		{
			base.TransmitterOnlineStatusChange(currentDevice, args);

			if (IsOnline)
				SetTransmitterAutoRouting();
		}
#endif
		#endregion

		#region Event Invocation

		private void RaiseRouteChange(int? oldInput, int? newInput, int output, eConnectionType type)
		{
			OnRouteChange.Raise(this, new RouteChangeEventArgs(oldInput, newInput, output, type));
		}

		#endregion

		#region Switcher Cache Callbacks

		private void Subscribe(SwitcherCache switcherCache)
		{
			switcherCache.OnRouteChange += SwitcherCacheOnRouteChange;
			switcherCache.OnActiveInputsChanged += SwitcherCacheOnActiveInputsChanged;
			switcherCache.OnActiveTransmissionStateChanged += SwitcherCacheOnActiveTransmissionStateChanged;
			switcherCache.OnSourceDetectionStateChange += SwitcherCacheOnSourceDetectionStateChange;
		}

		private void Unsubscribe(SwitcherCache switcherCache)
		{
			switcherCache.OnRouteChange -= SwitcherCacheOnRouteChange;
			switcherCache.OnActiveInputsChanged -= SwitcherCacheOnActiveInputsChanged;
			switcherCache.OnActiveTransmissionStateChanged -= SwitcherCacheOnActiveTransmissionStateChanged;
			switcherCache.OnSourceDetectionStateChange -= SwitcherCacheOnSourceDetectionStateChange;
		}

		private void SwitcherCacheOnRouteChange(object sender, RouteChangeEventArgs args)
		{
			RaiseRouteChange(args.OldInput, args.NewInput, args.Output, args.Type);
		}

		private void SwitcherCacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs args)
		{
			RaiseActiveInputsChanged(args.Input, args.Type, args.Active);
		}

		private void SwitcherCacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs args)
		{
			RaiseActiveTransmissionStateChanged(args.Output, args.Type, args.State);
		}

		private void SwitcherCacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs args)
		{
			RaiseSourceDetectionStateChange(args.Input, args.Type, args.State);
		}

		#endregion
	}
}