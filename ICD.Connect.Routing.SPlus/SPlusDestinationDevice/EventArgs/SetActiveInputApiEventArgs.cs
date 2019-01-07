using ICD.Connect.API.EventArguments;
using ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Proxy;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.EventArgs
{
	public sealed class SetActiveInputApiEventArgs : AbstractGenericApiEventArgs<int?>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="data"></param>
		public SetActiveInputApiEventArgs(int? data) : base(SPlusDestinationApi.EVENT_SET_ACTIVE_INPUT, data)
		{
		}
	}
}