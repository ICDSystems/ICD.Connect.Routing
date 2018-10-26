using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;

namespace ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint
{
	public interface IDtpCrosspointDevice : IExtronSwitcherDevice
	{
		event EventHandler<IntEventArgs> OnInputPortInitialized;
		event EventHandler<IntEventArgs> OnOutputPortInitialized;

		ISerialPort GetInputSerialInsertionPort(int input);
		ISerialPort GetOutputSerialInsertionPort(int output);

		void SetTxComPortSpec(int input, eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType,
								 eComStopBits stopBits);

		void SetRxComPortSpec(int output, eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType,
								  eComStopBits stopBits);
	}
}