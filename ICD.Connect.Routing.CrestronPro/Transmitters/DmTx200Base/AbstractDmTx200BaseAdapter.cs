using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Common.Properties;
using ICD.Common.Services.Logging;
using ICD.Common.Utils;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Misc.CrestronPro.Utils;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base
{
	/// <summary>
	/// Base class for DmTx200 device adapters.
	/// </summary>
	/// <typeparam name="TTransmitter"></typeparam>
	/// <typeparam name="TSettings"></typeparam>
	public abstract class AbstractDmTx200BaseAdapter<TTransmitter, TSettings> : AbstractDevice<TSettings>
		where TTransmitter : Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base
		where TSettings : AbstractDmTx200BaseAdapterSettings, new()
	{
		public delegate void TransmitterChangeCallback(object sender, TTransmitter transmitter);

		/// <summary>
		/// Raised when the wrapped transmitter changes.
		/// </summary>
		public event TransmitterChangeCallback OnTransmitterChanged;

		private int? m_ParentId;
		private TTransmitter m_Transmitter;

		#region Properties

		/// <summary>
		/// Gets the transmitter.
		/// </summary>
		public TTransmitter Transmitter
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

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			// Unsubscribe and unregister.
			SetTransmitter(null, null);
		}

		/// <summary>
		/// Sets the wrapped transmitter.
		/// </summary>
		/// <param name="transmitter"></param>
		/// <param name="parentId">The id of the parent DM Switch device.</param>
		[PublicAPI]
		public void SetTransmitter(TTransmitter transmitter, int? parentId)
		{
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

			Unsubscribe(Transmitter);
			Transmitter = transmitter;
			Subscribe(Transmitter);

			if (Transmitter != null && !Transmitter.Registered)
			{
				if (Name != null)
					Transmitter.Description = Name;
				eDeviceRegistrationUnRegistrationResponse result = Transmitter.Register();
				if (result != eDeviceRegistrationUnRegistrationResponse.Success)
					Logger.AddEntry(eSeverity.Error, "Unable to register {0} - {1}", Transmitter.GetType().Name, result);

				Transmitter.VideoSource = Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Auto;
			}

			UpdateCachedOnlineStatus();
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			DMInput input = Transmitter == null ? null : Transmitter.DMInput;

			settings.Ipid = Transmitter == null ? (byte?)null : (byte)Transmitter.ID;
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
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			TTransmitter transmitter =
				DmEndpointFactoryUtils.InstantiateEndpoint<TTransmitter>(settings.Ipid, settings.DmInputAddress,
				                                                         settings.DmSwitch, factory,
				                                                         InstantiateTransmitter,
				                                                         InstantiateTransmitter,
				                                                         InstantiateTransmitter);

			SetTransmitter(transmitter, settings.DmSwitch);
		}

		protected abstract TTransmitter InstantiateTransmitter(byte ipid, CrestronControlSystem controlSystem);

		protected abstract TTransmitter InstantiateTransmitter(byte ipid, DMInput input);

		protected abstract TTransmitter InstantiateTransmitter(DMInput input);

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return Transmitter != null && Transmitter.IsOnline;
		}

		#endregion

		#region Transmitter callbacks

		/// <summary>
		/// Subscribes to the transmitter events.
		/// </summary>
		/// <param name="transmitter"></param>
		private void Subscribe(TTransmitter transmitter)
		{
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
			UpdateCachedOnlineStatus();
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

			addRow("IPID", m_Transmitter == null ? null : StringUtils.ToIpIdString((byte)m_Transmitter.ID));
			addRow("DM Switch", m_ParentId);

			DMInput input = m_Transmitter == null ? null : m_Transmitter.DMInput;
			addRow("DM Input", input == null ? null : input.Number.ToString());
		}

		#endregion
	}
}
