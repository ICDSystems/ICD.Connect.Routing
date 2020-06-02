using System;
using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.EventArguments;
using ICD.Connect.Protocol.Extensions;
using ICD.Connect.Protocol.Ports.IoPort;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Devices.IoToggleSwitcher
{
	/// <summary>
	/// IoToggleSwitcherDevice is a 1 input to 2 switcher that is switched by digital output state.
	/// Low = Output 1
	/// High = Output 2
	/// </summary>
	public sealed class IoToggleSwitcherDevice : AbstractRouteSwitcherDevice<IoToggleSwitcherDeviceSettings>
	{
		private const int INPUT_ADDRESS = 1;
		private const int OUTPUT_1_ADDRESS = 1;
		private const int OUTPUT_2_ADDRESS = 2;

		private const bool OUTPUT_1_ACTIVE = false;
		private const bool OUTPUT_2_ACTIVE = true;

		private const eConnectionType CONNECTION_TYPE =
			eConnectionType.Audio |
			eConnectionType.Video |
			eConnectionType.Usb;

		/// <summary>
		/// Raised when an input source status changes.
		/// </summary>
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;

		/// <summary>
		/// Raised when the device starts/stops actively using an input, e.g. unroutes an input.
		/// </summary>
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		/// <summary>
		/// Called when a route changes.
		/// </summary>
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		private readonly SwitcherCache m_Cache;

		private IIoPort m_Port;

		#region Properties

		/// <summary>
		/// Gets/sets the IO port for the switcher.
		/// </summary>
		public IIoPort Port
		{
			get { return m_Port; }
			set
			{
				if (value == m_Port)
					return;

				Unsubscribe(m_Port);
				m_Port = value;
				Subscribe(m_Port);

				UpdateCachedOnlineStatus();
				UpdateActiveOutput();
			}
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public IoToggleSwitcherDevice()
		{
			m_Cache = new SwitcherCache();
			m_Cache.OnActiveInputsChanged += CacheOnActiveInputsChanged;
			m_Cache.OnActiveTransmissionStateChanged += CacheOnActiveTransmissionStateChanged;
			m_Cache.OnRouteChange += CacheOnRouteChange;
			m_Cache.OnSourceDetectionStateChange += CacheOnSourceDetectionStateChange;
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
			if (!ContainsInput(input))
				throw new ArgumentOutOfRangeException("input");

			if (type == eConnectionType.None)
				throw new ArgumentOutOfRangeException("type");

			return true;
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

			return new ConnectorInfo(input, CONNECTION_TYPE);
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return input == INPUT_ADDRESS;
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			yield return GetInput(INPUT_ADDRESS);
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int output)
		{
			if (!ContainsOutput(output))
				throw new ArgumentOutOfRangeException("output");

			return new ConnectorInfo(output, CONNECTION_TYPE);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return output == OUTPUT_1_ADDRESS || output == OUTPUT_2_ADDRESS;
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			yield return GetOutput(OUTPUT_1_ADDRESS);
			yield return GetOutput(OUTPUT_2_ADDRESS);
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
			if (!ContainsOutput(output))
				throw new ArgumentOutOfRangeException("output");

			if (type == eConnectionType.None)
				throw new ArgumentOutOfRangeException("type");

			return m_Cache.GetInputConnectorInfoForOutput(output, type);
		}

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			if (!ContainsOutput(input))
				throw new ArgumentOutOfRangeException("input");

			if (type == eConnectionType.None)
				throw new ArgumentOutOfRangeException("type");

			return m_Cache.GetOutputsForInput(input, type);
		}

		/// <summary>
		/// Performs the given route operation.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public override bool Route(RouteOperation info)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			int input = info.LocalInput;
			if (!ContainsInput(input))
				throw new ArgumentException("No input with address " + input);

			int output = info.LocalOutput;
			return Route(output);
		}

		/// <summary>
		/// Routes the input to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public bool Route(int output)
		{
			bool digitalOut;

			switch (output)
			{
				case OUTPUT_1_ADDRESS:
					digitalOut = OUTPUT_1_ACTIVE;
					break;

				case OUTPUT_2_ADDRESS:
					digitalOut = OUTPUT_2_ACTIVE;
					break;

				default:
					throw new ArgumentOutOfRangeException("output");
			}

			m_Port.SetDigitalOut(digitalOut);
			return true;
		}

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if successfully cleared.</returns>
		public override bool ClearOutput(int output, eConnectionType type)
		{
			// Clearing output doesn't really make sense
			return false;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return Port != null && Port.IsOnline;
		}

		/// <summary>
		/// Updates the active input to match the IO port state.
		/// </summary>
		private void UpdateActiveOutput()
		{
			bool? state = Port == null ? (bool?)null : Port.DigitalOut;

			m_Cache.SetInputForOutput(OUTPUT_1_ADDRESS, state == OUTPUT_1_ACTIVE ? INPUT_ADDRESS : (int?)null, CONNECTION_TYPE);
			m_Cache.SetInputForOutput(OUTPUT_2_ADDRESS, state == OUTPUT_2_ACTIVE ? INPUT_ADDRESS : (int?)null, CONNECTION_TYPE);
		}

		#endregion

		#region Port Callbacks

		/// <summary>
		/// Subscribe to the port events.
		/// </summary>
		/// <param name="port"></param>
		private void Subscribe(IIoPort port)
		{
			if (port == null)
				return;

			port.OnIsOnlineStateChanged += PortOnIsOnlineStateChanged;
			port.OnDigitalOutChanged += PortOnDigitalOutChanged;
		}

		/// <summary>
		/// Unsubscribe from the port events.
		/// </summary>
		/// <param name="port"></param>
		private void Unsubscribe(IIoPort port)
		{
			if (port == null)
				return;

			port.OnIsOnlineStateChanged -= PortOnIsOnlineStateChanged;
			port.OnDigitalOutChanged -= PortOnDigitalOutChanged;
		}

		/// <summary>
		/// Called when the port online state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void PortOnIsOnlineStateChanged(object sender, DeviceBaseOnlineStateApiEventArgs eventArgs)
		{
			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Called when the port digital output state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="boolEventArgs"></param>
		private void PortOnDigitalOutChanged(object sender, BoolEventArgs boolEventArgs)
		{
			UpdateActiveOutput();
		}

		#endregion

		#region Cache Callbacks

		private void CacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs eventArgs)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(eventArgs));
		}

		private void CacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs eventArgs)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(eventArgs));
		}

		private void CacheOnRouteChange(object sender, RouteChangeEventArgs eventArgs)
		{
			OnRouteChange.Raise(this, new RouteChangeEventArgs(eventArgs));
		}

		private void CacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs eventArgs)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(eventArgs));
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(IoToggleSwitcherDeviceSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			IIoPort port = null;

			if (settings.Port != null)
			{
				try
				{
					port = factory.GetPortById((int)settings.Port) as IIoPort;
				}
				catch (KeyNotFoundException)
				{
					Logger.Log(eSeverity.Error, "No IO port with id {0}", settings.Port);
				}
			}

			Port = port;
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			Port = null;
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(IoToggleSwitcherDeviceSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Port = Port == null ? (int?)null : Port.Id;
		}

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(IoToggleSwitcherDeviceSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new RouteSwitcherControl(this, 0));
		}

		#endregion
	}
}
