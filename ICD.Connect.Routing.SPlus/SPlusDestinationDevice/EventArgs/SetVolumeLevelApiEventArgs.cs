using ICD.Connect.API.EventArguments;
using ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Proxy;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.EventArgs
{
	public sealed class SetVolumeLevelApiEventArgs : AbstractGenericApiEventArgs<ushort>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="data"></param>
		public SetVolumeLevelApiEventArgs(ushort data) : base(SPlusDestinationApi.EVENT_SET_VOLUME_LEVEL, data)
		{
		}
	}
}