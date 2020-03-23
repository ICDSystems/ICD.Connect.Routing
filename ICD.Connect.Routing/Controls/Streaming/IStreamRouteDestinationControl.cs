using System;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Controls.Streaming
{
	public interface IStreamRouteDestinationControl : IRouteDestinationControl
	{
		event EventHandler<StreamUriEventArgs> OnInputStreamUriChanged;

		bool SetStreamForInput(int input, Uri stream);

		Uri GetStreamForInput(int input);
    }
}
