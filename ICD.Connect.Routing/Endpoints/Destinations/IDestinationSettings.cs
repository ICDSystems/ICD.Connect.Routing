namespace ICD.Connect.Routing.Endpoints.Destinations
{
	public interface IDestinationSettings : ISourceDestinationCommonSettings
	{

		string DestinationGroupString { get; set; }

	}
}
