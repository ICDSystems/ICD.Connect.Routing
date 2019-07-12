using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.Routing.Connections;
using Newtonsoft.Json;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.RouteState
{
	[JsonConverter(typeof(RouteStateConverter))]
	public sealed class RouteState
	{
		[CanBeNull]
		public int[] InputsDetected { get; set; }

		[CanBeNull]
		public Dictionary<eConnectionType, int?> InputsActive { get; set; }
	}
}