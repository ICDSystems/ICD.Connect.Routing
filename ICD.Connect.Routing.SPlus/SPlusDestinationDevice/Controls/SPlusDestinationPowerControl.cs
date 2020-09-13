using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.Controls.Power;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Controls
{
	public sealed class SPlusDestinationPowerControl : AbstractPowerDeviceControl<Device.SPlusDestinationDevice>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public SPlusDestinationPowerControl(Device.SPlusDestinationDevice parent, int id) : base(parent, id)
		{
		}

		/// <summary>
		/// Powers on the device.
		/// </summary>
		protected override void PowerOnFinal()
		{
			Parent.PowerOn();
		}

		/// <summary>
		/// Powers off the device.
		/// </summary>
		protected override void PowerOffFinal()
		{
			Parent.PowerOff();
		}

		/// <summary>
		/// Sets the power state from the device
		/// </summary>
		/// <param name="powerState"></param>
		internal void SetPowerStateFeedback(ePowerState powerState)
		{
			PowerState = powerState;
		}
	}
}