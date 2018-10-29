using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxBaseClass;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxSecondaryStreamSwitcher
{
	public sealed class DmNvxSecondaryStreamSwitcherDevice :
		AbstractDmNvxStreamSwitcherDevice<DmNvxSecondaryStreamSwitcherDeviceSettings>
	{
		/// <summary>
		/// Returns true if this switcher handles the primary stream, false for secondary stream.
		/// </summary>
		protected override bool IsPrimaryStream { get { return false; } }

		/// <summary>
		/// Routes the input endpoint to the output endpoint.
		/// </summary>
		/// <param name="inputEndpoint"></param>
		/// <param name="outputEndpoint"></param>
		/// <returns></returns>
		protected override bool SetInputForOutput(NvxEndpointInfo inputEndpoint, NvxEndpointInfo outputEndpoint)
		{
			if (outputEndpoint.Switcher.SecondaryAudioMulticastAddress == inputEndpoint.Switcher.SecondaryAudioMulticastAddress)
				return false;

			outputEndpoint.Switcher.SetSecondaryAudioMulticastAddress(inputEndpoint.Switcher.SecondaryAudioMulticastAddress);

			return true;
		}

		/// <summary>
		/// Clears the routing to the given output endpoint.
		/// </summary>
		/// <param name="outputEndpoint"></param>
		/// <returns></returns>
		protected override bool ClearOutput(NvxEndpointInfo outputEndpoint)
		{
			if (outputEndpoint.Switcher.SecondaryAudioMulticastAddress == null)
				return false;

			outputEndpoint.Switcher.SetSecondaryAudioMulticastAddress(null);

			return true;
		}

		#region Switcher Callbacks

		/// <summary>
		/// Subscribe to the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		protected override void Subscribe(DmNvxBaseClassSwitcherControl switcher)
		{
			base.Subscribe(switcher);

			if (switcher == null)
				return;

			switcher.OnSecondaryAudioMulticastAddressChange += SwitcherOnSecondaryAudioMulticastAddressChange;
		}

		/// <summary>
		/// Unsubscribe from the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		protected override void Unsubscribe(DmNvxBaseClassSwitcherControl switcher)
		{
			base.Unsubscribe(switcher);

			if (switcher == null)
				return;

			switcher.OnSecondaryAudioMulticastAddressChange -= SwitcherOnSecondaryAudioMulticastAddressChange;
		}

		/// <summary>
		/// Called when the switcher secondary audio multicast address changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="stringEventArgs"></param>
		private void SwitcherOnSecondaryAudioMulticastAddressChange(object sender, StringEventArgs stringEventArgs)
		{
			DmNvxBaseClassSwitcherControl switcher = sender as DmNvxBaseClassSwitcherControl;
			if (switcher == null)
				return;

			UpdateRouting(switcher);
		}

		#endregion
	}
}
