using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Routing.Extron.Devices.Switchers;

namespace ICD.Connect.Routing.Extron.Devices.Endpoints
{
	public interface IDtpHdmiDevice : IDevice
	{
		ISerialPort GetSerialInsertionPort();

		event EventHandler<BoolEventArgs> OnPortInitialized;

		void InitializeComPort(eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType, eComStopBits stopBits);
	}
}