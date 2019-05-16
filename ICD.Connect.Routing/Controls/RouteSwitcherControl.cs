using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Devices;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Controls
{
	public sealed class RouteSwitcherControl : AbstractRouteSwitcherControl<IRouteSwitcherDevice>
	{
		/// <summary>
		/// Raised when an input source status changes.
		/// </summary>
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;

		/// <summary>
		/// Raised when the device starts/stops actively using an input, e.g. unroutes an input.
		/// </summary>
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		/// <summary>
		/// Called when a route changes.
		/// </summary>
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public RouteSwitcherControl(IRouteSwitcherDevice parent, int id)
			: base(parent, id)
		{
			Subscribe(parent);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnSourceDetectionStateChange = null;
			OnActiveInputsChanged = null;
			OnActiveTransmissionStateChanged = null;
			OnRouteChange = null;

			base.DisposeFinal(disposing);

			Unsubscribe(Parent);
		}

		#region Methods

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			return Parent.GetSignalDetectedState(input, type);
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int input)
		{
			return Parent.GetInput(input);
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return Parent.ContainsInput(input);
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			return Parent.GetInputs();
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int address)
		{
			return Parent.GetOutput(address);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return Parent.ContainsOutput(output);
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			return Parent.GetOutputs();
		}

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			return Parent.GetOutputs(input, type);
		}

		/// <summary>
		/// Gets the input routed to the given output matching the given type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Type has multiple flags.</exception>
		public override ConnectorInfo? GetInput(int output, eConnectionType type)
		{
			return Parent.GetInput(output, type);
		}

		/// <summary>
		/// Performs the given route operation.
		/// </summary>
		/// <param name="info"/>
		/// <returns/>
		public override bool Route(RouteOperation info)
		{
			return Parent.Route(info);
		}

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if successfully cleared.</returns>
		public override bool ClearOutput(int output, eConnectionType type)
		{
			return Parent.ClearOutput(output, type);
		}

		public override IEnumerable<string> GetSwitcherVideoInputIds()
		{
			return GetInputs().Where(i => i.ConnectionType.HasFlag(eConnectionType.Video))
			                  .Select(i => string.Format("NVX Stream {0}", i.Address));
		}

		/// <summary>
		/// Gets the Input Name of the switcher (ie Content, Display In)
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> GetSwitcherVideoInputNames()
		{
			return GetSwitcherVideoInputIds();
		}

		/// <summary>
		/// Gets the Input Sync Type of the switcher's inputs (ie HDMI when HDMI Sync is detected, empty when not detected)
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> GetSwitcherVideoInputSyncType()
		{
			foreach (var input in GetInputs().Where(i => i.ConnectionType.HasFlag(eConnectionType.Video)))
			{
				bool syncState = GetSignalDetectedState(input.Address, eConnectionType.Video);
				if (!syncState)
				{
					yield return string.Empty;
					continue;
				}

				yield return "NVX";
			}
		}

		/// <summary>
		/// Gets the Input Resolution for the switcher's inputs (ie 1920x1080, or empty for no sync)
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> GetSwitcherVideoInputResolution()
		{
			foreach(var input in GetInputs().Where(i=> i.ConnectionType.HasFlag(eConnectionType.Video)))
			{
				bool syncState = GetSignalDetectedState(input.Address, eConnectionType.Video);
				if (!syncState)
				{
					yield return string.Empty;
					continue;
				}

				yield return "Unknown";
			}
		}

		#endregion

		#region Parent Callbacks

		/// <summary>
		/// Susbcribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Subscribe(IRouteSwitcherDevice parent)
		{
			parent.OnSourceDetectionStateChange += ParentOnSourceDetectionStateChange;
			parent.OnActiveInputsChanged += ParentOnActiveInputsChanged;
			parent.OnActiveTransmissionStateChanged += ParentOnActiveTransmissionStateChanged;
			parent.OnRouteChange += ParentOnRouteChange;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Unsubscribe(IRouteSwitcherDevice parent)
		{
			parent.OnSourceDetectionStateChange -= ParentOnSourceDetectionStateChange;
			parent.OnActiveInputsChanged -= ParentOnActiveInputsChanged;
			parent.OnActiveTransmissionStateChanged -= ParentOnActiveTransmissionStateChanged;
			parent.OnRouteChange -= ParentOnRouteChange;
		}

		private void ParentOnRouteChange(object sender, RouteChangeEventArgs eventArgs)
		{
			OnRouteChange.Raise(this, new RouteChangeEventArgs(eventArgs));
		}

		private void ParentOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs eventArgs)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(eventArgs));
		}

		private void ParentOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs eventArgs)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(eventArgs));
		}

		private void ParentOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs eventArgs)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(eventArgs));
		}

		#endregion
	}
}
