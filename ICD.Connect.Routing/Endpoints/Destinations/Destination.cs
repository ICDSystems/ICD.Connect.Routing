using Newtonsoft.Json;

namespace ICD.Connect.Routing.Endpoints.Destinations
{
	public sealed class Destination : AbstractDestination<DestinationSettings>
	{
		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		public Destination()
			: this(0, 0, 0)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="deviceId"></param>
		/// <param name="controlId"></param>
		/// <param name="inputAddress"></param>
		public Destination(int deviceId, int controlId, int inputAddress)
			: this(deviceId, controlId, inputAddress, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="deviceId"></param>
		/// <param name="controlId"></param>
		/// <param name="inputAddress"></param>
		/// <param name="name"></param>
		public Destination(int deviceId, int controlId, int inputAddress, string name)
		{
			Endpoint = new EndpointInfo(deviceId, controlId, inputAddress);
			Name = name;
		}

		[JsonConstructor]
		public Destination(int id, EndpointInfo endpoint, string name, bool remote, int order, bool disable)
		{
			Id = id;
			Endpoint = endpoint;
			Name = name;
			Remote = remote;
			Order = order;
			Disable = disable;
		}

		#endregion
	}
}
