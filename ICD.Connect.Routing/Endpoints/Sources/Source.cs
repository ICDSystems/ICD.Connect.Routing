using System.Collections.Generic;
using System.Linq;
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
			: this(0, 0, Enumerable.Empty<int>())
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="deviceId"></param>
		/// <param name="controlId"></param>
		/// <param name="addresses"></param>
		public Source(int deviceId, int controlId, IEnumerable<int> addresses)
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
		public Source(int deviceId, int controlId, IEnumerable<int> addresses, string name)
			: this(0, deviceId, controlId, addresses, name, false, int.MaxValue, false)
		{
		}

		[JsonConstructor]
		public Source(int id, int deviceId, int controlId, IEnumerable<int> addresses, string name, bool remote, int order, bool disable)
		{
			Id = id;
			Device = deviceId;
			Control = controlId;
			SetAddresses(addresses);
			Name = name;
			Remote = remote;
			Disable = disable;
			Order = order;
		}

		#endregion
	}
}
