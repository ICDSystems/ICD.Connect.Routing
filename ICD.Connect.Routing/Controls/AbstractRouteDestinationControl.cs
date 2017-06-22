using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Controls
{
	public abstract class AbstractRouteDestinationControl<T> : AbstractRouteControl<T>, IRouteDestinationControl
		where T : IDevice
	{
		public abstract event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public abstract event EventHandler OnActiveInputsChanged;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractRouteDestinationControl(T parent, int id)
			: base(parent, id)
		{
		}

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public abstract bool GetSignalDetectedState(int input, eConnectionType type);

		/// <summary>
		/// Returns the true if the input is actively being used by the source device.
		/// For example, a display might true if the input is currently on screen,
		/// while a switcher may return true if the input is currently routed.
		/// </summary>
		public abstract bool GetInputActiveState(int input, eConnectionType type);

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public virtual ConnectorInfo GetInput(int input)
		{
			return GetInputs().First(c => c.Address == input);
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public virtual bool ContainsInput(int input)
		{
			return GetInputs().Any(c => c.Address == input);
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<ConnectorInfo> GetInputs();
	}
}
