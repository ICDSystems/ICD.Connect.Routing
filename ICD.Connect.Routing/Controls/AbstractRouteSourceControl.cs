﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Controls
{
	public abstract class AbstractRouteSourceControl<T> : AbstractRouteControl<T>, IRouteSourceControl
		where T : IDeviceBase
	{
		public abstract event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractRouteSourceControl(T parent, int id)
			: base(parent, id)
		{
		}

		/// <summary>
		/// Returns true if the device is actively transmitting on the given output.
		/// This is NOT the same as sending video, since some devices may send an
		/// idle signal by default.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public abstract bool GetActiveTransmissionState(int output, eConnectionType type);

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public virtual ConnectorInfo GetOutput(int output)
		{
			return GetOutputs().First(c => c.Address == output);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public virtual bool ContainsOutput(int output)
		{
			return GetOutputs().Any(c => c.Address == output);
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<ConnectorInfo> GetOutputs();
	}
}
