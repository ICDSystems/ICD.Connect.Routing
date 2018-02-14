using System;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.EventArguments;
using ICD.Connect.Protocol.XSig;

namespace ICD.Connect.Routing.Crestron2Series.Devices
{
	public interface IDmps300CDevice : IDevice
	{
		event EventHandler<XSigEventArgs> OnSigEvent;

		string Address { get; }

		ushort Port { get; }

		/// <summary>
		/// Sends the sig data to the device.
		/// </summary>
		/// <param name="sig"></param>
		bool SendData(IXSig sig);
	}
}
