using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;

namespace ICD.Connect.Routing.Extron.Devices.Switchers
{
	public interface IDtpCrosspointDevice : IDevice
	{
		event EventHandler<BoolEventArgs> OnInitializedChanged;
		event EventHandler<IntEventArgs> OnInputPortInitialized;
		event EventHandler<IntEventArgs> OnOutputPortInitialized;

		/// <summary>
		/// Raised when the device sends a response.
		/// </summary>
		event EventHandler<StringEventArgs> OnResponseReceived;
		
		ISerialPort GetInputSerialInsertionPort(int input);
		ISerialPort GetOutputSerialInsertionPort(int output);

	    void SetTxComPortSpec(int input, eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType,
	                             eComStopBits stopBits);

		void SetRxComPortSpec(int output, eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType,
	                              eComStopBits stopBits);

		void SendCommand(string command, params object[] args);
	}
}