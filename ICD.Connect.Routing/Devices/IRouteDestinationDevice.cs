using System;
using System.Collections.Generic;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Devices
{
	public interface IRouteDestinationDevice : IRouteDevice
	{
		/// <summary>
		/// Raised when an input source status changes.
		/// </summary>
		event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;

		/// <summary>
		/// Raised when the device starts/stops actively using an input, e.g. unroutes an input.
		/// </summary>
		event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		bool GetSignalDetectedState(int input, eConnectionType type);

		/// <summary>
		/// Returns the true if the input is actively being used by the source device.
		/// For example, a display might true if the input is currently on screen,
		/// while a switcher may return true if the input is currently routed.
		/// </summary>
		bool GetInputActiveState(int input, eConnectionType type);

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		ConnectorInfo GetInput(int input);

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		bool ContainsInput(int input);

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		IEnumerable<ConnectorInfo> GetInputs();
	}
}