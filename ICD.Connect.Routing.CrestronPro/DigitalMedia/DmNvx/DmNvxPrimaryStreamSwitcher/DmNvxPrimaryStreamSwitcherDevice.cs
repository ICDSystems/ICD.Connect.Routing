namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxPrimaryStreamSwitcher
{
	public sealed class DmNvxPrimaryStreamSwitcherDevice :
		AbstractDmNvxStreamSwitcherDevice<DmNvxPrimaryStreamSwitcherSettings>
	{
		/// <summary>
		/// Returns true if this switcher handles the primary stream, false for secondary stream.
		/// </summary>
		protected override bool IsPrimaryStream { get { return true; } }

		/// <summary>
		/// Routes the input endpoint to the output endpoint.
		/// </summary>
		/// <param name="inputEndpoint"></param>
		/// <param name="outputEndpoint"></param>
		/// <returns></returns>
		protected override bool SetInputForOutput(NvxEndpointInfo inputEndpoint, NvxEndpointInfo outputEndpoint)
		{
			if (outputEndpoint.Switcher.ServerUrl == inputEndpoint.Switcher.ServerUrl)
				return false;

			outputEndpoint.Switcher.SetServerUrl(inputEndpoint.Switcher.ServerUrl);

			return true;
		}

		/// <summary>
		/// Clears the routing to the given output endpoint.
		/// </summary>
		/// <param name="outputEndpoint"></param>
		/// <returns></returns>
		protected override bool ClearOutput(NvxEndpointInfo outputEndpoint)
		{
			if (outputEndpoint.Switcher.ServerUrl == null)
				return false;

			outputEndpoint.Switcher.SetServerUrl(null);

			return true;
		}
	}
}
