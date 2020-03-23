using System;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Controls.Streaming
{
	public interface IStreamRouteSourceControl : IRouteSourceControl
	{
		event EventHandler<StreamUriEventArgs> OnOutputStreamUriChanged;

		Uri GetStreamForOutput(int output);
	}
}
