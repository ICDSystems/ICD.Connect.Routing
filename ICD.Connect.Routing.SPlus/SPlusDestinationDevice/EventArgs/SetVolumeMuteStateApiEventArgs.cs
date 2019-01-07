using ICD.Connect.API.EventArguments;
using ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Proxy;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.EventArgs
{
	public sealed class SetVolumeMuteStateApiEventArgs : AbstractGenericApiEventArgs<bool>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="data"></param>
		public SetVolumeMuteStateApiEventArgs(bool data) : base(SPlusDestinationApi.EVENT_SET_VOLUME_MUTE_STATE, data)
		{
		}
	}
}