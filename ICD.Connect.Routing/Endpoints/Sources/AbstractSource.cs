namespace ICD.Connect.Routing.Endpoints.Sources
{
	public abstract class AbstractSource<TSettings> : AbstractSourceDestinationBase<TSettings>, ISource
		where TSettings : AbstractSourceSettings, new()
	{
	}
}
