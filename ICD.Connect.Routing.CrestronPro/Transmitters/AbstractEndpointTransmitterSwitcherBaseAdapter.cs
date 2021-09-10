using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
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

		private bool m_UseAutoRouting;

		private bool m_SourceDetectionState;

		private readonly Dictionary<int, eConnectionType> m_ActiveTransmissionCache;

		private readonly SafeCriticalSection m_ActiveTransmissionCacheSection;

		#endregion

		#region Properties

		protected SwitcherCache SwitcherCache { get { return m_Cache; } }
        
		/// <summary>
		/// Used to determine if transmitters should enable auto routing.
		/// Set in start settings final.
		/// </summary>
		protected bool UseAutoRouting 
		{ 
			get { return m_UseAutoRouting; }
			private set
			{
				if (m_UseAutoRouting == value)
					return;

				m_UseAutoRouting = value;

				// If using autorouting, set active transmission based on source detect
				// If not, set active transmission based on switcher cache
				if (value)
					SetActiveTransmissionState(SourceDetectionState);
				else
					SetActiveTransmissionStateFromSwitcherCache();
			} 
		}

		/// <summary>
		/// Source detection state is a simple "is the transmitter detecting any source"
		/// This is used for active transmission state when in auto-routing mode
		/// </summary>
		protected bool SourceDetectionState
		{
			get { return m_SourceDetectionState; }
			private set
			{
				if (m_SourceDetectionState == value)
					return;

				m_SourceDetectionState = value;

				if (UseAutoRouting)
					SetActiveTransmissionState(value);
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractEndpointTransmitterSwitcherBaseAdapter()
		{
			m_ActiveTransmissionCache = new Dictionary<int, eConnectionType>();
			m_ActiveTransmissionCacheSection = new SafeCriticalSection();
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
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .Select(t => GetSignalDetectedState(input, t))
				                .Unanimous(false);
			}

			return m_Cache.GetSourceDetectedState(input, type);
		}

		/// <summary>
		/// Returns true if the device is actively transmitting on the given output.
		/// This is NOT the same as sending video, since some devices may send an
		/// idle signal by default.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetActiveTransmissionState(int output, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .Select(f => GetActiveTransmissionState(output, f))
				                .Unanimous(false);
			}

			if (!ContainsOutput(output))
			{
				string message = string.Format("{0} has no {1} output at address {2}", this, type, output);
				throw new ArgumentOutOfRangeException("output", message);
			}

			m_ActiveTransmissionCacheSection.Enter();

			try
			{
				return m_ActiveTransmissionCache.GetDefault(output).HasFlag(type);
			}
			finally
			{
				m_ActiveTransmissionCacheSection.Leave();
			}
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

		#region Source Detection State

		/// <summary>
		/// Called to update source detection state from GetSourceDetectionState
		/// </summary>
		protected void UpdateSourceDetectionState()
		{
			SourceDetectionState = GetSourceDetectionState();
		}

		/// <summary>
		/// Gets the source detect state from the Tx
		/// This is just a simple "is a laptop detected"
		/// Used for ActiveTransmission in AutoRouting mode
		/// Override to support additional inputs on a Tx
		/// </summary>
		/// <returns></returns>
		protected abstract bool GetSourceDetectionState();

		#endregion

		#region Active Transmission State

		/// <summary>
		/// Set the given output connection types active transmission state to true
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		private void AddActiveTransmissionState(int output, eConnectionType type)
		{
			if (type == eConnectionType.None)
				return;

			eConnectionType addedTypes;
			
			m_ActiveTransmissionCacheSection.Enter();

			try
			{
				// Get the old state
				eConnectionType oldState = m_ActiveTransmissionCache.GetOrAddDefault(output, eConnectionType.None);
				if (oldState == type)
					return;

				// Get what types were added by this
				addedTypes = EnumUtils.ExcludeFlags(type, oldState);

				// Update the cache to include old state and new state
				m_ActiveTransmissionCache[output] = EnumUtils.IncludeFlags(type, oldState);

			}
			finally
			{
				m_ActiveTransmissionCacheSection.Leave();
			}

			// Fire update event
			if (addedTypes != eConnectionType.None)
				RaiseActiveTransmissionStateChanged(output, addedTypes, true);
		}

		/// <summary>
		/// Set the given output connection types active transmission state to false
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		private void RemoveActiveTransmissionState(int output, eConnectionType type)
		{
			if (type == eConnectionType.None)
				return;

			eConnectionType removedTypes;

			m_ActiveTransmissionCacheSection.Enter();

			try
			{
				// Get old state
				// If it's not in the cache, bail out early
				eConnectionType oldState;
				if (!m_ActiveTransmissionCache.TryGetValue(output, out oldState))
					return;

				// Removed types are what we need to update via event
				removedTypes = EnumUtils.GetFlagsIntersection(type, oldState);

				// Get the new state
				// If it's none, remove the key from the dict, otherwise update the dict
				eConnectionType newState = EnumUtils.ExcludeFlags(oldState, type);
				if (newState == eConnectionType.None)
					m_ActiveTransmissionCache.Remove(output);
				else
					m_ActiveTransmissionCache[output] = newState;
			}
			finally
			{
				m_ActiveTransmissionCacheSection.Leave();
			}

			// Fire update event
			if (removedTypes != eConnectionType.None)
				RaiseActiveTransmissionStateChanged(output, removedTypes, false);
		}

		/// <summary>
		/// Sets all outputs and types to the given active transmission state
		/// </summary>
		/// <param name="state"></param>
		private void SetActiveTransmissionState(bool state)
		{
			if (state)
				SetActiveTransmissionState(
				                           GetOutputs()
					                           .Select(o => new KeyValuePair<int, eConnectionType>(o.Address, o.ConnectionType)));
			else
				SetActiveTransmissionState(Enumerable.Empty<KeyValuePair<int, eConnectionType>>());
		}

		/// <summary>
		/// Sets the given outputs/types as activly transmitting
		/// Clears all other outputs/types
		/// </summary>
		/// <param name="activeTransmissions"></param>
		private void SetActiveTransmissionState([NotNull] IEnumerable<KeyValuePair<int, eConnectionType>> activeTransmissions)
		{
			if (activeTransmissions == null)
				throw new ArgumentNullException("activeTransmissions");

			var newValues = activeTransmissions.ToArray();

			IcdHashSet<int> removedValues = null;
			m_ActiveTransmissionCacheSection.Execute(() => removedValues = m_ActiveTransmissionCache.Keys.ToIcdHashSet());
			removedValues.RemoveRange(newValues.Select(kvp => kvp.Key));

			// Set removed values to none
			foreach (int output in removedValues)
				SetActiveTransmissionState(output, eConnectionType.None);

			// Set new values
			foreach (var kvp in newValues)
				SetActiveTransmissionState(kvp.Key, kvp.Value);
		}

		/// <summary>
		/// Sets the given types as activly transmitting for an ouptut
		/// Removes any types not specified
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		private void SetActiveTransmissionState(int output, eConnectionType type)
		{
			eConnectionType addedTypes;
			eConnectionType removedTypes;

			m_ActiveTransmissionCacheSection.Enter();
			try
			{
				eConnectionType oldType;
				if (!m_ActiveTransmissionCache.TryGetValue(output, out oldType))
					oldType = eConnectionType.None;

				addedTypes = EnumUtils.ExcludeFlags(type, oldType);
				removedTypes = EnumUtils.ExcludeFlags(oldType, type);

				if (type == eConnectionType.None)
					m_ActiveTransmissionCache.Remove(output);
				else
					m_ActiveTransmissionCache[output] = type;
			}
			finally
			{
				m_ActiveTransmissionCacheSection.Leave();
			}

			if (addedTypes != eConnectionType.None)
				RaiseActiveTransmissionStateChanged(output, addedTypes, true);
			if (removedTypes != eConnectionType.None)
				RaiseActiveTransmissionStateChanged(output, removedTypes, false);
		}

		/// <summary>
		/// Sets the active transmission state from the switcher cache
		/// </summary>
		private void SetActiveTransmissionStateFromSwitcherCache()
		{
			List<KeyValuePair<int, eConnectionType>> transmissionState = new List<KeyValuePair<int, eConnectionType>>();

			foreach(ConnectorInfo output in GetOutputs())
			{
				ConnectorInfo output1 = output;
				eConnectionType activeTypes =
					EnumUtils.GetFlagsExceptNone(output.ConnectionType)
					         .Where(type => SwitcherCache.GetInputForOutput(output1.Address, type).HasValue)
					         .Aggregate(eConnectionType.None, (current, type) => current.IncludeFlags(type));
				
				if (activeTypes != eConnectionType.None)
					transmissionState.Add(new KeyValuePair<int, eConnectionType>(output.Address, activeTypes));
			}

			SetActiveTransmissionState(transmissionState);
		}

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
			// Don't update if using auto-routing
			if (UseAutoRouting)
				return;

			if (args.State)
				AddActiveTransmissionState(args.Output, args.Type);
			else
				RemoveActiveTransmissionState(args.Output, args.Type);
		}

		private void SwitcherCacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs args)
		{
			RaiseSourceDetectionStateChange(args.Input, args.Type, args.State);
		}

		#endregion
	}
}