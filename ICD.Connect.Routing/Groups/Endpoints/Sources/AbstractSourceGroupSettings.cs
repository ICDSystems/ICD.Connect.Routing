namespace ICD.Connect.Routing.Groups.Endpoints.Sources
{
	public abstract class AbstractSourceGroupSettings : AbstractSourceDestinationBaseGroupSettings, ISourceGroupSettings
	{
		protected override string RootElement { get { return "Sources"; } }

		protected override string ChildElement { get { return "Source"; } }
	}
}
