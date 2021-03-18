using System;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.Devices.Streaming
{
	public interface IStreamSourceDevice : IDevice
	{
		/// <summary>
		/// Raised when the stream URI changes
		/// </summary>
		[PublicAPI]
		event EventHandler<GenericEventArgs<Uri>> OnStreamUriChanged;

		/// <summary>
		/// URI of the stream
		/// </summary>
		[PublicAPI]
		[CanBeNull]
		Uri StreamUri { get; }

		/// <summary>
		/// Goes to the given channel stream
		/// </summary>
		/// <param name="uri"></param>
		[PublicAPI]
		void SetStreamUri(Uri uri);
	}
}
