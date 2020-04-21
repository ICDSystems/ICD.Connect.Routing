using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Info;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices.Proxies.Controls;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Proxies;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Proxy
{
	public sealed class ProxySPlusDestinationRouteControl : AbstractProxyDeviceControl, IRouteInputSelectControl
	{

		private readonly IcdHashSet<int> m_InputsDetectedHashSet;
		private readonly SafeCriticalSection m_InputsDetectedCriticalSection;

		private readonly Dictionary<eConnectionType, int?> m_ActiveInputs;
		private readonly SafeCriticalSection m_ActiveInputsSection;

		public int InputCount { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		/// <param name="inputCount"></param>
		public ProxySPlusDestinationRouteControl(IProxySPlusDestinationDevice parent, int id, int inputCount)
			: base(parent, id)
		{
			m_InputsDetectedCriticalSection = new SafeCriticalSection();
			m_InputsDetectedHashSet = new IcdHashSet<int>();
			m_ActiveInputs = new Dictionary<eConnectionType, int?>();
			m_ActiveInputsSection = new SafeCriticalSection();
			InputCount = inputCount;
		}

		public event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool GetSignalDetectedState(int input, eConnectionType type)
		{
			return m_InputsDetectedCriticalSection.Execute(() => m_InputsDetectedHashSet.Contains(input));
		}

		/// <summary>
		/// Returns the true if the input is actively being used by the source device.
		/// For example, a display might true if the input is currently on screen,
		/// while a switcher may return true if the input is currently routed.
		/// </summary>
		public bool GetInputActiveState(int input, eConnectionType type)
		{
			return EnumUtils.GetFlagsExceptNone(type)
							.All(f => GetActiveInput(f) == input);
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public ConnectorInfo GetInput(int input)
		{
			if (ContainsInput(input))
				return new ConnectorInfo(input, eConnectionType.Audio | eConnectionType.Video);
			throw new ArgumentOutOfRangeException("input", "input not found");
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public bool ContainsInput(int input)
		{
			return input <= InputCount && input >= 1;
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ConnectorInfo> GetInputs()
		{
			for (int i = 1; i <= InputCount; i++)
			{
				yield return GetInput(i);
			}
		}

		/// <summary>
		/// Sets the current active input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		public void SetActiveInput(int? input, eConnectionType type)
		{
			CallMethod(RouteInputSelectControlApi.METHOD_SET_ACTIVE_INPUT, input, type);
		}

		/// <summary>
		/// Gets the current active input.
		/// </summary>
		public int? GetActiveInput(eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentOutOfRangeException("flag");

			m_ActiveInputsSection.Enter();

			try
			{
				int? input;
				return m_ActiveInputs.TryGetValue(flag, out input) ? input : null;
			}
			finally
			{
				m_ActiveInputsSection.Leave();
			}
		}

		#region Private Methods

		private void HandleCachedActiveInputChange(InputStateChangeData data)
		{
			SetCachedActiveInput(data.State ? data.Input : (int?)null, data.Type);
		}

		/// <summary>
		/// Updates the internal active input cache.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		private void SetCachedActiveInput(int? input, eConnectionType type)
		{
			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
				SetCachedActiveInputSingle(input, flag);
		}

		/// <summary>
		/// Sets the active input for a single flag.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="flag"></param>
		private void SetCachedActiveInputSingle(int? input, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentOutOfRangeException("flag");

			int? old;

			m_ActiveInputsSection.Enter();

			try
			{
				m_ActiveInputs.TryGetValue(flag, out old);

				// No change
				if (input == old)
					return;

				m_ActiveInputs[flag] = input;
			}
			finally
			{
				m_ActiveInputsSection.Leave();
			}

			// Stopped using the old input
			if (old.HasValue)
				OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(old.Value, flag, false));

			// Started using the new input
			if (input.HasValue)
				OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(input.Value, flag, true));
		}

		private void HandleInputDetectedChange(InputStateChangeData data)
		{
			SetInputDetectedFeedback(data.Input, data.State);
		}

		private void SetInputDetectedFeedback(int input, bool state)
		{
			if (!ContainsInput(input))
				throw new ArgumentOutOfRangeException("input");

			m_InputsDetectedCriticalSection.Enter();
			try
			{
				if (m_InputsDetectedHashSet.Contains(input) == state)
					return;

				if (state)
					m_InputsDetectedHashSet.Add(input);
				else
					m_InputsDetectedHashSet.Remove(input);
			}
			finally
			{
				m_InputsDetectedCriticalSection.Leave();
			}

			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(input, eConnectionType.Audio | eConnectionType.Video, state));
		}

		private void HandleGetRouteState(RouteState.RouteState routeState)
		{
			if (routeState == null)
				throw new ArgumentNullException("routeState");

			HandleGetRouteStateInputDetect(routeState);
			HandleGetRouteStateActiveInput(routeState);
		}

		private void HandleGetRouteStateActiveInput(RouteState.RouteState routeState)
		{
			if (routeState == null)
				throw new ArgumentNullException("routeState");

			Dictionary<eConnectionType, int?> inputsActive = routeState.InputsActive ?? new Dictionary<eConnectionType, int?>();

			// todo: Don't double-event types that have the same input active

			List<ActiveInputStateChangeEventArgs> changes = new List<ActiveInputStateChangeEventArgs>();

			m_ActiveInputsSection.Enter();
			try
			{
				List<eConnectionType> currentKeys = m_ActiveInputs.Keys.ToList();
				List<eConnectionType> newKeys = inputsActive.Keys.ToList();

				// Types that are no longer active
				foreach (eConnectionType oldType in currentKeys.Except(newKeys))
				{
					if (!m_ActiveInputs[oldType].HasValue)
						continue;

					int? oldInput = m_ActiveInputs[oldType].Value;
					if (oldInput == null)
						continue;
					changes.Add(new ActiveInputStateChangeEventArgs(oldInput.Value,oldType,false));
					m_ActiveInputs[oldType] = null;
				}

				//Possible new active types
				foreach (eConnectionType newType in newKeys)
				{
					int? oldValue;
					if (m_ActiveInputs.TryGetValue(newType, out oldValue))
					{
						if (!oldValue.HasValue || oldValue == inputsActive[newType])
							continue;
						changes.Add(new ActiveInputStateChangeEventArgs(oldValue.Value, newType, false));
					}

					var newInput = inputsActive[newType];

					m_ActiveInputs[newType] = newInput;
					if (newInput.HasValue)
						changes.Add(new ActiveInputStateChangeEventArgs(newInput.Value, newType, true));
				}
			}
			finally
			{
				m_ActiveInputsSection.Leave();
			}

			foreach (ActiveInputStateChangeEventArgs change in changes)
			{
				OnActiveInputsChanged.Raise(this, change);
			}
		}

		private void HandleGetRouteStateInputDetect(RouteState.RouteState routeState)
		{
			if (routeState == null)
				throw new ArgumentNullException("routeState");

			IEnumerable<int> noLongerDetected;
			IEnumerable<int> newDetected;

			int[] inputsDetected = routeState.InputsDetected ?? new int[] {};

			m_InputsDetectedCriticalSection.Enter();
			try
			{
				noLongerDetected = m_InputsDetectedHashSet.Except(inputsDetected);
				newDetected = inputsDetected.Except(m_InputsDetectedHashSet);

				m_InputsDetectedHashSet.Clear();
				m_InputsDetectedHashSet.AddRange(inputsDetected);
			}
			finally
			{
				m_InputsDetectedCriticalSection.Leave();
			}

			foreach (var input in noLongerDetected)
				OnSourceDetectionStateChange.Raise(this,
				                                   new SourceDetectionStateChangeEventArgs(input,
				                                                                           eConnectionType.Audio |
				                                                                           eConnectionType.Video, false));
			foreach (var input in newDetected)
				OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(input, eConnectionType.Audio| eConnectionType.Video, true));
		}

		#endregion

		#region API

		/// <summary>
		/// Updates the proxy with event feedback info.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="result"></param>
		protected override void ParseEvent(string name, ApiResult result)
		{
			base.ParseEvent(name, result);

			switch (name)
			{
				case RouteDestinationControlApi.EVENT_ACTIVE_INPUTS_CHANGED:
					HandleCachedActiveInputChange(result.GetValue<InputStateChangeData>());
					break;
				case RouteDestinationControlApi.EVENT_SOURCE_DETECTION_STATE_CHANGE:
					HandleInputDetectedChange(result.GetValue<InputStateChangeData>());
					break;
			}
		}

		/// <summary>
		/// Updates the proxy with a method result.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="result"></param>
		protected override void ParseMethod(string name, ApiResult result)
		{
			base.ParseMethod(name, result);

			switch (name)
			{
				case SPlusDestinationRouteControlApi.METHOD_GET_CURRENT_ROUTE_STATE:
					HandleGetRouteState(result.GetValue<RouteState.RouteState>());
					break;
			}
		}

		/// <summary>
		/// Override to build initialization commands on top of the current class info.
		/// </summary>
		/// <param name="command"></param>
		protected override void Initialize(ApiClassInfo command)
		{
			base.Initialize(command);

			ApiCommandBuilder.UpdateCommand(command)
			                 .SubscribeEvent(RouteDestinationControlApi.EVENT_ACTIVE_INPUTS_CHANGED)
			                 .SubscribeEvent(RouteDestinationControlApi.EVENT_SOURCE_DETECTION_STATE_CHANGE)
							 .CallMethod(SPlusDestinationRouteControlApi.METHOD_GET_CURRENT_ROUTE_STATE)
			                 .Complete();
		}

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			RouteDestinationControlConsole.BuildConsoleStatus(this, addRow);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			foreach (IConsoleCommand command in RouteDestinationControlConsole.GetConsoleCommands(this))
				yield return command;
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (IConsoleNodeBase node in GetBaseConsoleNodes())
				yield return node;

			foreach (IConsoleNodeBase node in RouteDestinationControlConsole.GetConsoleNodes(this))
				yield return node;
		}

		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}

		#endregion
	}
}