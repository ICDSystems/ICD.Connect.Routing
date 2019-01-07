using ICD.Connect.API.EventArguments;
using ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Proxy;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.EventArgs
{
	public sealed class PowerControlApiEventArgs : AbstractGenericApiEventArgs<bool>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="data"></param>
		public PowerControlApiEventArgs(bool data) : base(SPlusDestinationApi.EVENT_SET_POWER_STATE, data)
		{
		}
	}
}