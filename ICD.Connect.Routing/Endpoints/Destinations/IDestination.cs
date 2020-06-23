namespace ICD.Connect.Routing.Endpoints.Destinations
{
	public interface IDestination : ISourceDestinationCommon, IDestinationBase
	{
		/// <summary>
		/// Gets the group name that is used when the core loads to generate destination groups.
		/// </summary>
		string Group { get; }
	}
}
