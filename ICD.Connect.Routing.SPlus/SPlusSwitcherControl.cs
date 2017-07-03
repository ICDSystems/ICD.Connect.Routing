using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.SPlus
{
	public sealed class SPlusSwitcherControl : AbstractRouteSwitcherControl<SPlusSwitcher>
	{
		#region S+ Delegates

		public delegate bool SPlusGetSignalDetectedState(int input, eConnectionType type);

		public delegate IEnumerable<ConnectorInfo> SPlusGetInputs();

		public delegate IEnumerable<ConnectorInfo> SPlusGetOutputs();

		public delegate IEnumerable<ConnectorInfo> SPlusGetInputsForOutput(int output, eConnectionType type);

		public delegate bool SPlusRoute(RouteOperation info);

		public delegate bool SPlusClearOutput(int output, eConnectionType type);

		#endregion

		#region S+ Events

		[PublicAPI("SPlus")]
		public SPlusGetSignalDetectedState GetSignalDetectedStateCallback { get; set; }

		[PublicAPI("SPlus")]
		public SPlusGetInputs GetInputsCallback { get; set; }

		[PublicAPI("SPlus")]
		public SPlusGetOutputs GetOutputsCallback { get; set; }

		[PublicAPI("SPlus")]
		public SPlusGetInputsForOutput GetInputsForOutputCallback { get; set; }

		[PublicAPI("SPlus")]
		public SPlusRoute RouteCallback { get; set; }

		[PublicAPI("SPlus")]
		public SPlusClearOutput ClearOutputCallback { get; set; }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public SPlusSwitcherControl(SPlusSwitcher parent, int id)
			: base(parent, id)
		{
		}

		#region Methods

		/// <summary>
		/// Called to inform the system of a source detection change.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		[PublicAPI("SPlus")]
		public void UpdateSourceDetection(int input, eConnectionType type)
		{
			bool state = GetSignalDetectedState(input, type);
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(input, type, state));
		}

		/// <summary>
		/// Called to inform the system of a switcher routing change.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		[PublicAPI("SPlus")]
		public void UpdateSwitcherOutput(int output, eConnectionType type)
		{
			// Raise the route change event.
			OnRouteChange.Raise(this, new RouteChangeEventArgs(output, type));

			// Raise the active transmission changed event for the output.
			bool state = GetActiveTransmissionState(output, type);
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(output, type, state));

			// Raise the active inputs change event for the input
			OnActiveInputsChanged.Raise(this);
		}

		#endregion

		#region Protected Methods

		protected override void DisposeFinal(bool disposing)
		{
			GetSignalDetectedStateCallback = null;
			GetInputsCallback = null;
			GetOutputsCallback = null;
			GetInputsForOutputCallback = null;
			RouteCallback = null;
			ClearOutputCallback = null;

			base.DisposeFinal(disposing);
		}

		#endregion

		#region IRouteSwitcherControl

		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

		public override bool Route(RouteOperation info)
		{
			return RouteCallback(info);
		}

		public override bool ClearOutput(int output, eConnectionType type)
		{
			return ClearOutputCallback(output, type);
		}

		#region IRouteMidpointControl

		public override IEnumerable<ConnectorInfo> GetInputs(int output, eConnectionType type)
		{
			return GetInputsForOutputCallback != null
				       ? GetInputsForOutputCallback(output, type)
				       : new ConnectorInfo[] {new ConnectorInfo(0, eConnectionType.Audio & eConnectionType.Video)};
		}

		#region IRouteSourceControl

		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			if (GetOutputsCallback != null)
				return GetOutputsCallback();

			ConnectorInfo[] fakeReturn = new ConnectorInfo[32];
			for (int i = 1; i < 32; i++)
			{
				fakeReturn[i - 1] = new ConnectorInfo(i, eConnectionType.Audio & eConnectionType.Video);
			}
			return fakeReturn;
		}

		#endregion

		#region IRouteDestinationControl

		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public override event EventHandler OnActiveInputsChanged;

		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			return GetSignalDetectedStateCallback(input, type);
		}

		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			if (GetInputsCallback != null)
				return GetInputsCallback();

			ConnectorInfo[] fakeReturn = new ConnectorInfo[32];
			for (int i = 1; i < 32; i++)
			{
				fakeReturn[i - 1] = new ConnectorInfo(i, eConnectionType.Audio & eConnectionType.Video);
			}
			return fakeReturn;
		}

		#endregion

		#endregion

		#endregion
	}
}
