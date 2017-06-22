using Newtonsoft.Json;

namespace ICD.Connect.Routing.Endpoints.Sources
{
	public sealed class Source : AbstractSource<SourceSettings>
	{
		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		public Source()
			: this(0, 0, 0)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="deviceId"></param>
		/// <param name="controlId"></param>
		/// <param name="outputAddress"></param>
		public Source(int deviceId, int controlId, int outputAddress)
			: this(deviceId, controlId, outputAddress, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="deviceId"></param>
		/// <param name="controlId"></param>
		/// <param name="outputAddress"></param>
		/// <param name="name"></param>
		public Source(int deviceId, int controlId, int outputAddress, string name)
		{
			Endpoint = new EndpointInfo(deviceId, controlId, outputAddress);
			Name = name;
		}

		[JsonConstructor]
		public Source(int id, EndpointInfo endpoint, string name, bool remote, int order, bool disable)
		{
			Id = id;
			Endpoint = endpoint;
			Name = name;
			Remote = remote;
			Disable = disable;
			Order = order;
		}

		#endregion
	}
}
