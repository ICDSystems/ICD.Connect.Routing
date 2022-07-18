using ICD.Common.Utils.EventArguments;

namespace ICD.Connect.Routing.SPlus.EventArgs
{
	public sealed class SetVolumeLevelEventArgs : GenericEventArgs<ushort>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="data"></param>
		public SetVolumeLevelEventArgs(ushort data) : base(data)
		{
		}
	}
}