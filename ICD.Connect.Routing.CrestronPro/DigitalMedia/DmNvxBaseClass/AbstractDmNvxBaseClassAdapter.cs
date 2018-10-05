#if SIMPLSHARP
using Crestron.SimplSharpPro.DM.Streaming;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.Dm100xStrBase;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvxBaseClass
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
		/// Configures the current device mode.
		/// </summary>
		/// <param name="deviceMode"></param>
		public void SetDeviceMode(eDeviceMode deviceMode)
		{
			m_DeviceMode = deviceMode;

#if SIMPLSHARP
			DmNvxControl nvxControl = Switcher == null ? null : Switcher.Control;
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
	}
}
