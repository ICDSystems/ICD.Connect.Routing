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
	}
}
