using System;
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
		private readonly Dictionary<int, eConnectionType> m_SourceDetectionStates;
		private readonly SafeCriticalSection m_SourceDetectionStatesSection;

		// Keeps track of active transmission
		private readonly Dictionary<int, eConnectionType> m_ActiveTransmissionStates;
		private readonly SafeCriticalSection m_ActiveTransmissionStatesSection;

		// Keeps track of output->input mapping
		private readonly Dictionary<int, Dictionary<eConnectionType, int>> m_OutputInputMap;
		private readonly SafeCriticalSection m_OutputInputMapSection;

		// Keeps track of input->output mapping
		private readonly Dictionary<int, Dictionary<eConnectionType, IcdHashSet<int>>> m_InputOutputMap;
		private readonly SafeCriticalSection m_InputOutputMapSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		public SwitcherCache()
		{
			m_SourceDetectionStates = new Dictionary<int, eConnectionType>();
			m_SourceDetectionStatesSection = new SafeCriticalSection();

			m_ActiveTransmissionStates = new Dictionary<int, eConnectionType>();
			m_ActiveTransmissionStatesSection = new SafeCriticalSection();

			m_OutputInputMap = new Dictionary<int, Dictionary<eConnectionType, int>>();
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
		/// <param name="flag"></param>
		/// <returns></returns>
		[PublicAPI]
		public bool GetSourceDetectedState(int input, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("Type has multiple flags", "flag");

			m_SourceDetectionStatesSection.Enter();

			try
			{
				return m_SourceDetectionStates.GetDefault(input).HasFlag(flag);
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
		public bool SetSourceDetectedState(int input, eConnectionType type, bool state)
		{
			eConnectionType changed;

			m_SourceDetectionStatesSection.Enter();

			try
			{
				eConnectionType current = m_SourceDetectionStates.GetDefault(input);
				eConnectionType result;

				if (state)
					result = current | type;
				else
					result = current & ~type;

				if (result == current)
					return false;

				changed = current ^ result;

				m_SourceDetectionStates[input] = result;
			}
			finally
			{
				m_SourceDetectionStatesSection.Leave();
			}

			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(input, changed, state));
			return true;
		}

		[PublicAPI]
		public bool SetInputForOutput(int output, int? input, eConnectionType type)
		{
			bool change = false;
			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
				change |= SetInputForOutputSingle(output, input, flag);

			return change;
		}

		/// <summary>
		/// Gets the cached input for the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[PublicAPI]
		public int? GetInputForOutput(int output, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
				throw new ArgumentException("Type must have a single flag", "type");

			m_OutputInputMapSection.Enter();

			try
			{
				Dictionary<eConnectionType, int> dict;
				if (!m_OutputInputMap.TryGetValue(output, out dict))
					return null;

				int address;
				return dict.TryGetValue(type, out address) ? address : (int?)null;
			}
			finally
			{
				m_OutputInputMapSection.Leave();
			}
		}

		/// <summary>
		/// Gets the cached input for the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[PublicAPI]
		public ConnectorInfo? GetInputConnectorInfoForOutput(int output, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
				throw new ArgumentException("Type must have a single flag", "type");

			int? input = GetInputForOutput(output, type);
			if (input == null)
				return null;

			return new ConnectorInfo((int)input, type);
		}

		/// <summary>
		/// Gets the cached outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<ConnectorInfo> GetOutputsForInput(int input, eConnectionType type)
		{
			m_InputOutputMapSection.Enter();

			try
			{
				Dictionary<eConnectionType, IcdHashSet<int>> dict;
				if (!m_InputOutputMap.TryGetValue(input, out dict))
					return Enumerable.Empty<ConnectorInfo>();

				return EnumUtils.GetFlagsExceptNone(type)
				                .SelectMany(f =>
				                            {
					                            IcdHashSet<int> collection;
					                            return dict.TryGetValue(f, out collection)
						                                   ? collection.Select(i => new ConnectorInfo(i, f))
						                                   : Enumerable.Empty<ConnectorInfo>();
				                            })
				                .ToArray();
			}
			finally
			{
				m_InputOutputMapSection.Leave();
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Clears the mapped output inputs.
		/// </summary>
		private void ClearOutputInputMap()
		{
			int[] outputs = m_OutputInputMapSection.Execute(() => m_OutputInputMap.Keys.ToArray(m_OutputInputMap.Count));
			eConnectionType allTypes = EnumUtils.GetFlagsAllValue<eConnectionType>();

			foreach (int output in outputs)
				SetInputForOutput(output, null, allTypes);
		}

		/// <summary>
		/// Clears the cached active transmission states.
		/// </summary>
		private void ClearSourceDetectedStates()
		{
			int[] inputs =
				m_SourceDetectionStatesSection.Execute(() => m_SourceDetectionStates.Keys.ToArray(m_SourceDetectionStates.Count));
			eConnectionType allTypes = EnumUtils.GetFlagsAllValue<eConnectionType>();

			foreach (int input in inputs)
				SetSourceDetectedState(input, allTypes, false);
		}

		/// <summary>
		/// Sets the input for the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="input"></param>
		/// <param name="flag"></param>
		private bool SetInputForOutputSingle(int output, int? input, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("Type must have single flag", "flag");

			int? oldInput;

			m_OutputInputMapSection.Enter();

			try
			{
				Dictionary<eConnectionType, int> cache;
				if (!m_OutputInputMap.TryGetValue(output, out cache))
				{
					cache = new Dictionary<eConnectionType, int>();
					m_OutputInputMap.Add(output, cache);
				}

				int cachedInput;
				oldInput = cache.TryGetValue(flag, out cachedInput) ? cachedInput : (int?)null;

				// No change
				if (oldInput == input)
					return false;

				if (input == null)
					cache.Remove(flag);
				else
					cache[flag] = (int)input;
			}
			finally
			{
				m_OutputInputMapSection.Leave();
			}

			UpdateInputOutputMap(oldInput, input, output, flag);
			SetActiveTransmissionState(output, flag, input.HasValue);

			OnRouteChange.Raise(this, new RouteChangeEventArgs(oldInput, input, output, flag));
			return true;
		}

		/// <summary>
		/// Sets the active transmission state for the given output and type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <param name="state"></param>
		private void SetActiveTransmissionState(int output, eConnectionType type, bool state)
		{
			eConnectionType changed;

			m_ActiveTransmissionStatesSection.Enter();

			try
			{
				eConnectionType current = m_ActiveTransmissionStates.GetDefault(output);
				eConnectionType result;

				if (state)
					result = current | type;
				else
					result = current & ~type;

				if (result == current)
					return;

				changed = current ^ result;

				m_SourceDetectionStates[output] = result;
			}
			finally
			{
				m_ActiveTransmissionStatesSection.Leave();
			}

			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(output, changed, state));
		}

		/// <summary>
		/// Updates the Input->Output map.
		/// </summary>
		/// <param name="oldInput"></param>
		/// <param name="newInput"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		private void UpdateInputOutputMap(int? oldInput, int? newInput, int output, eConnectionType type)
		{
			// No change
			if (oldInput == newInput)
				return;

			eConnectionType oldChange = eConnectionType.None;
			eConnectionType newChange = eConnectionType.None;

			m_InputOutputMapSection.Enter();

			try
			{
				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
				{
					// Remove the output from the old input
					if (oldInput.HasValue && InputOutputMapRemove(oldInput.Value, output, flag))
						oldChange |= flag;

					// Add the output to the new input
					if (newInput.HasValue && InputOutputMapAdd(newInput.Value, output, flag))
						newChange |= flag;
				}
			}
			finally
			{
				m_InputOutputMapSection.Leave();
			}

			if (oldInput.HasValue && oldChange != eConnectionType.None)
				OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(oldInput.Value, oldChange, false));

			if (newInput.HasValue && newChange != eConnectionType.None)
				OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(newInput.Value, newChange, true));
		}

		/// <summary>
		/// Removes the Input->Output mapping for the given flag.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="output"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		private bool InputOutputMapRemove(int input, int output, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("Type must have single flag", "flag");

			m_InputOutputMapSection.Enter();

			try
			{
				Dictionary<eConnectionType, IcdHashSet<int>> cache;
				if (!m_InputOutputMap.TryGetValue(input, out cache))
					return false;

				IcdHashSet<int> innerCache;
				return cache.TryGetValue(flag, out innerCache) && innerCache.Remove(output);
			}
			finally
			{
				m_InputOutputMapSection.Leave();
			}
		}

		/// <summary>
		/// Adds the Input->Output mapping for the given flag.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="output"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		private bool InputOutputMapAdd(int input, int output, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("Type must have single flag", "flag");

			m_InputOutputMapSection.Enter();

			try
			{
				Dictionary<eConnectionType, IcdHashSet<int>> cache;
				if (!m_InputOutputMap.TryGetValue(input, out cache))
				{
					cache = new Dictionary<eConnectionType, IcdHashSet<int>>();
					m_InputOutputMap.Add(input, cache);
				}

				IcdHashSet<int> innerCache;
				if (!cache.TryGetValue(flag, out innerCache))
				{
					innerCache = new IcdHashSet<int>();
					cache.Add(flag, innerCache);
				}

				return innerCache.Add(output);
			}
			finally
			{
				m_InputOutputMapSection.Leave();
			}
		}

		#endregion
	}
}
