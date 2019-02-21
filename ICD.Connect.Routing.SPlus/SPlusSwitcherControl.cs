using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.SPlus
{
	public sealed class SPlusSwitcherControl : AbstractRouteSwitcherControl<SPlusSwitcher>
	{
		/// <summary>
		/// Called when a route changes.
		/// </summary>
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		/// <summary>
		/// Raised when an input source status changes.
		/// </summary>
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;

		/// <summary>
		/// Raised when the device starts/stops actively using an input, e.g. unroutes an input.
		/// </summary>
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		private readonly SwitcherCache m_Cache;

		#region S+ Delegates

		public delegate bool SPlusGetSignalDetectedState(int input, eConnectionType type);

		public delegate IEnumerable<ConnectorInfo> SPlusGetInputs();

		public delegate IEnumerable<ConnectorInfo> SPlusGetOutputs();

		public delegate bool SPlusRoute(RouteOperation info);

		public delegate bool SPlusClearOutput(int output, eConnectionType type);

		#endregion

		#region S+ Events

		[PublicAPI("S+")]
		public SPlusGetSignalDetectedState GetSignalDetectedStateCallback { get; set; }

		[PublicAPI("S+")]
		public SPlusGetInputs GetInputsCallback { get; set; }

		[PublicAPI("S+")]
		public SPlusGetOutputs GetOutputsCallback { get; set; }

		[PublicAPI("S+")]
		public SPlusRoute RouteCallback { get; set; }

		[PublicAPI("S+")]
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
			m_Cache = new SwitcherCache();
			Subscribe(m_Cache);
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnRouteChange = null;
			OnActiveTransmissionStateChanged = null;
			OnSourceDetectionStateChange = null;
			OnActiveInputsChanged = null;

			GetSignalDetectedStateCallback = null;
			GetInputsCallback = null;
			GetOutputsCallback = null;
			RouteCallback = null;
			ClearOutputCallback = null;

			base.DisposeFinal(disposing);

			Unsubscribe(m_Cache);
		}

		#region Methods

		/// <summary>
		/// Called to inform the system of a source detection change.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		[PublicAPI("S+")]
		public void UpdateSourceDetection(int input, eConnectionType type)
		{
			bool state = GetSignalDetectedStateCallback != null && GetSignalDetectedStateCallback(input, type);
			m_Cache.SetSourceDetectedState(input, type, state);
		}

		/// <summary>
		/// Called to inform the system of a switcher routing change.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="input"></param>
		/// <param name="type"></param>
		[PublicAPI("S+")]
		public void SetInputForOutput(int output, int input, eConnectionType type)
		{
			m_Cache.SetInputForOutput(output, input, type);
		}

		#endregion

		#region IRouteSwitcherControl

		/// <summary>
		/// Performs the given route operation.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public override bool Route(RouteOperation info)
		{
			return RouteCallback != null && RouteCallback(info);
		}

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if successfully cleared.</returns>
		public override bool ClearOutput(int output, eConnectionType type)
		{
			return ClearOutputCallback != null && ClearOutputCallback(output, type);
		}

		public override IEnumerable<string> GetSwitcherVideoInputIds()
		{
			return GetInputs().Where(i => i.ConnectionType.HasFlag(eConnectionType.Video))
			                  .Select(i => string.Format("S+ Video Input {0}", i.Address));
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

				yield return "S+ Video Input Sync";
			}
		}

		/// <summary>
		/// Gets the Input Resolution for the switcher's inputs (ie 1920x1080, or empty for no sync)
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> GetSwitcherVideoInputResolutions()
		{
			foreach (var input in GetInputs().Where(i => i.ConnectionType.HasFlag(eConnectionType.Video)))
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

		/// <summary>
		/// Gets the Output Ids of the switcher's outputs (ie HDMI1, VGA2)
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> GetSwitcherVideoOutputIds()
		{
			return GetOutputs().Where(o => o.ConnectionType.HasFlag(eConnectionType.Video))
							   .Select(o => string.Format("S+ Video Output {0}", o.Address));
		}

		/// <summary>
		/// Gets the Output Name of the switcher's outputs (ie Content, Display In)
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> GetSwitcherVideoOutputNames()
		{
			return GetSwitcherVideoOutputIds();
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int input)
		{
			return GetInputs().First(c => c.Address == input);
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return GetInputs().Any(c => c.Address == input);
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			return GetInputsCallback == null
				       ? Enumerable.Empty<ConnectorInfo>()
				       : GetInputsCallback();
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int address)
		{
			return GetOutputs().First(c => c.Address == address);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return GetOutputs().Any(c => c.Address == output);
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			return GetOutputsCallback == null
				       ? Enumerable.Empty<ConnectorInfo>()
				       : GetOutputsCallback();
		}

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			return m_Cache.GetOutputsForInput(input, type);
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
			return m_Cache.GetInputConnectorInfoForOutput(output, type);
		}

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			return m_Cache.GetSourceDetectedState(input, type);
		}

		#endregion

		#region Cache Callbacks

		/// <summary>
		/// Subscribe to the cache events.
		/// </summary>
		/// <param name="cache"></param>
		private void Subscribe(SwitcherCache cache)
		{
			cache.OnRouteChange += CacheOnRouteChange;
			cache.OnActiveInputsChanged += CacheOnActiveInputsChanged;
			cache.OnSourceDetectionStateChange += CacheOnSourceDetectionStateChange;
			cache.OnActiveTransmissionStateChanged += CacheOnActiveTransmissionStateChanged;
		}

		/// <summary>
		/// Unsubscribe from the cache events.
		/// </summary>
		/// <param name="cache"></param>
		private void Unsubscribe(SwitcherCache cache)
		{
			cache.OnRouteChange -= CacheOnRouteChange;
			cache.OnActiveInputsChanged -= CacheOnActiveInputsChanged;
			cache.OnSourceDetectionStateChange -= CacheOnSourceDetectionStateChange;
			cache.OnActiveTransmissionStateChanged -= CacheOnActiveTransmissionStateChanged;
		}

		private void CacheOnRouteChange(object sender, RouteChangeEventArgs args)
		{
			OnRouteChange.Raise(this, new RouteChangeEventArgs(args));
		}

		private void CacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs args)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(args));
		}

		private void CacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs args)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(args));
		}

		private void CacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs args)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(args));
		}

		#endregion
	}
}
