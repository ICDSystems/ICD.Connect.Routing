using ICD.Connect.Misc.CrestronPro.Utils;
using ICD.Connect.Settings;
#if !NETSTANDARD
using System;
using Crestron.SimplSharpPro;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Services.Logging;
#endif
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmXio.DmXioDirectorBase
{
#if !NETSTANDARD
	public abstract class AbstractDmXioDirectorBaseAdapter<TDirector, TSettings> : AbstractDevice<TSettings>, IDmXioDirectorBaseAdapter
		where TDirector : Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase
#else
	public abstract class AbstractDmXioDirectorBaseAdapter<TSettings> : AbstractDevice<TSettings>, IDmXioDirectorBaseAdapter
#endif
		where TSettings : IDmXioDirectorBaseAdapterSettings, new()
	{
#if !NETSTANDARD
		public event DmXioDirectorChangeCallback OnDirectorChanged;

		private TDirector m_Director;
#endif

#region Properties

#if !NETSTANDARD
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
#if !NETSTANDARD
			OnDirectorChanged = null;
#endif

			base.DisposeFinal(disposing);

#if !NETSTANDARD
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
#if !NETSTANDARD
			return m_Director != null && m_Director.IsOnline;
#else
			return false;
#endif
		}

#region Methods

#if !NETSTANDARD
		/// <summary>
		/// Sets the wrapped switcher.
		/// </summary>
		/// <param name="switcher"></param>
		[PublicAPI]
		public void SetDirector(TDirector switcher)
		{
			Unsubscribe(Director);

			if (Director != null)
				GenericBaseUtils.TearDown(Director);

			Director = switcher;

			eDeviceRegistrationUnRegistrationResponse result;
			if (Director != null && !GenericBaseUtils.SetUp(Director, this, out result))
				Logger.Log(eSeverity.Error, "Unable to register {0} - {1}", Director.GetType().Name, result);

			Subscribe(Director);

			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Gets the domain with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase.DmXioDomain GetDomain(uint id)
		{
			if (m_Director == null)
				throw new InvalidOperationException("Wrapped Director is null");

			return m_Director.Domain[id];
		}
#endif

#endregion

#region Director Callbacks

#if !NETSTANDARD
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

#if !NETSTANDARD
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

#if !NETSTANDARD
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
#if !NETSTANDARD

			SetDirector(settings);
#endif
		}

#if !NETSTANDARD
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

#if !NETSTANDARD
			addRow("Ethernet ID", Director == null ? null : StringUtils.ToIpIdString((byte)Director.ID));
#endif
		}

#endregion
	}
}
