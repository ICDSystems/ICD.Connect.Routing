#if SIMPLSHARP
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using ICD.Connect.Routing.CrestronPro.Utils;
using ICD.Common.Properties;
using ICD.Common.Services.Logging;
using ICD.Common.Utils;
using ICD.Connect.API.Nodes;
#else
using System;
#endif
using ICD.Connect.Devices;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4K302C
{
	public sealed class DmTx4K302CAdapter : AbstractEndpointTransmitterBaseAdapter<DmTx4k302C,DmTx4K302CAdapterSettings>
	{
#if SIMPLSHARP
        public delegate void TransmitterChangeCallback(DmTx4K302CAdapter sender, DmTx4k302C transmitter);

		/// <summary>
		/// Raised when the wrapped transmitter changes.
		/// </summary>
		public event TransmitterChangeCallback OnTransmitterChanged;

		private DmTx4k302C m_Transmitter;
#endif
		private int? m_ParentId;

        #region Properties

#if SIMPLSHARP
        /// <summary>
        /// Gets the wrapped transmitter.
        /// </summary>
        public DmTx4k302C Transmitter
		{
			get { return m_Transmitter; }
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
#endif

#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public DmTx4K302CAdapter()
		{
#if SIMPLSHARP
            Controls.Add(new DmTx4K302CSourceControl(this));
#endif
		}

#region Methods

		/// <summary>
		/// Release resources
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

#if SIMPLSHARP
            // Unsbscribe and unregister
            SetTransmitter(null, null);
#endif
		}

#if SIMPLSHARP
        /// <summary>
        /// Sets the wrapped transmitter.
        /// </summary>
        /// <param name="transmitter"></param>
        /// <param name="parentId"></param>
        [PublicAPI]
		public void SetTransmitter(DmTx4k302C transmitter, int? parentId)
		{
			Unsubscribe(Transmitter);

			if (Transmitter != null)
			{
				if (Transmitter.Registered)
					Transmitter.UnRegister();

				try
				{
					Transmitter.Dispose();
				}
				catch
				{
				}
			}

			m_ParentId = parentId;
			Transmitter = transmitter;

			if (Transmitter != null && !Transmitter.Registered)
			{
				if (Name != null)
					Transmitter.Description = Name;
				eDeviceRegistrationUnRegistrationResponse result = Transmitter.Register();
				if (result != eDeviceRegistrationUnRegistrationResponse.Success)
					Logger.AddEntry(eSeverity.Error, "Unable to register {0} - {1}", Transmitter.GetType().Name, result);
			}

			Subscribe(Transmitter);
			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public ComPort GetComPort(int address)
		{
			if (address == 1)
				return Transmitter.ComPorts[1];

			string message = string.Format("{0} has no {1} with address {2}", this, typeof(ComPort).Name, address);
			throw new KeyNotFoundException(message);
		}

		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public IROutputPort GetIrOutputPort(int address)
		{
			if (address == 1)
				return Transmitter.IROutputPorts[1];

			string message = string.Format("{0} has no {1} with address {2}", this, typeof(IROutputPort).Name, address);
			throw new KeyNotFoundException(message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public Relay GetRelayPort(int address)
		{
			string message = string.Format("{0} has no {1} with address {2}", this, typeof(Relay).Name, address);
			throw new KeyNotFoundException(message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public Versiport GetIoPort(int address)
		{
			string message = string.Format("{0} has no {1} with address {2}", this, typeof(Versiport).Name, address);
			throw new KeyNotFoundException(message);
		}
#endif

#endregion

#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(DmTx4K302CAdapterSettings settings)
		{
			base.CopySettingsFinal(settings);

#if SIMPLSHARP
            DMInput input = m_Transmitter == null ? null : m_Transmitter.DMInput;

			settings.Ipid = m_Transmitter == null ? (byte)0 : (byte)m_Transmitter.ID;
			settings.DmSwitch = m_ParentId;
			settings.DmInputAddress = input == null ? (int?)null : (int)input.Number;
#else
            settings.Ipid = 0;
            settings.DmSwitch = m_ParentId;
            settings.DmInputAddress = null;
#endif
        }

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
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
		protected override void ApplySettingsFinal(DmTx4K302CAdapterSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

#if SIMPLSHARP
            DmTx4k302C transmitter =
				DmEndpointFactoryUtils.InstantiateEndpoint<DmTx4k302C>(settings.Ipid, settings.DmInputAddress,
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
        protected override DmTx4k302C InstantiateTransmitter(byte ipid, CrestronControlSystem controlSystem)
        {
            return new DmTx4k302C(ipid, controlSystem);
		}

		protected override DmTx4k302C InstantiateTransmitter(byte ipid, DMInput input)
		{
			return new DmTx4k302C(ipid, input);
		}

		protected override DmTx4k302C InstantiateTransmitter(DMInput input)
		{
			return new DmTx4k302C(input);
		}
#endif

#endregion

#region Private Methods

#if SIMPLSHARP
        /// <summary>
        /// Subscribe to the transmitter events.
        /// </summary>
        /// <param name="transmitter"></param>
        private void Subscribe(DmTx4k302C transmitter)
		{
			if (transmitter == null)
				return;

			transmitter.OnlineStatusChange += TransmitterOnlineStatusChange;
		}

		/// <summary>
		/// Unsubscribes from the transmitter events.
		/// </summary>
		/// <param name="transmitter"></param>
		private void Unsubscribe(DmTx4k302C transmitter)
		{
			if (transmitter == null)
				return;

			transmitter.OnlineStatusChange -= TransmitterOnlineStatusChange;
		}

		/// <summary>
		/// Called when the device online status changes.
		/// </summary>
		/// <param name="currentDevice"></param>
		/// <param name="args"></param>
		private void TransmitterOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			UpdateCachedOnlineStatus();
		}
#endif

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
#if SIMPLSHARP
            return Transmitter != null && Transmitter.IsOnline;
#else
            return false;
#endif
        }

        #endregion

#region Console

#if SIMPLSHARP
        /// <summary>
        /// Calls the delegate for each console status item.
        /// </summary>
        /// <param name="addRow"></param>
        public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
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