namespace ICD.Connect.Routing.Endpoints.Sources
{
	public abstract class AbstractSourceSettings : AbstractSourceDestinationBaseSettings, ISourceSettings
	{
		private const string SOURCE_ELEMENT = "Source";

		/// <summary>
		/// Gets the xml element.
		/// </summary>
		protected override string Element { get { return SOURCE_ELEMENT; } }

		protected static void ParseXml(AbstractSourceSettings instance, string xml)
		{
			AbstractSourceDestinationBaseSettings.ParseXml(instance, xml);
		}
	}
}
