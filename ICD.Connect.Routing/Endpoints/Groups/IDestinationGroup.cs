using System.Collections.Generic;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Routing.Endpoints.Groups
{
	public interface IDestinationGroup : IOriginator
	{
		IEnumerable<int> Destinations { get; set; }
		bool Remote { get; set; }
		int Order { get; set; }
		bool Disable { get; set; }
	}
}
