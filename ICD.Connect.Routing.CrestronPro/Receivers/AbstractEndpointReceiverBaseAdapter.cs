using System;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Devices.Extensions;
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.CrestronPro.Cards;
using ICD.Connect.Routing.Devices;
using ICD.Connect.Settings.Core;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Misc.CrestronPro.Utils.Extensions;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.API.Nodes;
#endif

namespace ICD.Connect.Routing.CrestronPro.Receivers
{
	/// <summary>
	/// EndpointReceiverBaseAdapter wraps a EndpointReceiverBase to provide a routing device.
	/// </summary>
#if SIMPLSHARP
	public abstract class AbstractEndpointReceiverBaseAdapter<TReceiver, TSettings> : AbstractRouteMidpointDevice<TSettings>,
	                                                                                  IPortParent,
	                                                                                  IEndpointReceiverBaseAdapter
		                                                                                  <TReceiver>
		where TReceiver : EndpointReceiverBase
#else
    public abstract class AbstractEndpointReceiverBaseAdapter<TSettings> : AbstractRouteMidpointDevice<TSettings>
#endif
		where TSettings : IEndpointReceiverBaseAdapterSettings, new()
	{
#if SIMPLSHARP
		/// <summary>
		/// Raised when the wrapped scaler changes.
		/// </summary>
		public event ReceiverChangeCallback OnReceiverChanged;

		private TReceiver m_Receiver;
#endif
		private int? m_ParentId;

		#region Properties

#if SIMPLSHARP
		/// <summary>
		/// Gets the wrapped scaler.
		/// </summary>
		public TReceiver Receiver
		{
			get { return m_Receiver; }
			private set
			{
				if (value == m_Receiver)
					return;

				m_Receiver = value;

				ReceiverChangeCallback handler = OnReceiverChanged;
				if (handler != null)
					handler(this, m_Receiver);
			}
		}

		EndpointReceiverBase IEndpointReceiverBaseAdapter.Receiver { get { return Receiver; } }
#endif

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractEndpointReceiverBaseAdapter()
		{
			Controls.Add(new RouteMidpointControl(this, 0));
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
			SetScaler(null, null);
#endif
		}

#if SIMPLSHARP
		/// <summary>
		/// Sets the wrapped scaler.
		/// </summary>
		/// <param name="scaler"></param>
		/// <param name="parentId"></param>
		[PublicAPI]
		public void SetScaler(TReceiver scaler, int? parentId)
		{
			Unsubscribe(Receiver);
			Unregister(Receiver);

			m_ParentId = parentId;
			Receiver = scaler;

			Register(Receiver);
			Subscribe(Receiver);

			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Unregisters the given scaler.
		/// </summary>
		/// <param name="scaler"></param>
		private void Unregister(TReceiver scaler)
		{
			if (scaler == null || !scaler.Registered)
				return;

			scaler.UnRegister();

			try
			{
				scaler.Dispose();
			}
			catch
			{
			}
		}

		/// <summary>
		/// Registers the given scaler and re-registers the DM parent.
		/// </summary>
		/// <param name="scaler"></param>
		private void Register(TReceiver scaler)
		{
			if (scaler == null || scaler.Registered)
				return;

			if (Name != null)
				scaler.Description = Name;

			eDeviceRegistrationUnRegistrationResponse result = scaler.Register();
			if (result != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Log(eSeverity.Error, "Unable to register {0} - {1}", scaler.GetType().Name, result);
				return;
			}

			GenericDevice parent = scaler.Parent as GenericDevice;
			if (parent == null)
				return;

			eDeviceRegistrationUnRegistrationResponse parentResult = parent.ReRegister();
			if (parentResult != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Log(eSeverity.Error, "Unable to register parent {0} - {1}", parent.GetType().Name, parentResult);
			}
		}

#endif

		#endregion

		#region IO

		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual ComPort GetComPort(int address)
		{
			string message = string.Format("{0} has no {1} with address {2}", this, typeof(ComPort).Name, address);
			throw new IndexOutOfRangeException(message);
		}

		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual IROutputPort GetIrOutputPort(int address)
		{
			string message = string.Format("{0} has no {1} with address {2}", this, typeof(IROutputPort).Name, address);
			throw new IndexOutOfRangeException(message);
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
			DMOutput input = m_Receiver == null ? null : m_Receiver.DMOutput;

			settings.Ipid = m_Receiver == null ? (byte)0 : (byte)m_Receiver.ID;
			settings.DmSwitch = m_ParentId;
			settings.DmOutputAddress = input == null ? (int?)null : (int)input.Number;
#else
            settings.Ipid = 0;
            settings.DmSwitch = m_ParentId;
            settings.DmOutputAddress = null;
#endif
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

#if SIMPLSHARP
			SetScaler(null, null);
#endif
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			factory.LoadOriginators<ICardAdapter>();

			base.ApplySettingsFinal(settings, factory);

#if SIMPLSHARP
			TReceiver scaler = null;

			try
			{
				scaler = InstantiateEndpoint(settings, factory);
			}
			catch (Exception e)
			{
				Log(eSeverity.Error, "Failed to instantiate internal {0} - {1}", typeof(TReceiver).Name, e.Message);
			}

			SetScaler(scaler, settings.DmSwitch);
#else
            throw new NotImplementedException();
#endif
		}

#if SIMPLSHARP
		/// <summary>
		/// Determines the best way to instantiate a DMEndpoint based on the available information.
		/// Instantiates via parent DM Switch if specified, otherwise uses the ControlSystem.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <returns></returns>
		[NotNull]
		private TReceiver InstantiateEndpoint(TSettings settings, IDeviceFactory factory)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");

			if (factory == null)
				throw new ArgumentNullException("factory");

			if (settings.DmSwitch == null)
			{
				if (settings.Ipid == null)
					throw new InvalidOperationException("Can't instantiate ControlSystem endpoint without IPID");
				return InstantiateReceiver((byte)settings.Ipid, ProgramInfo.ControlSystem);
			}

			if (settings.DmOutputAddress == null)
				throw new InvalidOperationException("Can't instantiate DM endpoint without DM address");

			IDmParent provider = factory.GetDeviceById((int)settings.DmSwitch) as IDmParent;
			if (provider == null)
				throw new InvalidOperationException(string.Format("Device {0} is not a {1}", settings.DmSwitch,
				                                                  typeof(IDmParent).Name));

			DMOutput output = provider.GetDmOutput((int)settings.DmOutputAddress);

			return settings.Ipid == null
				       ? InstantiateReceiver(output)
				       : InstantiateReceiver((byte)settings.Ipid, output);
		}

		public abstract TReceiver InstantiateReceiver(byte ipid, CrestronControlSystem controlSystem);

		public abstract TReceiver InstantiateReceiver(byte ipid, DMOutput output);

		public abstract TReceiver InstantiateReceiver(DMOutput output);
#endif

		#endregion

		#region Scaler Callbacks

#if SIMPLSHARP
		/// <summary>
		/// Subscribe to the scaler events.
		/// </summary>
		/// <param name="scaler"></param>
		private void Subscribe(EndpointReceiverBase scaler)
		{
			if (scaler == null)
				return;

			scaler.OnlineStatusChange += ScalerOnlineStatusChange;
		}

		/// <summary>
		/// Unsubscribes from the scaler events.
		/// </summary>
		/// <param name="scaler"></param>
		private void Unsubscribe(EndpointReceiverBase scaler)
		{
			if (scaler == null)
				return;

			scaler.OnlineStatusChange -= ScalerOnlineStatusChange;
		}

		/// <summary>
		/// Called when the device online status changes.
		/// </summary>
		/// <param name="currentDevice"></param>
		/// <param name="args"></param>
		private void ScalerOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
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
			return Receiver != null && Receiver.IsOnline;
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

			addRow("IPID", m_Receiver == null ? null : StringUtils.ToIpIdString((byte)m_Receiver.ID));
			addRow("DM Switch", m_ParentId);

			DMOutput output = m_Receiver == null ? null : m_Receiver.DMOutput;
			addRow("DM Output", output == null ? null : output.Number.ToString());
		}
#endif

		#endregion
	}
}
