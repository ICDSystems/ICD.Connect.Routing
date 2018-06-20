using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;

namespace ICD.Connect.Routing.Extron.Devices.DtpCrosspointBase
{
	public interface IDtpCrosspointDevice : IDevice
	{
		event EventHandler<BoolEventArgs> OnInitializedChanged;

		HostInfo? GetInputComPortHostInfo(int input);
		HostInfo? GetOutputComPortHostInfo(int output);

		void SetInputComPortSpec(int input, eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType, eComStopBits stopBits)
		void SetOutputComPortSpec(int output, eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType, eComStopBits stopBits)
	}
}