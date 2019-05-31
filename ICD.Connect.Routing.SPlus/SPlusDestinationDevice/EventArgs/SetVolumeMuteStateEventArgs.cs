using ICD.Common.Utils.EventArguments;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.EventArgs
{
	public sealed class SetVolumeMuteStateEventArgs : GenericEventArgs<bool>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="data"></param>
		public SetVolumeMuteStateEventArgs(bool data) : base(data)
		{
		}
	}
}