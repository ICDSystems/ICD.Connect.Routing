using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.Extron.Devices.Switchers
{
	public interface IExtronSwitcherDevice : IDevice
	{
		event EventHandler<BoolEventArgs> OnInitializedChanged;

		/// <summary>
		/// Raised when the device sends a response.
		/// </summary>
		event EventHandler<StringEventArgs> OnResponseReceived;

		void SendCommand(string command, params object[] args);
	}
}
