using System;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxBaseClass
{
	public enum eDeviceMode
	{
		Receiver,
		Transmitter,
	}

	public static class DeviceModeExtensions
	{
#if !NETSTANDARD
		/// <summary>
		/// Converts the Crestron device mode into the common device mode enum.
		/// </summary>
		/// <param name="deviceMode"></param>
		/// <returns></returns>
		public static eDeviceMode FromCrestron(this Crestron.SimplSharpPro.DM.Streaming.eDeviceMode deviceMode)
		{
			switch (deviceMode)
			{
				case Crestron.SimplSharpPro.DM.Streaming.eDeviceMode.Receiver:
					return eDeviceMode.Receiver;
				case Crestron.SimplSharpPro.DM.Streaming.eDeviceMode.Transmitter:
					return eDeviceMode.Transmitter;
				default:
					throw new ArgumentOutOfRangeException("deviceMode");
			}
		}

		/// <summary>
		/// Converts the common device mode into the Crestron device mode.
		/// </summary>
		/// <param name="deviceMode"></param>
		/// <returns></returns>
		public static Crestron.SimplSharpPro.DM.Streaming.eDeviceMode ToCrestron(this eDeviceMode deviceMode)
		{
			switch (deviceMode)
			{
				case eDeviceMode.Receiver:
					return Crestron.SimplSharpPro.DM.Streaming.eDeviceMode.Receiver;
				case eDeviceMode.Transmitter:
					return Crestron.SimplSharpPro.DM.Streaming.eDeviceMode.Transmitter;
				default:
					throw new ArgumentOutOfRangeException("deviceMode");
			}
		}
#endif
	}
}
