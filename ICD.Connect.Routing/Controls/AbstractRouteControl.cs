using ICD.Connect.Devices;
using ICD.Connect.Devices.Controls;

namespace ICD.Connect.Routing.Controls
{
	public abstract class AbstractRouteControl<T> : AbstractDeviceControl<T>, IRouteControl
		where T : IDeviceBase
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractRouteControl(T parent, int id)
			: base(parent, id)
		{
		}
	}
}
