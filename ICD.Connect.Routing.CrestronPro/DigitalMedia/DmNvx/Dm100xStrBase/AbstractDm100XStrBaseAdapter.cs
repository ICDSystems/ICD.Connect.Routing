using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Devices;
using ICD.Connect.Routing.CrestronPro.Utils;
using ICD.Connect.Settings.Core;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.Dm100xStrBase
{
#if SIMPLSHARP
	public abstract class AbstractDm100XStrBaseAdapter<TStreamer, TSettings> : AbstractDevice<TSettings>,
	                                                                           IDm100XStrBaseAdapter<TStreamer>
		where TStreamer : Crestron.SimplSharpPro.DM.Streaming.Dm100xStrBase
#else
	public abstract class AbstractDm100XStrBaseAdapter<TSettings> : AbstractDevice<TSettings>, IDm100XStrBaseAdapter
#endif
		where TSettings : IDm100XStrBaseAdapterSettings, new()
	{
#if SIMPLSHARP
		/// <summary>
		/// Raised when the wrapped streamer instance changes.
		/// </summary>
		public event Dm100XStrBaseChangeCallback OnStreamerChanged;

		private TStreamer m_Streamer;
#endif

		#region Properties

#if SIMPLSHARP
		/// <summary>
		/// Gets the wrapped streamer.
		/// </summary>
		public TStreamer Streamer
		{
			get { return m_Streamer; }
			private set
			{
				if (value == m_Streamer)
					return;

				m_Streamer = value;

				Dm100XStrBaseChangeCallback handler = OnStreamerChanged;
				if (handler != null)
					handler(this, m_Streamer);
			}
		}

		Crestron.SimplSharpPro.DM.Streaming.Dm100xStrBase IDm100XStrBaseAdapter.Streamer { get { return Streamer; } }
#endif

		#endregion

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
#if SIMPLSHARP
			OnStreamerChanged = null;
#endif

			base.DisposeFinal(disposing);

#if SIMPLSHARP
			// Unsubscribe and unregister.
			SetStreamer(null);
#endif
		}

#if SIMPLSHARP
		/// <summary>
		/// Sets the wrapped streamer.
		/// </summary>
		/// <param name="streamer"></param>
		[PublicAPI]
		public void SetStreamer(TStreamer streamer)
		{
			Unsubscribe(Streamer);

			if (Streamer != null)
			{
				if (Streamer.Registered)
					Streamer.UnRegister();

				try
				{
					Streamer.Dispose();
				}
				catch
				{
				}
			}

			Streamer = streamer;

			if (Streamer != null && !Streamer.Registered)
			{
				if (Name != null)
					Streamer.Description = Name;

				eDeviceRegistrationUnRegistrationResponse result = Streamer.Register();
				if (result != eDeviceRegistrationUnRegistrationResponse.Success)
					Log(eSeverity.Error, "Unable to register {0} - {1}", Streamer.GetType().Name, result);
			}

			Subscribe(Streamer);

			UpdateCachedOnlineStatus();
		}

#endif

		#endregion

		#region Ports

#if SIMPLSHARP
		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual ComPort GetComPort(int address)
		{
			if (Streamer == null)
				throw new InvalidOperationException("No switcher instantiated");

			return Streamer.ComPorts[(uint)address];
		}

		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual IROutputPort GetIrOutputPort(int address)
		{
			if (Streamer == null)
				throw new InvalidOperationException("No switcher instantiated");

			return Streamer.IROutputPorts[(uint)address];
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual Relay GetRelayPort(int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(Relay).Name);
			throw new NotSupportedException(message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual Versiport GetIoPort(int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(Versiport).Name);
			throw new NotSupportedException(message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public DigitalInput GetDigitalInputPort(int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(DigitalInput).Name);
			throw new NotSupportedException(message);
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
			settings.EthernetId = Streamer == null ? (byte)0 : (byte)Streamer.ID;
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
			SetStreamer(null);
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
			TStreamer streamer = DmEndpointFactoryUtils.InstantiateStreamer(settings, factory, this);
			SetStreamer(streamer);
#else
            throw new NotSupportedException();
#endif
		}

#if SIMPLSHARP
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		public abstract TStreamer InstantiateStreamer(uint ethernetId, CrestronControlSystem controlSystem);

		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="endpointId"></param>
		/// <param name="domain"></param>
		/// <param name="isReceiver"></param>
		/// <returns></returns>
		public abstract TStreamer InstantiateStreamer(uint endpointId,
		                                              Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase.DmXioDomain domain,
		                                              bool isReceiver);
#endif

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
#if SIMPLSHARP
			return Streamer != null && Streamer.IsOnline;
#else
            return false;
#endif
		}

#if SIMPLSHARP
		/// <summary>
		/// Subscribe to the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		private void Subscribe(TStreamer switcher)
		{
			if (switcher == null)
				return;

			switcher.OnlineStatusChange += SwitcherOnlineStatusChange;
		}

		/// <summary>
		/// Unsubscribe from the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		private void Unsubscribe(TStreamer switcher)
		{
			if (switcher == null)
				return;

			switcher.OnlineStatusChange -= SwitcherOnlineStatusChange;
		}

		/// <summary>
		/// Called when the device online status changes.
		/// </summary>
		/// <param name="currentDevice"></param>
		/// <param name="args"></param>
		private void SwitcherOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			UpdateCachedOnlineStatus();
		}
#endif

		#endregion
	}
}
