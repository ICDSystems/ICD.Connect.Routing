using System;
using ICD.Common.Utils.Collections;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Routing.Extron.Controls.Routing;
using ICD.Connect.Routing.Extron.Devices.Endpoints;

namespace ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint
{
	public static class DtpUtils
	{
		private static readonly BiDictionary<eComParityType, char> s_ParityMap =
			new BiDictionary<eComParityType, char>
			{
				{eComParityType.Even, 'e'},
				{eComParityType.Odd, 'o'},
				{eComParityType.Mark, 's'},
				{eComParityType.None, 'n'},
			};

		/// <summary>
		/// Gets the character for the given parity type.
		/// </summary>
		/// <param name="parityType"></param>
		/// <returns></returns>
		public static char GetParityChar(eComParityType parityType)
		{
			return s_ParityMap.GetValue(parityType);
		}

		/// <summary>
		/// Gets the parity type for the given character.
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		public static eComParityType FromParityChar(char character)
		{
			character = char.ToLower(character);

			return s_ParityMap.GetKey(character);
		}

		/// <summary>
		/// Gets the switcher address and input/output direction for the given offset address.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="offset"></param>
		/// <param name="inputOutput"></param>
		/// <returns></returns>
		public static int? GetSwitcherAddressFromPortOffset(IDtpCrosspointDevice device, int offset, out eDtpInputOuput inputOutput)
		{
			inputOutput = eDtpInputOuput.None;

			// 1 is reserved for the switcher control port
            if (offset <= 1)
				return null;
			offset -= 1;

			int inputs = GetNumberOfSwitcherInputs(device);
			if (offset <= inputs)
			{
				inputOutput = eDtpInputOuput.Input;
				return offset;
			}

			offset -= inputs;
			if (offset <= GetNumberOfSwitcherOutputs(device))
			{
				inputOutput = eDtpInputOuput.Output;
				return offset;
			}

			return null;
		}

		/// <summary>
		/// Gets the port offset for the input/output address on the switcher.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="address"></param>
		/// <param name="inputOutput"></param>
		/// <returns></returns>
		public static int? GetPortOffsetFromSwitcher(IDtpCrosspointDevice device, int address, eDtpInputOuput inputOutput)
		{
			int portOffset;

			switch (inputOutput)
			{
				case eDtpInputOuput.Input:
					portOffset = address - GetNumberOfSwitcherInputs(device) + device.NumberOfDtpInputPorts;
					if (portOffset <= 0)
						return null;
					break;

				case eDtpInputOuput.Output:
					portOffset = address - GetNumberOfSwitcherOutputs(device) + (device.NumberOfDtpInputPorts + device.NumberOfDtpOutputPorts);
					if (portOffset <= device.NumberOfDtpInputPorts)
						return null;
					break;

				default:
					throw new ArgumentOutOfRangeException("inputOutput");
			}

			return portOffset;
		}

		/// <summary>
		/// Returns the number of HDMI and DTP inputs on the switcher, e.g. an 8x4 returns 8.
		/// </summary>
		/// <returns></returns>
		public static int GetNumberOfSwitcherInputs(IDtpCrosspointDevice device)
		{
			var switcherControl = device.Controls.GetControl<IExtronSwitcherControl>(0);
			return switcherControl.NumberOfInputs;
		}

		/// <summary>
		/// Returns the number of HDMI and DTP outputs on the switcher, e.g. an 8x4 returns 4.
		/// </summary>
		/// <returns></returns>
		public static int GetNumberOfSwitcherOutputs(IDtpCrosspointDevice device)
		{
			var switcherControl = device.Controls.GetControl<IExtronSwitcherControl>(0);
			return switcherControl.NumberOfOutputs;
		}
	}
}
