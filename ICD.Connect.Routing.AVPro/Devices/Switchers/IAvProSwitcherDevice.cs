using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.AVPro.Devices.Switchers
{
	public interface IAvProSwitcherDevice : IDevice
	{
		/// <summary>
		/// Raised when the device becomes initialized/deinitialized.
		/// </summary>
		event EventHandler<BoolEventArgs> OnInitializedChanged;

		/// <summary>
		/// Raised when the device sends a response.
		/// </summary>
		event EventHandler<StringEventArgs> OnResponseReceived;

		/// <summary>
		/// Gets the number of AV inputs.
		/// </summary>
		int NumberOfInputs { get; }

		/// <summary>
		/// Gets the number of AV outputs.
		/// </summary>
		int NumberOfOutputs { get; }

		/// <summary>
		/// Sends the command to the device.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="args"></param>
		void SendCommand(string command, params object[] args);
	}
}
