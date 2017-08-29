using ICD.Common.Utils.EventArguments;
using ICD.Connect.Routing.Endpoints.Sources;

namespace ICD.Connect.Routing.EventArguments
{
	public sealed class SourceEventArgs : GenericEventArgs<ISource>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="data"></param>
		public SourceEventArgs(ISource data)
			: base(data)
		{
		}
	}
}
