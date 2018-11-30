using System;
using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Audio.Console;
using ICD.Connect.Audio.Controls;
using ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume
{
	public sealed class Dmps3CrosspointVolumeControl : AbstractVolumeLevelDeviceControl<ControlSystemDevice>, IVolumeMuteFeedbackDeviceControl
	{
		public event EventHandler<BoolEventArgs> OnMuteStateChanged;

		private readonly string m_Name;
		private readonly IDmps3Crosspoint m_Crosspoint;

		#region Properties

		public override string Name
		{
			get { return m_Name; }
		}

		public override float VolumeLevel
		{
			get { return m_Crosspoint.VolumeLevel / 10.0f; }
		}

		protected override float VolumeRawMinAbsolute
		{
			get { return m_Crosspoint.VolumeRawMinAbsolute / 10.0f; }
		}

		protected override float VolumeRawMaxAbsolute
		{
			get { return m_Crosspoint.VolumeRawMaxAbsolute / 10.0f; }
		}

		public bool VolumeIsMuted { get { return m_Crosspoint.VolumeIsMuted; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <param name="crosspoint"></param>
		public Dmps3CrosspointVolumeControl(ControlSystemDevice parent, int id, string name, IDmps3Crosspoint crosspoint)
			: base(parent, id)
		{
			m_Name = name;
			m_Crosspoint = crosspoint;

			Subscribe(m_Crosspoint);
		}

		protected override void DisposeFinal(bool disposing)
		{
			OnMuteStateChanged = null;

			base.DisposeFinal(disposing);

			Unsubscribe(m_Crosspoint);
		}

		#region Methods

		public override void SetVolumeLevel(float volume)
		{
			m_Crosspoint.SetVolumeLevel((short)(volume * 10));
		}

		public void VolumeMuteToggle()
		{
			m_Crosspoint.VolumeMuteToggle();
		}

		public void SetVolumeMute(bool mute)
		{
			m_Crosspoint.SetVolumeMute(mute);
		}

		#endregion

		#region Crosspoint Callbacks

		private void Subscribe(IDmps3Crosspoint crosspoint)
		{
			crosspoint.OnVolumeLevelChanged += CrosspointOnVolumeLevelChanged;
			crosspoint.OnMuteStateChanged += CrosspointOnMuteStateChanged;
		}

		private void Unsubscribe(IDmps3Crosspoint crosspoint)
		{
			crosspoint.OnVolumeLevelChanged -= CrosspointOnVolumeLevelChanged;
			crosspoint.OnMuteStateChanged -= CrosspointOnMuteStateChanged;
		}

		private void CrosspointOnVolumeLevelChanged(object sender, GenericEventArgs<short> e)
		{
			VolumeFeedback(VolumeLevel);
		}

		private void CrosspointOnMuteStateChanged(object sender, BoolEventArgs e)
		{
			Log(eSeverity.Informational, "Mute changed: Mute={0}", e.Data);
			OnMuteStateChanged.Raise(this, new BoolEventArgs(e.Data));
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

			VolumeMuteFeedbackDeviceControlConsole.BuildConsoleStatus(this, addRow);
			VolumeMuteDeviceControlConsole.BuildConsoleStatus(this, addRow);
			VolumeMuteBasicDeviceControlConsole.BuildConsoleStatus(this, addRow);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			foreach (IConsoleCommand command in VolumeMuteFeedbackDeviceControlConsole.GetConsoleCommands(this))
				yield return command;

			foreach (IConsoleCommand command in VolumeMuteDeviceControlConsole.GetConsoleCommands(this))
				yield return command;

			foreach (IConsoleCommand command in VolumeMuteBasicDeviceControlConsole.GetConsoleCommands(this))
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
			foreach (IConsoleNodeBase command in GetBaseConsoleNodes())
				yield return command;

			foreach (IConsoleNodeBase command in VolumeMuteFeedbackDeviceControlConsole.GetConsoleNodes(this))
				yield return command;

			foreach (IConsoleNodeBase command in VolumeMuteDeviceControlConsole.GetConsoleNodes(this))
				yield return command;

			foreach (IConsoleNodeBase command in VolumeMuteBasicDeviceControlConsole.GetConsoleNodes(this))
				yield return command;
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