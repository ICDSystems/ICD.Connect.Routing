using System.Collections.Generic;
using ICD.Connect.Routing.Connections;
using Newtonsoft.Json;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.RouteState
{
	[JsonConverter(typeof(RouteStateConverter))]
	public sealed class RouteState
	{
		public int[] InputsDetected { get; set; }

		public Dictionary<eConnectionType, int?> InputsActive { get; set; }
	}
}