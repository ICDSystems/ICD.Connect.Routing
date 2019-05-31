using ICD.Common.Utils.EventArguments;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.EventArgs
{
	public sealed class PowerControlEventArgs : GenericEventArgs<bool>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="data"></param>
		public PowerControlEventArgs(bool data) : base(data)
		{
		}
	}
}