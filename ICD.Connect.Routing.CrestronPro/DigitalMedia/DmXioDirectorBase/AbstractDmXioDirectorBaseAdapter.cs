#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmXioDirectorBase
{
#if SIMPLSHARP
	public abstract class AbstractDmXioDirectorBaseAdapter<TDirector, TSettings> : AbstractDevice<TSettings>, IDmXioDirectorBaseAdapter
		where TDirector : Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase
#else
	public abstract class AbstractDmXioDirectorBaseAdapter<TSettings> : AbstractDevice<TSettings>, IDmXioDirectorBaseAdapter
#endif
		where TSettings : IDmXioDirectorBaseAdapterSettings, new()
	{
#if SIMPLSHARP
		public event DmXioDirectorChangeCallback OnDirectorChanged;

		private TDirector m_Director;
#endif

#region Properties

#if SIMPLSHARP
		/// <summary>
		/// Gets the wrapped director.
		/// </summary>
		public TDirector Director
		{
			get { return m_Director; }
			private set
			{
				if (value == m_Director)
					return;

				m_Director = value;

				DmXioDirectorChangeCallback handler = OnDirectorChanged;
				if (handler != null)
					handler(this, m_Director);
			}
		}

		/// <summary>
		/// Gets the wrapped director instance.
		/// </summary>
		Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase IDmXioDirectorBaseAdapter.Director { get { return Director; } }
#endif

#endregion

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
#if SIMPLSHARP
			OnDirectorChanged = null;
#endif

			base.DisposeFinal(disposing);

#if SIMPLSHARP
			// Unsubscribe and unregister.
			SetDirector(null);
#endif
		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
#if SIMPLSHARP
			return m_Director != null && m_Director.IsOnline;
#else
			return false;
#endif
		}

#region Methods

#if SIMPLSHARP
		/// <summary>
		/// Sets the wrapped switcher.
		/// </summary>
		/// <param name="switcher"></param>
		[PublicAPI]
		protected void SetDirector(TDirector switcher)
		{
			Unsubscribe(Director);
			Unregister(Director);

			Director = switcher;

			Register(Director);
			Subscribe(Director);

			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Unregisters the given director.
		/// </summary>
		/// <param name="director"></param>
		private void Unregister(TDirector director)
		{
			if (director == null || !director.Registered)
				return;

			director.UnRegister();

			try
			{
				director.Dispose();
			}
			catch
			{
			}
		}

		/// <summary>
		/// Registers the given director.
		/// </summary>
		/// <param name="director"></param>
		private void Register(TDirector director)
		{
			if (director == null || director.Registered)
				return;

			if (Name != null)
				director.Description = Name;

			eDeviceRegistrationUnRegistrationResponse result = director.Register();
			if (result != eDeviceRegistrationUnRegistrationResponse.Success)
				Log(eSeverity.Error, "Unable to register {0} - {1}", director.GetType().Name, result);
		}
#endif

#endregion

#region Director Callbacks

#if SIMPLSHARP
		/// <summary>
		/// Subscribe to the director events.
		/// </summary>
		/// <param name="director"></param>
		private void Subscribe(TDirector director)
		{
			if (director == null)
				return;

			director.OnlineStatusChange += DirectorOnlineStatusChange;
		}

		/// <summary>
		/// Unsubscribe from the director events.
		/// </summary>
		/// <param name="director"></param>
		private void Unsubscribe(TDirector director)
		{
			if (director == null)
				return;

			director.OnlineStatusChange -= DirectorOnlineStatusChange;
		}

		/// <summary>
		/// Called when the device online status changes.
		/// </summary>
		/// <param name="genericBase"></param>
		/// <param name="args"></param>
		private void DirectorOnlineStatusChange(GenericBase genericBase, OnlineOfflineEventArgs args)
		{
			UpdateCachedOnlineStatus();
		}
#endif

#endregion

#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

#if SIMPLSHARP
			settings.EthernetId = Director == null ? (byte)0 : (byte)Director.ID;
#else
			settings.EthernetId = 0;
#endif
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

#if SIMPLSHARP
			SetDirector(null);
#endif
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);
#if SIMPLSHARP

			SetDirector(settings);
#endif
		}

#if SIMPLSHARP
		/// <summary>
		/// Override to control how the director is assigned from settings.
		/// </summary>
		/// <param name="settings"></param>
		protected virtual void SetDirector(TSettings settings)
		{
			TDirector director = InstantiateDirector(settings);
			SetDirector(director);
		}

		/// <summary>
		/// Creates a new instance of the wrapped internal director.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected abstract TDirector InstantiateDirector(TSettings settings);
#endif

#endregion

#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

#if SIMPLSHARP
			addRow("Ethernet ID", Director == null ? null : StringUtils.ToIpIdString((byte)Director.ID));
#endif
		}

#endregion
	}
}
