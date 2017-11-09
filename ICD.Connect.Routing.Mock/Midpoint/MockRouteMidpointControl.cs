using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Services;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Mock.Midpoint
{
	public sealed class MockRouteMidpointControl : AbstractRouteMidpointControl<IDevice>
	{
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		private readonly Dictionary<int, ConnectorInfo> m_Inputs;
		private readonly Dictionary<int, Dictionary<eConnectionType, bool>> m_SignalDetected;
		private readonly Dictionary<int, ConnectorInfo> m_Outputs;
		private readonly Dictionary<int, Dictionary<eConnectionType, int>> m_OutputToInputMap;

		/// <summary>
		/// Returns the number of outputs on the device.
		/// </summary>
		[PublicAPI]
		public int OutputCount { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public MockRouteMidpointControl(IDevice parent, int id)
			: base(parent, id)
		{
			m_Inputs = new Dictionary<int, ConnectorInfo>();
			m_SignalDetected = new Dictionary<int, Dictionary<eConnectionType, bool>>();
			m_Outputs = new Dictionary<int, ConnectorInfo>();
			m_OutputToInputMap = new Dictionary<int, Dictionary<eConnectionType, int>>();
		}

		#region Methods

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnActiveInputsChanged = null;
			OnSourceDetectionStateChange = null;
			OnActiveInputsChanged = null;

			base.DisposeFinal(disposing);
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			return
				ServiceProvider.GetService<IRoutingGraph>()
				               .Connections.GetConnections()
				               .Where(c => c.Source.Device == Parent.Id && c.Source.Control == Id)
				               .Select(c => new ConnectorInfo(c.Source.Address, c.ConnectionType));
		}

		/// <summary>
		/// Sets the outputs.
		/// </summary>
		/// <param name="outputs"></param>
		public void SetOutputs(IEnumerable<ConnectorInfo> outputs)
		{
			m_Outputs.Clear();
			m_Outputs.AddRange(outputs, info => info.Address);
		}

		/// <summary>
		/// Gets the inputs routed to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs(int output, eConnectionType type)
		{
			if (!m_OutputToInputMap.ContainsKey(output))
				return Enumerable.Empty<ConnectorInfo>();

			return EnumUtils.GetFlagsExceptNone(type)
			                .Where(f => m_OutputToInputMap[output].ContainsKey(f))
			                .Select(f => GetInput(m_OutputToInputMap[output][f]))
			                .Distinct();
		}

		/// <summary>
		/// Sets the inputs.
		/// </summary>
		/// <param name="inputs"></param>
		public void SetInputs(IEnumerable<ConnectorInfo> inputs)
		{
			m_Inputs.Clear();
			m_Inputs.AddRange(inputs, info => info.Address);
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			return
				ServiceProvider.GetService<IRoutingGraph>()
				               .Connections.GetConnections()
				               .Where(c => c.Destination.Device == Parent.Id && c.Destination.Control == Id)
				               .Select(c => new ConnectorInfo(c.Destination.Address, c.ConnectionType));
		}

		/// <summary>
		/// Returns true if video is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			return m_SignalDetected.ContainsKey(input) && m_SignalDetected[input].GetDefault(type, false);
		}

		/// <summary>
		/// Sets the video detected state at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <param name="state"></param>
		[PublicAPI]
		public void SetSignalDetectedState(int input, eConnectionType type, bool state)
		{
			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
			{
				if (state == GetSignalDetectedState(input, flag))
					continue;

				if (!m_SignalDetected.ContainsKey(input))
					m_SignalDetected[input] = new Dictionary<eConnectionType, bool>();

				m_SignalDetected[input][flag] = state;

				RaiseOnSourceChange(input, flag);
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Simulates a source change.
		/// </summary>
		private void RaiseOnSourceChange(int input, eConnectionType type)
		{
			bool detected = GetSignalDetectedState(input, type);
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(input, type, detected));
		}

		#endregion

		#region Console

		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new GenericConsoleCommand<int, eConnectionType>(
				"AddInput",
				"Adds an input to the device",
				(a, b) => AddInput(a, b));
			yield return new GenericConsoleCommand<int, eConnectionType>(
				"AddOutput",
				"Adds an output to the device",
				(a, b) => AddOutput(a, b));
			yield return new GenericConsoleCommand<int, eConnectionType, bool>(
				"SetSignalDetectedState",
				"<input> <connectionType> <true/false>",
				(a, b, c) => SetSignalDetectedState(a, b, c));
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		private void AddOutput(int address, eConnectionType type)
		{
			SetOutputs(GetOutputs().Append(new ConnectorInfo(address, type)));
		}

		private void AddInput(int address, eConnectionType type)
		{
			SetInputs(GetInputs().Append(new ConnectorInfo(address, type)));
		}

		#endregion
	}
}
