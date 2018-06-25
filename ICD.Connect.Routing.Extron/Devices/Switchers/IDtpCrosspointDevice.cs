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

		HostInfo? GetInputComPortHostInfo(int input);
		HostInfo? GetOutputComPortHostInfo(int output);

	    void InitializeTxComPort(int input, eExtronPortInsertionMode mode, eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType,
	                             eComStopBits stopBits);

		void InitializeRxComPort(int output, eExtronPortInsertionMode mode, eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType,
	                              eComStopBits stopBits);
	}
}