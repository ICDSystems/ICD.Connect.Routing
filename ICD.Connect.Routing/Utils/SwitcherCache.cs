﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Utils
{
	/// <summary>
	/// Contains the common switcher state caching mechanisms for tracking input detection, switching changes, etc.
	/// </summary>
	public sealed class SwitcherCache
	{
		public event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;
		public event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;
		public event EventHandler<RouteChangeEventArgs> OnRouteChange;

		// Keeps track of source detection
		private readonly Dictionary<int, Dictionary<eConnectionType, bool>> m_SourceDetectionStates;
		private readonly SafeCriticalSection m_SourceDetectionStatesSection;

		// Keeps track of active transmission
		private readonly Dictionary<int, Dictionary<eConnectionType, bool>> m_ActiveTransmissionStates;
		private readonly SafeCriticalSection m_ActiveTransmissionStatesSection;

		// Keeps track of output->input mapping
		private readonly Dictionary<int, Dictionary<eConnectionType, int?>> m_OutputInputMap;
		private readonly SafeCriticalSection m_OutputInputMapSection;

		// Keeps track of input->output mapping
		private readonly Dictionary<int, Dictionary<eConnectionType, IcdHashSet<int>>> m_InputOutputMap;
		private readonly SafeCriticalSection m_InputOutputMapSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		public SwitcherCache()
		{
			m_SourceDetectionStates = new Dictionary<int, Dictionary<eConnectionType, bool>>();
			m_SourceDetectionStatesSection = new SafeCriticalSection();

			m_ActiveTransmissionStates = new Dictionary<int, Dictionary<eConnectionType, bool>>();
			m_ActiveTransmissionStatesSection = new SafeCriticalSection();

			m_OutputInputMap = new Dictionary<int, Dictionary<eConnectionType, int?>>();
			m_OutputInputMapSection = new SafeCriticalSection();

			m_InputOutputMap = new Dictionary<int, Dictionary<eConnectionType, IcdHashSet<int>>>();
			m_InputOutputMapSection = new SafeCriticalSection();
		}

		#region Methods

		/// <summary>
		/// Clears all of the cached values.
		/// </summary>
		[PublicAPI]
		public void Clear()
		{
			ClearSourceDetectedStates();
			ClearOutputInputMap();
		}

		/// <summary>
		/// Gets the cached source detection state for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[PublicAPI]
		public bool GetSourceDetectedState(int input, eConnectionType type)
		{
			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Type has multiple flags", "type");

			m_SourceDetectionStatesSection.Enter();

			try
			{
				return m_SourceDetectionStates.ContainsKey(input)
					&& m_SourceDetectionStates[input].GetDefault(type, false);
			}
			finally
			{
				m_SourceDetectionStatesSection.Leave();
			}
		}

		/// <summary>
		/// Sets the source detection state for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <param name="state"></param>
		[PublicAPI]
		public void SetSourceDetectedState(int input, eConnectionType type, bool state)
		{
			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
				SetSourceDetectedStateSingle(input, flag, state);
		}

		[PublicAPI]
		public void SetInputForOutput(int output, int? input, eConnectionType type)
		{
			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
				SetInputForOutputSingle(output, input, flag);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Clears the mapped output inputs.
		/// </summary>
		private void ClearOutputInputMap()
		{
			int[] outputs = m_OutputInputMapSection.Execute(() => m_OutputInputMap.Keys.ToArray());
			eConnectionType allTypes = EnumUtils.GetFlagsAllValue<eConnectionType>();

			foreach (int output in outputs)
				SetInputForOutput(output, null, allTypes);
		}

		/// <summary>
		/// Clears the cached active transmission states.
		/// </summary>
		private void ClearSourceDetectedStates()
		{
			int[] inputs = m_SourceDetectionStatesSection.Execute(() => m_SourceDetectionStates.Keys.ToArray());
			eConnectionType allTypes = EnumUtils.GetFlagsAllValue<eConnectionType>();

			foreach (int input in inputs)
				SetSourceDetectedState(input, allTypes, false);
		}

		/// <summary>
		/// Sets the source detection state for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <param name="state"></param>
		private void SetSourceDetectedStateSingle(int input, eConnectionType type, bool state)
		{
			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Type must have single flag", "type");

			m_SourceDetectionStatesSection.Enter();

			try
			{
				if (!m_SourceDetectionStates.ContainsKey(input))
				{
					// No change
					if (!state)
						return;

					m_SourceDetectionStates[input] = new Dictionary<eConnectionType, bool>();
				}

				// No change
				if (m_SourceDetectionStates[input].GetDefault(type, false) == state)
					return;

				m_SourceDetectionStates[input][type] = state;
			}
			finally
			{
				m_SourceDetectionStatesSection.Leave();
			}

			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(input, type, state));
		}

		/// <summary>
		/// Sets the input for the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="input"></param>
		/// <param name="type"></param>
		private void SetInputForOutputSingle(int output, int? input, eConnectionType type)
		{
			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Type must have single flag", "type");

			int? oldInput;

			m_OutputInputMapSection.Enter();

			try
			{
				if (!m_OutputInputMap.ContainsKey(output))
				{
					// No change
					if (!input.HasValue)
						return;

					m_OutputInputMap[output] = new Dictionary<eConnectionType, int?>();
				}

				oldInput = m_OutputInputMap[output].GetDefault(type, null);

				// No change
				if (oldInput == input)
					return;

				m_OutputInputMap[output][type] = input;
			}
			finally
			{
				m_OutputInputMapSection.Leave();
			}

			SetActiveTransmissionStateSingle(output, type, input.HasValue);
			UpdateInputOutputMapSingle(oldInput, input, output, type);

			OnRouteChange.Raise(this, new RouteChangeEventArgs(output, type));
		}

		/// <summary>
		/// Updates the Input->Output map.
		/// </summary>
		/// <param name="oldInput"></param>
		/// <param name="newInput"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		private void UpdateInputOutputMapSingle(int? oldInput, int? newInput, int output, eConnectionType type)
		{
			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Type must have single flag", "type");

			// No change
			if (oldInput == newInput)
				return;

			m_InputOutputMapSection.Enter();

			try
			{
				bool change = false;

				// Remove the output from the old input
				if (oldInput.HasValue)
				{
					if (!m_InputOutputMap.ContainsKey(oldInput.Value))
						m_InputOutputMap[oldInput.Value] = new Dictionary<eConnectionType, IcdHashSet<int>>();

					if (!m_InputOutputMap[oldInput.Value].ContainsKey(type))
						m_InputOutputMap[oldInput.Value][type] = new IcdHashSet<int>();

					change |= m_InputOutputMap[oldInput.Value][type].Remove(output);
				}

				// Add the output to the new input
				if (newInput.HasValue)
				{
					if (!m_InputOutputMap.ContainsKey(newInput.Value))
						m_InputOutputMap[newInput.Value] = new Dictionary<eConnectionType, IcdHashSet<int>>();

					if (!m_InputOutputMap[newInput.Value].ContainsKey(type))
						m_InputOutputMap[newInput.Value][type] = new IcdHashSet<int>();

					change |= m_InputOutputMap[newInput.Value][type].Add(output);
				}

				// No change.
				if (!change)
					return;
			}
			finally
			{
				m_InputOutputMapSection.Leave();
			}

			if (oldInput.HasValue)
				OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(oldInput.Value, type, false));

			if (newInput.HasValue)
				OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(newInput.Value, type, true));
		}

		/// <summary>
		/// Sets the active transmission state for the given output and type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <param name="state"></param>
		private void SetActiveTransmissionStateSingle(int output, eConnectionType type, bool state)
		{
			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Type must have single flag", "type");

			m_ActiveTransmissionStatesSection.Enter();

			try
			{
				if (!m_ActiveTransmissionStates.ContainsKey(output))
				{
					// No change
					if (!state)
						return;

					m_ActiveTransmissionStates[output] = new Dictionary<eConnectionType, bool>();
				}

				// No change
				if (m_ActiveTransmissionStates[output].GetDefault(type, false) == state)
					return;

				m_ActiveTransmissionStates[output][type] = state;
			}
			finally
			{
				m_ActiveTransmissionStatesSection.Leave();
			}

			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(output, type, state));
		}

		#endregion
	}
}