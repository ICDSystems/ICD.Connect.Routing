using System;
using ICD.Connect.Devices;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Controls.Streaming
{
	public abstract class AbstractStreamRouteMidpointControl<T> : AbstractRouteMidpointControl<T>, IStreamRouteMidpointControl
		where T : IDeviceBase
	{
		public abstract event EventHandler<StreamUriEventArgs> OnStreamUriChanged;

		protected AbstractStreamRouteMidpointControl(T parent, int id)
			: base(parent, id)
		{
		}

		public abstract Uri GetStreamForOutput(int output);
	}
}
