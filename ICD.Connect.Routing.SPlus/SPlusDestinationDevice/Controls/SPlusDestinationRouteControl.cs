using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Controls
{
	public sealed class SPlusDestinationRouteControl : AbstractRouteInputSelectControl<Device.SPlusDestinationDevice>
	{

		private readonly IcdHashSet<int> m_InputsDetectedHashSet;
		private readonly SafeCriticalSection m_InputsDetectedCriticalSection;

		public int InputCount { get; private set; }


		#region Constructor
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		/// <param name="inputCount"></param>
		public SPlusDestinationRouteControl(Device.SPlusDestinationDevice parent, int id, int inputCount) : base(parent, id)
		{
			m_InputsDetectedCriticalSection = new SafeCriticalSection();
			m_InputsDetectedHashSet = new IcdHashSet<int>();
			InputCount = inputCount;
		}

		#endregion

		#region Events

		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;

		#endregion

		#region Methods

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			return m_InputsDetectedCriticalSection.Execute(() => m_InputsDetectedHashSet.Contains(input));
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int input)
		{
			if (ContainsInput(input))
				return new ConnectorInfo(input, eConnectionType.Audio|eConnectionType.Video);
			throw new ArgumentOutOfRangeException("input", "input not found");
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return input <= InputCount && input >= 1;
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			for (int i = 1; i <= InputCount; i++)
			{
				yield return GetInput(i);
			}
		}

		/// <summary>
		/// Sets the current active input.
		/// </summary>
		public override void SetActiveInput(int? input, eConnectionType type)
		{
			Parent.SetActiveInput(input);
		}

		#endregion

		#region Internal/Private Methods

		/// <summary>
		/// Called from the shim (through the device) to set the current active input feedback
		/// </summary>
		/// <param name="input"></param>
		internal void SetActiveInputFeedback(int? input)
		{
			SetCachedActiveInput(input, eConnectionType.Audio | eConnectionType.Video);
		}

		/// <summary>
		/// Called from the shim (through the device) to set detected state for inputs.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="state"></param>
		internal void SetInputDetectedFeedback(int input, bool state)
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

		/// <summary>
		/// Called from the shim (through the device) to set all new set of detected inputs.
		/// Sets any existing detected inputs to not detected.
		/// </summary>
		/// <param name="detectedInputs"></param>
		internal void ResetInputDetectedFeedback(List<int> detectedInputs)
		{
			if (detectedInputs == null)
				throw new ArgumentNullException("detectedInputs");

			if (detectedInputs.AnyAndAll(i => !ContainsInput(i)))
				throw new ArgumentOutOfRangeException("detectedInputs");

			IEnumerable<int> newDetected;
			IEnumerable<int> noLongerDetected;

			m_InputsDetectedCriticalSection.Enter();
			try
			{
				newDetected = detectedInputs.Except(m_InputsDetectedHashSet);
				noLongerDetected = m_InputsDetectedHashSet.Except(detectedInputs);
				m_InputsDetectedHashSet.Clear();
				m_InputsDetectedHashSet.AddRange(detectedInputs);
			}
			finally
			{
				m_InputsDetectedCriticalSection.Leave();
			}

			foreach (int input in noLongerDetected)
			{
				OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(input, eConnectionType.Audio | eConnectionType.Video, false));
			}

			foreach (int input in newDetected)
			{
				OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(input, eConnectionType.Audio | eConnectionType.Video, true));
			}
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

			addRow("Input Count", InputCount);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new GenericConsoleCommand<int>("SetInputCount","Sets the number of inputs for the control. NOT SAFE!", count => InputCount = count);}

		/// <summary>
		/// Gets the base's console commands.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}