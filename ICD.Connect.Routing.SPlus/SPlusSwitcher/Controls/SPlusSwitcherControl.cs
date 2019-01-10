using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Nodes;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.SPlus.SPlusSwitcher.Controls
{
	public sealed class SPlusSwitcherControl : AbstractRouteSwitcherControl<Device.SPlusSwitcherDevice>
	{

		#region Fields

		private readonly SwitcherCache m_SwitcherCache;

		#endregion

		#region Properties

		public ushort InputCount { get; private set; }
		public ushort OutputCount { get; private set; }
		public eConnectionType SwitcherLayers { get; private set; }
		public bool SupportsSourceDetection { get; private set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		/// <param name="inputCount"></param>
		/// <param name="outputCount"></param>
		/// <param name="switcherLayers"></param>
		/// <param name="supportsSourceDetection"></param>
		public SPlusSwitcherControl(Device.SPlusSwitcherDevice parent, int id, ushort inputCount, ushort outputCount, eConnectionType switcherLayers, bool supportsSourceDetection) : base(parent, id)
		{
			if (inputCount < 1)
				throw new ArgumentOutOfRangeException("inputCount");

			if (outputCount < 1)
				throw new ArgumentOutOfRangeException("outputCount");

			InputCount = inputCount;
			OutputCount = outputCount;
			SwitcherLayers = switcherLayers;
			SupportsSourceDetection = supportsSourceDetection;

			m_SwitcherCache = new SwitcherCache();

			Subscribe(m_SwitcherCache);

			// Set Detect State if not supported
			if (!SupportsSourceDetection)
				SetAllInputsDetected();
		}

		#endregion

		#region IRouteSwitcherControl

		#region Events

		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		#endregion

		#region Public Methods

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
								.Select(t => GetSignalDetectedState(input, t))
								.Unanimous(false);
			}

			return m_SwitcherCache.GetSourceDetectedState(input, type);
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int input)
		{
			if (!ContainsInput(input))
				throw new ArgumentOutOfRangeException("input");

			return new ConnectorInfo(input, SwitcherLayers);
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return input >= 1 && input <= InputCount;
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			return Enumerable.Range(1, InputCount)
						 .Select(i => new ConnectorInfo(i, SwitcherLayers));
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int address)
		{
			if (!ContainsOutput(address))
				throw new ArgumentOutOfRangeException("address");

			return new ConnectorInfo(address, SwitcherLayers);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return output >= 1 && output <= OutputCount;
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			{
				return Enumerable.Range(1, OutputCount)
								 .Select(i => new ConnectorInfo(i, SwitcherLayers));
			}
		}

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			return m_SwitcherCache.GetOutputsForInput(input, type);
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
			return m_SwitcherCache.GetInputConnectorInfoForOutput(output, type);
		}

		/// <summary>
		/// Performs the given route operation.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public override bool Route(RouteOperation info)
		{
			eConnectionType type = info.ConnectionType;
			int input = info.LocalInput;
			int output = info.LocalOutput;

			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
								.Select(t => this.Route(input, output, t))
								.ToArray()
								.Unanimous(false);
			}

			if (!Parent.Route(output, input, type))
				return false;

			return m_SwitcherCache.SetInputForOutput(output, input, type);
		}

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if successfully cleared.</returns>
		public override bool ClearOutput(int output, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
								.Select(t => ClearOutput(output, t))
								.ToArray()
								.Unanimous(false);
			}

			if (Parent.ClearOutput(output, type))
				return false;

			return m_SwitcherCache.SetInputForOutput(output, null, type);
		}


		#endregion

		#endregion

		#region Internal Methods from Device

		/// <summary>
		/// From Shim, sets the signal detection state on a input/type
		/// </summary>
		/// <param name="input"></param>
		/// <param name="state"></param>
		internal void SetSignalDetectedState(int input, bool state)
		{
			if (!SupportsSourceDetection)
				return;
			if (ContainsInput(input))
				m_SwitcherCache.SetSourceDetectedState(input, SwitcherLayers, state);
		}

		internal void SetInputForOutput(int output , int? input , eConnectionType type)
		{
			if ((input == null || ContainsInput(input.Value)) && ContainsOutput(output))
				m_SwitcherCache.SetInputForOutput(output, input, type);
		}

		/// <summary>
		/// From Shim, clears the switcher cache, so it can be re-built from scratch.
		/// </summary>
		internal void ClearCache()
		{
			m_SwitcherCache.Clear();

			// If the switcher doesn't support source detection, set all sources as detected on clear
			if (!SupportsSourceDetection)
				SetAllInputsDetected();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			Unsubscribe(m_SwitcherCache);
			m_SwitcherCache.Clear();
		}

		/// <summary>
		/// Sets all inputs detected for all layers
		/// </summary>
		private void SetAllInputsDetected()
		{
			Log(eSeverity.Debug, "Switch doesn't support input detection, setting all inputs as detected");
			foreach (var layer in EnumUtils.GetFlagsExceptNone(SwitcherLayers))
				for (int input = 1; input <= InputCount; input++)
					m_SwitcherCache.SetSourceDetectedState(input, layer, true);
		}

		#region Switcher Cache Callbacks

		private void Subscribe(SwitcherCache switcherCache)
		{
			if (switcherCache == null)
				return;

			switcherCache.OnActiveInputsChanged += SwitcherCacheOnActiveInputsChanged;
			switcherCache.OnActiveTransmissionStateChanged += SwitcherCacheOnActiveTransmissionStateChanged;
			switcherCache.OnRouteChange += SwitcherCacheOnRouteChange;
			switcherCache.OnSourceDetectionStateChange += SwitcherCacheOnSourceDetectionStateChange;
		}

		private void Unsubscribe(SwitcherCache switcherCache)
		{
			if (switcherCache == null)
				return;

			switcherCache.OnActiveInputsChanged -= SwitcherCacheOnActiveInputsChanged;
			switcherCache.OnActiveTransmissionStateChanged -= SwitcherCacheOnActiveTransmissionStateChanged;
			switcherCache.OnRouteChange -= SwitcherCacheOnRouteChange;
			switcherCache.OnSourceDetectionStateChange -= SwitcherCacheOnSourceDetectionStateChange;
		}

		private void SwitcherCacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs args)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(args));
		}

		private void SwitcherCacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs args)
		{
			OnActiveTransmissionStateChanged(this, new TransmissionStateEventArgs(args));
		}

		private void SwitcherCacheOnRouteChange(object sender, RouteChangeEventArgs args)
		{
			OnRouteChange.Raise(this, new RouteChangeEventArgs(args));
		}

		private void SwitcherCacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs args)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(args));
		}

		#endregion

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Input Count", InputCount);
			addRow("Output Count", OutputCount);
			addRow("Supports Detect", SupportsSourceDetection);
		}

		#endregion

	}
}
