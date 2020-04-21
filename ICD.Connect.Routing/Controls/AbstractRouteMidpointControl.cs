using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Controls
{
	public abstract class AbstractRouteMidpointControl<T> : AbstractRouteDestinationControl<T>, IRouteMidpointControl
		where T : IDevice
	{
		public event EventHandler<BoolEventArgs> OnAudioBreakawayEnabledChanged;
		public event EventHandler<BoolEventArgs> OnUsbBreakawayEnabledChanged;

		/// <summary>
		/// Raised when a route changes.
		/// </summary>
		public abstract event EventHandler<RouteChangeEventArgs> OnRouteChange;

		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		public abstract event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		private bool m_AudioBreakawayEnabled;
		private bool m_UsbBreakawayEnabled;

		/// <summary>
		/// Describes whether a switcher is breaking away audio.
		/// </summary>
		public bool AudioBreakawayEnabled
		{
			get { return m_AudioBreakawayEnabled; }
			protected set
			{
				if (m_AudioBreakawayEnabled == value)
					return;

				m_AudioBreakawayEnabled = value;

				OnAudioBreakawayEnabledChanged.Raise(this, new BoolEventArgs(m_AudioBreakawayEnabled));
			}
		}

		/// <summary>
		/// Describes whether a switcher is breaking away audio.
		/// </summary>
		public bool UsbBreakawayEnabled
		{
			get { return m_UsbBreakawayEnabled; }
			protected set
			{
				if (m_UsbBreakawayEnabled == value)
					return;

				m_UsbBreakawayEnabled = value;

				OnUsbBreakawayEnabledChanged.Raise(this, new BoolEventArgs(m_UsbBreakawayEnabled));
			}
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractRouteMidpointControl(T parent, int id)
			: base(parent, id)
		{
		}

		#region Methods

		/// <summary>
		/// Returns true if the device is actively transmitting on the given output.
		/// This is NOT the same as sending video, since some devices may send an
		/// idle signal by default.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public virtual bool GetActiveTransmissionState(int output, eConnectionType type)
		{
			// Returns true if the output is transmitting an input on all flags
			return EnumUtils.GetFlagsExceptNone(type).All(flag => this.GetInputs(output, flag).Any());
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public abstract ConnectorInfo GetOutput(int address);

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public abstract bool ContainsOutput(int output);

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<ConnectorInfo> GetOutputs();

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public abstract IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type);

		/// <summary>
		/// Gets the input routed to the given output matching the given type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Type has multiple flags.</exception>
		public abstract ConnectorInfo? GetInput(int output, eConnectionType type);

		/// <summary>
		/// Returns the true if the input is actively being used by the source device.
		/// For example, a display might true if the input is currently on screen,
		/// while a switcher may return true if the input is currently routed.
		/// </summary>
		public override bool GetInputActiveState(int input, eConnectionType type)
		{
			return GetOutputs(input, type).Any();
		}

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			RouteSourceControlConsole.BuildConsoleStatus(this, addRow);
			RouteMidpointControlConsole.BuildConsoleStatus(this, addRow);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			foreach (IConsoleCommand command in RouteSourceControlConsole.GetConsoleCommands(this))
				yield return command;

			foreach (IConsoleCommand command in RouteMidpointControlConsole.GetConsoleCommands(this))
				yield return command;
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (IConsoleNodeBase node in GetBaseConsoleNodes())
				yield return node;

			foreach (IConsoleNodeBase node in RouteSourceControlConsole.GetConsoleNodes(this))
				yield return node;

			foreach (IConsoleNodeBase node in RouteMidpointControlConsole.GetConsoleNodes(this))
				yield return node;
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}

		#endregion
	}
}
