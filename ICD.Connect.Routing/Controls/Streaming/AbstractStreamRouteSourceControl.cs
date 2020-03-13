using System;
using ICD.Connect.Devices;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Controls.Streaming
{
	public abstract class AbstractStreamRouteSourceControl<T> : AbstractRouteSourceControl<T>, IStreamRouteSourceControl
		where T : IDeviceBase
	{
		#region Events

		public abstract event EventHandler<StreamUriEventArgs> OnStreamUriChanged;

		#endregion

		protected AbstractStreamRouteSourceControl(T parent, int id)
			: base(parent, id)
		{
		}

		public abstract Uri GetStreamForOutput(int output);
	}
}
