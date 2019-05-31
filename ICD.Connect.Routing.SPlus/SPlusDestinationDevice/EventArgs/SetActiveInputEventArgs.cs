using ICD.Common.Utils.EventArguments;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.EventArgs
{
	public sealed class SetActiveInputEventArgs : GenericEventArgs<int?>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="data"></param>
		public SetActiveInputEventArgs(int? data) : base(data)
		{
		}
	}
}