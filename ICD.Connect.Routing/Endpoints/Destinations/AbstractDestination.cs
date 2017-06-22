namespace ICD.Connect.Routing.Endpoints.Destinations
{
	public abstract class AbstractDestination<TSettings> : AbstractSourceDestinationBase<TSettings>, IDestination
		where TSettings : AbstractDestinationSettings, new()
	{
	}
}
