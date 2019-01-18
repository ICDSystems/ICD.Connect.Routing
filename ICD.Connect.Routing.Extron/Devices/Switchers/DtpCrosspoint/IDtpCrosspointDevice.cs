using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Routing.Extron.Devices.Endpoints;

namespace ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint
{
	public delegate void PortInitializedCallback(IDtpCrosspointDevice device, int address, eDtpInputOuput inputOutput);

	public delegate void PortComSpecCallback(IDtpCrosspointDevice device, int address, eDtpInputOuput inputOutput, ComSpec comSpec);

	public interface IDtpCrosspointDevice : IExtronSwitcherDevice
	{
		/// <summary>
		/// Raised when a serial port is initialized.
		/// </summary>
		event PortInitializedCallback OnPortInitialized;

		/// <summary>
		/// Raised when a serial port comspec changes.
		/// </summary>
		event PortComSpecCallback OnPortComSpecChanged;

		int NumberOfDtpInputPorts { get; }
		int NumberOfDtpOutputPorts { get; }

		ISerialPort GetSerialInsertionPort(int address, eDtpInputOuput inputOutput);

		void SetComPortSpec(int address, eDtpInputOuput inputOutput, eComBaudRates baudRate, eComDataBits dataBits,
		                    eComParityType parityType, eComStopBits stopBits);
	}
}