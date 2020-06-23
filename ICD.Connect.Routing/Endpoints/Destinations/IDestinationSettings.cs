namespace ICD.Connect.Routing.Endpoints.Destinations
{
	public interface IDestinationSettings : ISourceDestinationCommonSettings
	{
		/// <summary>
		/// Gets/sets the group name that is used when the core loads to generate destination groups.
		/// </summary>
		string Group { get; set; }
	}
}
