namespace ICD.Connect.Routing.Groups.Endpoints.Destinations
{
	public abstract class AbstractDestinationGroupSettings : AbstractSourceDestinationGroupCommonSettings, IDestinationGroupSettings
	{
		protected override string RootElement { get { return "Destinations"; } }

		protected override string ChildElement { get { return "Destination"; } }
	}
}
