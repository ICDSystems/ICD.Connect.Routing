using System;
using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.Controls
{
	public abstract class AbstractRouteSwitcherControl<T> : AbstractRouteMidpointControl<T>, IRouteSwitcherControl
		where T : IDeviceBase
	{
		private bool m_AudioBreakawayEnabled;
		private bool m_UsbBreakawayEnabled;

		public event EventHandler<BoolEventArgs> OnAudioBreakawayEnabledChanged;
		public event EventHandler<BoolEventArgs> OnUsbBreakawayEnabledChanged;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractRouteSwitcherControl(T parent, int id)
			: base(parent, id)
		{
		}

		#region Methods

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
		/// Performs the given route operation.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public abstract bool Route(RouteOperation info);

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if successfully cleared.</returns>
		public abstract bool ClearOutput(int output, eConnectionType type);

		/// <summary>
		/// Gets the Input Ids of the switcher's inputs (ie HDMI1, VGA2)
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<string> GetSwitcherVideoInputIds();

		/// <summary>
		/// Gets the Input Name of the switcher (ie Content, Display In)
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<string> GetSwitcherVideoInputNames();

		/// <summary>
		/// Gets the Input Sync Type of the switcher's inputs (ie HDMI when HDMI Sync is detected, empty when not detected)
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<string> GetSwitcherVideoInputSyncType();

		/// <summary>
		/// Gets the Input Resolution for the switcher's inputs (ie 1920x1080, or empty for no sync)
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<string> GetSwitcherVideoInputResolutions();

		/// <summary>
		/// Gets the Output Ids of the switcher's outputs (ie HDMI1, VGA2)
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<string> GetSwitcherVideoOutputIds();

		/// <summary>
		/// Gets the Output Name of the switcher's outputs (ie Content, Display In)
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<string> GetSwitcherVideoOutputNames();

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			RouteSwitcherControlConsole.BuildConsoleStatus(this, addRow);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			foreach (IConsoleCommand command in RouteSwitcherControlConsole.GetConsoleCommands(this))
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

			foreach (IConsoleNodeBase node in RouteSwitcherControlConsole.GetConsoleNodes(this))
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
