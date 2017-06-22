using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using ICD.Common.Properties;
using ICD.Common.Services.Logging;
using ICD.Connect.Devices;
using ICD.Connect.Misc.CrestronPro.Utils;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4K302C
{
	public sealed class DmTx4K302CAdapter : AbstractDevice<DmTx4K302CAdapterSettings>
	{
		public delegate void TransmitterChangeCallback(
			DmTx4K302CAdapter sender, DmTx4k302C transmitter);

		/// <summary>
		/// Raised when the wrapped transmitter changes.
		/// </summary>
		public event TransmitterChangeCallback OnTransmitterChanged;

		private DmTx4k302C m_Transmitter;
		private int? m_ParentId;

		#region Properties

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

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public DmTx4K302CAdapter()
		{
			Controls.Add(new DmTx4K302CRouteControl(this));
		}

		#region Methods

		/// <summary>
		/// Release resources
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			// Unsbscribe and unregister
			SetTransmitter(null, null);
		}

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

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(DmTx4K302CAdapterSettings settings)
		{
			base.CopySettingsFinal(settings);

			DMInput input = m_Transmitter == null ? null : m_Transmitter.DMInput;

			settings.Ipid = m_Transmitter == null ? (byte)0 : (byte)m_Transmitter.ID;
			settings.DmSwitch = m_ParentId;
			settings.DmInputAddress = input == null ? (int?)null : (int)input.Number;
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			SetTransmitter(null, null);
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(DmTx4K302CAdapterSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			DmTx4k302C transmitter =
				DmEndpointFactoryUtils.InstantiateEndpoint<DmTx4k302C>(settings.Ipid, settings.DmInputAddress,
				                                                         settings.DmSwitch, factory,
				                                                         InstantiateTransmitter,
				                                                         InstantiateTransmitter,
				                                                         InstantiateTransmitter);

			SetTransmitter(transmitter, settings.DmSwitch);
		}

		private static DmTx4k302C InstantiateTransmitter(byte ipid, CrestronControlSystem controlSystem)
		{
			return new DmTx4k302C(ipid, controlSystem);
		}

		private static DmTx4k302C InstantiateTransmitter(byte ipid, DMInput input)
		{
			return new DmTx4k302C(ipid, input);
		}

		private static DmTx4k302C InstantiateTransmitter(DMInput input)
		{
			return new DmTx4k302C(input);
		}

		#endregion

		#region Private Methods

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

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return Transmitter != null && Transmitter.IsOnline;
		}

		#endregion
	}
}