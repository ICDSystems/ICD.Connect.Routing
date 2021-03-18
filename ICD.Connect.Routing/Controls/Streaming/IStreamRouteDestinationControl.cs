using System;
using ICD.Common.Properties;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Controls.Streaming
{
	public interface IStreamRouteDestinationControl : IRouteDestinationControl
	{
		event EventHandler<StreamUriEventArgs> OnInputStreamUriChanged;

		bool SetStreamForInput(int input, [CanBeNull] Uri stream);

		[CanBeNull]
		Uri GetStreamForInput(int input);
    }
}
