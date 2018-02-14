namespace ICD.Connect.Routing.Endpoints.Destinations
{
	public abstract class AbstractDestinationSettings : AbstractSourceDestinationBaseSettings, IDestinationSettings
	{
		private const string DESTINATION_ELEMENT = "Destination";

		/// <summary>
		/// Gets the xml element.
		/// </summary>
		protected override string Element { get { return DESTINATION_ELEMENT; } }
	}
}
