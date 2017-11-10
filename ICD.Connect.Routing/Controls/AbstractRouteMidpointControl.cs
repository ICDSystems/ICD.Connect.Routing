using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Controls
{
	public abstract class AbstractRouteMidpointControl<T> : AbstractRouteDestinationControl<T>, IRouteMidpointControl
		where T : IDevice
	{
		public abstract event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractRouteMidpointControl(T parent, int id)
			: base(parent, id)
		{
		}

		#region Methods

		/// <summary>
		/// Returns true if the device is actively transmitting on the given output.
		/// This is NOT the same as sending video, since some devices may send an
		/// idle signal by default.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public virtual bool GetActiveTransmissionState(int output, eConnectionType type)
		{
			// Returns true if the output is transmitting an input on all flags
			return EnumUtils.GetFlagsExceptNone(type).All(flag => this.GetInputs(output, flag).Any());
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual ConnectorInfo GetOutput(int address)
		{
			return GetOutputs().First(c => c.Address == address);
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<ConnectorInfo> GetOutputs();

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public abstract IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type);

		/// <summary>
		/// Gets the input routed to the given output matching the given type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Type has multiple flags.</exception>
		public abstract ConnectorInfo? GetInput(int output, eConnectionType type);

		/// <summary>
		/// Returns the true if the input is actively being used by the source device.
		/// For example, a display might true if the input is currently on screen,
		/// while a switcher may return true if the input is currently routed.
		/// </summary>
		public override bool GetInputActiveState(int input, eConnectionType type)
		{
			return GetOutputs(input, type).Any();
		}

		#endregion

		#region Console

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return
				new ConsoleCommand("PrintRouting", "Prints a table of the current routing state", () => PrintRouteStatusTable());
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		private string PrintRouteStatusTable()
		{
			TableBuilder builder = new TableBuilder("Input", "Type", "Detected", "Outputs");

			foreach (ConnectorInfo input in GetInputs().OrderBy(c => c.Address))
			{
				bool first = true;

				foreach (eConnectionType type in EnumUtils.GetFlagsExceptNone(input.ConnectionType))
				{
					string inputString = first ? input.Address.ToString() : string.Empty;
					first = false;

					string typeString = type.ToString();
					string detectedString = GetSignalDetectedState(input.Address, type) ? "True" : string.Empty;
					string outputsString = StringUtils.ArrayFormat(GetOutputs(input.Address, type));

					builder.AddRow(inputString, typeString, detectedString, outputsString);
				}
			}

			return builder.ToString();
		}

		#endregion
	}
}
