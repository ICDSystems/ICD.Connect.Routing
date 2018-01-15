using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.Mock.Destination
{
	public sealed class MockRouteDestinationControl : AbstractRouteDestinationControl<IDevice>
	{
		/// <summary>
		/// Called when an input source status changes.
		/// </summary>
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;

		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		private readonly Dictionary<int, ConnectorInfo> m_Inputs;
		private readonly SwitcherCache m_Cache;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public MockRouteDestinationControl(IDevice parent, int id) :
			base(parent, id)
		{
			m_Cache = new SwitcherCache();
			m_Cache.OnActiveInputsChanged += CacheOnActiveInputsChanged;
			m_Cache.OnSourceDetectionStateChange += CacheOnSourceDetectionStateChange;

			m_Inputs = new Dictionary<int, ConnectorInfo>();
		}

		#region Methods

		/// <summary>
		/// Returns true if video is detected at the given input.
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

			return m_Cache.GetSourceDetectedState(input, type);
		}

		/// <summary>
		/// Returns the true if the input is actively being used by the source device.
		/// For example, a display might true if the input is currently on screen,
		/// while a switcher may return true if the input is currently routed.
		/// </summary>
		public override bool GetInputActiveState(int input, eConnectionType type)
		{
			return true;
		}

		/// <summary>
		/// Sets the inputs.
		/// </summary>
		/// <param name="inputs"></param>
		public void SetInputs(IEnumerable<ConnectorInfo> inputs)
		{
			m_Inputs.Clear();
			m_Inputs.AddRange(inputs, info => info.Address);
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			return
				ServiceProvider.GetService<IRoutingGraph>()
				               .Connections.GetConnections()
				               .Where(c => c.Destination.Device == Parent.Id && c.Destination.Control == Id)
				               .Select(c => new ConnectorInfo(c.Destination.Address, c.ConnectionType));
		}

		/// <summary>
		/// Sets the video detected state at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <param name="state"></param>
		[PublicAPI]
		public void SetSignalDetectedState(int input, eConnectionType type, bool state)
		{
			m_Cache.SetSourceDetectedState(input, type, state);
		}

		/// <summary>
		/// Simulates a source change.
		/// </summary>
		[PublicAPI]
		public void RaiseOnSourceChange(int input, eConnectionType type)
		{
			bool detected = GetSignalDetectedState(input, type);
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(input, type, detected));
		}

		/// <summary>
		/// Raises the OnActiveInputsChanged event.
		/// </summary>
		[PublicAPI]
		public void RaiseOnActiveInputsChanged(int input, eConnectionType type, bool active)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(input, type, active));
		}

		#endregion

		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new GenericConsoleCommand<int, eConnectionType>(
				"AddInput",
				"Adds an input to the device",
				(a, b) => AddInput(a, b));

			yield return new GenericConsoleCommand<int, eConnectionType, bool>(
				"SetSignalDetectedState",
				"<input> <connectionType> <true/false>",
				(a, b, c) => SetSignalDetectedState(a, b, c));
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		private void AddInput(int address, eConnectionType type)
		{
			SetInputs(GetInputs().Append(new ConnectorInfo(address, type)));
		}

		#region Cache Callbacks

		private void CacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs args)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(args.Input, args.Type, args.State));
		}

		private void CacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs args)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(args.Input, args.Type, args.Active));
		}

		#endregion
	}
}
