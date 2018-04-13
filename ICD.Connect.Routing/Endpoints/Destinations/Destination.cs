using System.Collections.Generic;
using System.Linq;
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
			: this(0, 0, Enumerable.Empty<int>())
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="deviceId"></param>
		/// <param name="controlId"></param>
		/// <param name="addresses"></param>
		public Destination(int deviceId, int controlId, IEnumerable<int> addresses)
			: this(deviceId, controlId, addresses, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="deviceId"></param>
		/// <param name="controlId"></param>
		/// <param name="addresses"></param>
		/// <param name="name"></param>
		public Destination(int deviceId, int controlId, IEnumerable<int> addresses, string name)
			: this(0, deviceId, controlId, addresses, name, false, int.MaxValue, false)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="deviceId"></param>
		/// <param name="controlId"></param>
		/// <param name="addresses"></param>
		/// <param name="name"></param>
		/// <param name="remote"></param>
		/// <param name="order"></param>
		/// <param name="disable"></param>
		[JsonConstructor]
		public Destination(int id, int deviceId, int controlId, IEnumerable<int> addresses, string name, bool remote, int order, bool disable)
		{
			Id = id;
			Device = deviceId;
			Control = controlId;
			SetAddresses(addresses);
			Name = name;
			Remote = remote;
			Order = order;
			Disable = disable;
		}

		#endregion
	}
}
