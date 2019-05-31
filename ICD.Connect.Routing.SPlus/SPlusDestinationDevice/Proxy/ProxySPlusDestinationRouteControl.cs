using System;
using System.Collections.Generic;
using ICD.Connect.Devices.Proxies.Controls;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Proxy
{
	public sealed class ProxySPlusDestinationRouteControl : AbstractProxyDeviceControl, IRouteInputSelectControl
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public ProxySPlusDestinationRouteControl(IProxySPlusDestinationDevice parent, int id)
			: base(parent, id)
		{
		}

		public event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool GetSignalDetectedState(int input, eConnectionType type)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the true if the input is actively being used by the source device.
		/// For example, a display might true if the input is currently on screen,
		/// while a switcher may return true if the input is currently routed.
		/// </summary>
		public bool GetInputActiveState(int input, eConnectionType type)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public ConnectorInfo GetInput(int input)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public bool ContainsInput(int input)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ConnectorInfo> GetInputs()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sets the current active input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		public void SetActiveInput(int? input, eConnectionType type)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the current active input.
		/// </summary>
		public int? GetActiveInput(eConnectionType flag)
		{
			throw new NotImplementedException();
		}
	}
}