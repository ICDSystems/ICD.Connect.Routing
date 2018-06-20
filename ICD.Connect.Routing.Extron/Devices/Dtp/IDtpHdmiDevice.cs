using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Routing.Extron.Devices.DtpCrosspointBase;

namespace ICD.Connect.Routing.Extron.Devices.Dtp
{
	public interface IDtpHdmiDevice : IDevice
	{
		HostInfo? GetComPortHostInfo();

		event EventHandler<BoolEventArgs> OnInitializedChanged;

		void SetComPortSpec(eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType, eComStopBits stopBits);
	}
}