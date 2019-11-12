using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings.Originators;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Endpoints
{
	public interface ISourceDestinationCommonCollection<T> : IOriginatorCollection<T>
		where T : class, ISourceDestinationCommon
	{
		/// <summary>
		/// Raised when the disabled state of a source destination base changes.
		/// </summary>
		event EventHandler<SourceDestinationBaseDisabledStateChangedEventArgs> OnSourceDestinationBaseDisabledStateChanged;

		/// <summary>
		/// Gets the children with the given endpoint info.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <returns></returns>
		[NotNull]
		IEnumerable<T> GetChildren(EndpointInfo endpoint);

		/// <summary>
		/// Gets the children with the given endpoint info.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[NotNull]
		IEnumerable<T> GetChildren(EndpointInfo endpoint, eConnectionType type);

		/// <summary>
		/// Gets the children with the given device id.
		/// </summary>
		/// <param name="deviceId"></param>
		/// <returns></returns>
		[NotNull]
		IEnumerable<T> GetChildrenForDevice(int deviceId);
	}
}
