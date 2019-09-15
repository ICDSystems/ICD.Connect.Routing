namespace ICD.Connect.Routing.Groups.Endpoints.Sources
{
	public abstract class AbstractSourceGroupSettings : AbstractSourceDestinationGroupCommonSettings, ISourceGroupSettings
	{
		protected override string RootElement { get { return "Sources"; } }

		protected override string ChildElement { get { return "Source"; } }
	}
}
