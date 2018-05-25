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
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
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

		public override bool Route(RouteOperation info)
		{
			return RouteCallback != null && RouteCallback(info);
		}

		public override bool ClearOutput(int output, eConnectionType type)
		{
			return ClearOutputCallback != null && ClearOutputCallback(output, type);
		}

		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			return GetInputsCallback == null
				       ? Enumerable.Empty<ConnectorInfo>()
				       : GetInputsCallback();
		}

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
