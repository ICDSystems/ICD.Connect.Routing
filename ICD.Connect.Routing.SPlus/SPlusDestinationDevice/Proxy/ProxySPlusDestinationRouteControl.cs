﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API;
using ICD.Connect.API.Info;
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
		public ProxySPlusDestinationRouteControl(IProxySPlusDestinationDevice parent, int id, int inputCount) : base(parent, id)
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
		/// Override to build initialization commands on top of the current class info.
		/// </summary>
		/// <param name="command"></param>
		protected override void Initialize(ApiClassInfo command)
		{
			base.Initialize(command);

			ApiCommandBuilder.UpdateCommand(command)
			                 .SubscribeEvent(RouteDestinationControlApi.EVENT_ACTIVE_INPUTS_CHANGED)
			                 .SubscribeEvent(RouteDestinationControlApi.EVENT_SOURCE_DETECTION_STATE_CHANGE)
			                 .Complete();
		}

		#endregion
	}
}