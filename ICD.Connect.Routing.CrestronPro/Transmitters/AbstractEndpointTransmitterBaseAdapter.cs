﻿using ICD.Connect.Routing.CrestronPro.Cards;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro.Utils.Extensions;
using ICD.Connect.Routing.CrestronPro.Utils;
#else
using System;
#endif
using ICD.Common.Properties;
using ICD.Common.Services.Logging;
using ICD.Common.Utils;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.Transmitters
{
	/// <summary>
	/// Base class for EndpointTransmitterBase device adapters.
	/// </summary>
	/// <typeparam name="TTransmitter"></typeparam>
	/// <typeparam name="TSettings"></typeparam>
#if SIMPLSHARP
	public abstract class AbstractEndpointTransmitterBaseAdapter<TTransmitter, TSettings> : AbstractDevice<TSettings>, IEndpointTransmitterBaseAdapter
		where TTransmitter : Crestron.SimplSharpPro.DM.Endpoints.Transmitters.EndpointTransmitterBase
#else
    public abstract class AbstractEndpointTransmitterBaseAdapter<TSettings> : AbstractDevice<TSettings>, IEndpointTransmitterBaseAdapter
#endif
		where TSettings : IEndpointTransmitterBaseAdapterSettings, new()
	{
#if SIMPLSHARP
		/// <summary>
		/// Raised when the wrapped transmitter changes.
		/// </summary>
		public event TransmitterChangeCallback OnTransmitterChanged;

		private TTransmitter m_Transmitter;
#endif
		private int? m_ParentId;

		#region Properties

#if SIMPLSHARP
		/// <summary>
		/// Gets the transmitter.
		/// </summary>
		public TTransmitter Transmitter
		{
		    get
		    {
                Logger.AddEntry(eSeverity.Notice, typeof(TTransmitter).ToString() + ": " + "Get Public Member");
		        return m_Transmitter;
		    }
			private set
			{
                if (value == m_Transmitter)
					return;

				m_Transmitter = value;

				TransmitterChangeCallback handler = OnTransmitterChanged;
				if (handler != null)
					handler(this, m_Transmitter);
			}
		}

		/// <summary>
		/// Gets the wrapped transmitter instance.
		/// </summary>
		Crestron.SimplSharpPro.DM.Endpoints.Transmitters.EndpointTransmitterBase IEndpointTransmitterBaseAdapter.Transmitter
		{
		    get
		    {
		        Logger.AddEntry(eSeverity.Notice, typeof(TTransmitter).ToString() + ": " +  "Get Wrapped Instance");
                return Transmitter;
		    }
		}
#endif

		#endregion

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
            Logger.AddEntry(eSeverity.Notice, typeof(TTransmitter).ToString() + ": " +  "Dispose Final");
			base.DisposeFinal(disposing);

#if SIMPLSHARP
			// Unsubscribe and unregister.
			SetTransmitter(null, null);
#endif
		}

#if SIMPLSHARP
		/// <summary>
		/// Sets the wrapped transmitter.
		/// </summary>
		/// <param name="transmitter"></param>
		/// <param name="parentId">The id of the parent DM Switch device.</param>
		[PublicAPI]
		public void SetTransmitter(TTransmitter transmitter, int? parentId)
		{
            Logger.AddEntry(eSeverity.Notice, typeof(TTransmitter).ToString() + ": " +  "Set Transmitter to " + (transmitter == null ? "null":"actual"));
			Unsubscribe(Transmitter);
			Unregister(Transmitter);

			m_ParentId = parentId;
			Transmitter = transmitter;

			Register(Transmitter);
			Subscribe(Transmitter);

			ConfigureTransmitter(Transmitter);

			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Called when the wrapped transmitter is assigned.
		/// </summary>
		/// <param name="transmitter"></param>
		protected virtual void ConfigureTransmitter(TTransmitter transmitter)
		{
            Logger.AddEntry(eSeverity.Notice, typeof(TTransmitter).ToString() + ": " +  "ConfigureTransmitter");
		}

		/// <summary>
		/// Unregisters the given transmitter.
		/// </summary>
		/// <param name="transmitter"></param>
		private void Unregister(TTransmitter transmitter)
		{
            Logger.AddEntry(eSeverity.Notice, typeof(TTransmitter).ToString() + ": " +  "Unregister Transmitter");
			if (transmitter == null || !transmitter.Registered)
				return;

			transmitter.UnRegister();

			try
			{
				transmitter.Dispose();
			}
			catch
			{
			}
		}

		/// <summary>
		/// Registers the given transmitter and re-registers the DM parent.
		/// </summary>
		/// <param name="transmitter"></param>
		private void Register(TTransmitter transmitter)
		{
            Logger.AddEntry(eSeverity.Notice, typeof(TTransmitter).ToString() + ": " +  "Begin Register");
			if (transmitter == null || transmitter.Registered)
				return;

			if (Name != null)
				transmitter.Description = Name;

			eDeviceRegistrationUnRegistrationResponse result = transmitter.Register();
			if (result != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Logger.AddEntry(eSeverity.Error, "Unable to register {0} - {1}", transmitter.GetType().Name, result);
				return;
			}

			GenericDevice parent = transmitter.Parent as GenericDevice;
			if (parent == null)
				return;

			eDeviceRegistrationUnRegistrationResponse parentResult = parent.ReRegister();
			if (parentResult != eDeviceRegistrationUnRegistrationResponse.Success)
				Logger.AddEntry(eSeverity.Error, "Unable to register parent {0} - {1}", parent.GetType().Name, parentResult);
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
            Logger.AddEntry(eSeverity.Notice, typeof(TTransmitter).ToString() + ": " +  "Copy Settings Final");
			base.CopySettingsFinal(settings);

#if SIMPLSHARP
			DMInput input = Transmitter == null ? null : Transmitter.DMInput;

			settings.Ipid = Transmitter == null ? (byte?)null : (byte)Transmitter.ID;
			settings.DmSwitch = m_ParentId;
			settings.DmInputAddress = input == null ? (int?)null : (int)input.Number;
#else
            settings.Ipid = null;
            settings.DmSwitch = m_ParentId;
            settings.DmInputAddress = null;
#endif
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
            Logger.AddEntry(eSeverity.Notice, typeof(TTransmitter).ToString() + ": " +  "ClearSettingsFinal");
			base.ClearSettingsFinal();

#if SIMPLSHARP
			SetTransmitter(null, null);
#endif
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
            Logger.AddEntry(eSeverity.Notice, typeof(TTransmitter).ToString() + ": " +  "Begin Apply Settings Final For TX");
		    factory.LoadOriginators<ICardAdapter>();
			base.ApplySettingsFinal(settings, factory);

#if SIMPLSHARP
            TTransmitter transmitter =
				DmEndpointFactoryUtils.InstantiateEndpoint<TTransmitter>(settings.Ipid, settings.DmInputAddress,
																		 settings.DmSwitch, factory,
																		 InstantiateTransmitter,
																		 InstantiateTransmitter,
																		 InstantiateTransmitter);
			SetTransmitter(transmitter, settings.DmSwitch);
#else
            throw new NotImplementedException();
#endif
		}

#if SIMPLSHARP
		protected abstract TTransmitter InstantiateTransmitter(byte ipid, CrestronControlSystem controlSystem);

		protected abstract TTransmitter InstantiateTransmitter(byte ipid, DMInput input);

		protected abstract TTransmitter InstantiateTransmitter(DMInput input);
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
            Logger.AddEntry(eSeverity.Notice, typeof(TTransmitter).ToString() + ": " + "get online status");
			return Transmitter != null && Transmitter.IsOnline;
#else
            return false;
#endif
		}

		#endregion

		#region Transmitter callbacks

#if SIMPLSHARP
		/// <summary>
		/// Subscribes to the transmitter events.
		/// </summary>
		/// <param name="transmitter"></param>
		private void Subscribe(TTransmitter transmitter)
		{
            Logger.AddEntry(eSeverity.Notice, typeof(TTransmitter).ToString() + ": " + "subscribe callback");
			if (transmitter == null)
				return;

			transmitter.OnlineStatusChange += TransmitterOnlineStatusChange;
		}

		/// <summary>
		/// Unsubscribes from the transmitter events.
		/// </summary>
		/// <param name="transmitter"></param>
		private void Unsubscribe(TTransmitter transmitter)
		{
            Logger.AddEntry(eSeverity.Notice, typeof(TTransmitter).ToString() + ": " +  "unsub callback");
			if (transmitter == null)
				return;

			transmitter.OnlineStatusChange -= TransmitterOnlineStatusChange;
		}

		/// <summary>
		/// Called when the device goes online/offline.
		/// </summary>
		/// <param name="currentDevice"></param>
		/// <param name="args"></param>
		private void TransmitterOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
            Logger.AddEntry(eSeverity.Notice, typeof(TTransmitter).ToString() + ": " +  "online status change callback");
			UpdateCachedOnlineStatus();
		}
#endif

		#endregion

		#region Console

#if SIMPLSHARP
		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
            Logger.AddEntry(eSeverity.Notice, typeof(TTransmitter).ToString() + ": " +  "Build console Status");
			base.BuildConsoleStatus(addRow);

			addRow("IPID", m_Transmitter == null ? null : StringUtils.ToIpIdString((byte)m_Transmitter.ID));
			addRow("DM Switch", m_ParentId);

			DMInput input = m_Transmitter == null ? null : m_Transmitter.DMInput;
			addRow("DM Input", input == null ? null : input.Number.ToString());
		}
#endif

		#endregion
	}
}