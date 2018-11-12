using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM.Streaming;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.Dm100xStrBase;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxBaseClass
{
#if SIMPLSHARP
	public abstract class AbstractDmNvxBaseClassAdapter<TSwitcher, TSettings> :
		AbstractDm100XStrBaseAdapter<TSwitcher, TSettings>, IDmNvxBaseClassAdapter
		where TSwitcher : Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass
#else
	public abstract class AbstractDmNvxBaseClassAdapter<TSettings> : AbstractDm100XStrBaseAdapter<TSettings>, IDmNvxBaseClassAdapter
#endif
		where TSettings : IDmNvxBaseClassAdapterSettings, new()
	{
		private eDeviceMode m_DeviceMode;

		/// <summary>
		/// Gets the configured device mode (i.e. Transmit or Receive)
		/// </summary>
		public eDeviceMode DeviceMode { get { return m_DeviceMode; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractDmNvxBaseClassAdapter()
		{
#if SIMPLSHARP
			Controls.Add(new DmNvxBaseClassSwitcherControl(this, 0));
			Controls.Add(new DmNvxBaseClassVolumeControl(this, 1));
#endif
		}

		/// <summary>
		/// Configures the current device mode.
		/// </summary>
		/// <param name="deviceMode"></param>
		public void SetDeviceMode(eDeviceMode deviceMode)
		{
			m_DeviceMode = deviceMode;

#if SIMPLSHARP
			DmNvxControl nvxControl = Streamer == null ? null : Streamer.Control;
			if (nvxControl != null)
				nvxControl.DeviceMode = m_DeviceMode.ToCrestron();
#endif
		}

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			SetDeviceMode(eDeviceMode.Receiver);
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.DeviceMode = m_DeviceMode;
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			SetDeviceMode(settings.DeviceMode);
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

			addRow("DeviceMode", DeviceMode);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			string deviceModeHelp = string.Format("SetDeviceMode <{0}>", StringUtils.ArrayFormat(EnumUtils.GetValues<eDeviceMode>()));

			yield return new GenericConsoleCommand<eDeviceMode>("SetDeviceMode", deviceModeHelp, m => SetDeviceMode(m));
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}
