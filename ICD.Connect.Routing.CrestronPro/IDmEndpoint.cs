#if SIMPLSHARP
using Crestron.SimplSharpPro.DM.Endpoints;
#endif
using ICD.Common.Properties;

namespace ICD.Connect.Routing.CrestronPro
{
	[PublicAPI]
	public interface IDmEndpoint
	{
#if SIMPLSHARP
		/// <summary>
		/// Gets the wrapped DMEndpointBase device.
		/// </summary>
		DMEndpointBase Device { get; }
#endif
	}
}
