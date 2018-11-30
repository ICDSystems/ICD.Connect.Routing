using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;

namespace ICD.Connect.Routing.Extron.Devices.Endpoints
{
	public interface IDtpHdmiDevice : IDevice
	{
		event EventHandler<BoolEventArgs> OnPortInitialized;

		/// <summary>
		/// Raised when the comspec changes for the port.
		/// </summary>
		event EventHandler<GenericEventArgs<ComSpec>> OnPortComSpecChanged; 

		/// <summary>
		/// Gets the address where this endpoint is connected to the switcher.
		/// </summary>
		int SwitcherAddress { get; }

		/// <summary>
		/// Returns Input for TX, Output for RX.
		/// </summary>
		eDtpInputOuput SwitcherInputOutput { get; }

		ISerialPort GetSerialInsertionPort();

		void InitializeComPort(eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType, eComStopBits stopBits);
	}
}