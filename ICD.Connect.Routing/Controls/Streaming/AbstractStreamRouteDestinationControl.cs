using ICD.Connect.Devices;

namespace ICD.Connect.Routing.Controls.Streaming
{
	public abstract class AbstractStreamRouteDestinationControl<T> : AbstractRouteDestinationControl<T>, IStreamRouteDestinationControl
		where T : IDeviceBase
	{
		protected AbstractStreamRouteDestinationControl(T parent, int id)
			: base(parent, id)
		{
		}
	}
}
