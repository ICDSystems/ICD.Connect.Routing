﻿using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Endpoints
{
	public interface ISourceDestinationBaseSettings : ISettings
	{
		/// <summary>
		/// Gets the endpoint device.
		/// </summary>
		int Device { get; set; }

		/// <summary>
		/// Gets the endpoint device control.
		/// </summary>
		int Control { get; set; }

		/// <summary>
		/// Gets the endpoint connector address.
		/// </summary>
		int Address { get; set; }

		/// <summary>
		/// Specifies which media types to use for the source.
		/// </summary>
		eConnectionType ConnectionType { get; set; }

		/// <summary>
		/// Specifies custom ordering of the instance to the end user.
		/// </summary>
		int Order { get; set; }

		/// <summary>
		/// Shorthand for disabling an instance in the system.
		/// </summary>
		bool Disable { get; set; }
	}
}
