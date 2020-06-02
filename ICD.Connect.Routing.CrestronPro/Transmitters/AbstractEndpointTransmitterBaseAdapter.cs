using System;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Misc.CrestronPro.Utils;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.CrestronPro.Cards;
using ICD.Connect.Settings;
using ICD.Connect.Routing.Devices;
using ICD.Connect.Routing.EventArguments;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Nodes;
using ICD.Connect.Routing.CrestronPro.Utils;
#endif

namespace ICD.Connect.Routing.CrestronPro.Transmitters
{
	/// <summary>
	/// Base class for EndpointTransmitterBase device adapters.
	/// </summary>
	/// <typeparam name="TSettings"></typeparam>
#if SIMPLSHARP
	/// <typeparam name="TTransmitter"></typeparam>
	public abstract class AbstractEndpointTransmitterBaseAdapter<TTransmitter, TSettings> : AbstractRouteSourceDevice<TSettings>,
	                                                                                        IEndpointTransmitterBaseAdapter<TTransmitter>
		where TTransmitter : Crestron.SimplSharpPro.DM.Endpoints.Transmitters.EndpointTransmitterBase
#else
	public abstract class AbstractEndpointTransmitterBaseAdapter<TSettings> : AbstractRouteSourceDevice<TSettings>, IEndpointTransmitterBaseAdapter
#endif
		where TSettings : IEndpointTransmitterBaseAdapterSettings, new()
	{
		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

#if SIMPLSHARP
		/// <summary>
		/// Raised when the wrapped transmitter changes.
		/// </summary>
		public event TransmitterChangeCallback OnTransmitterChanged;

		private TTransmitter m_Transmitter;
		private int? m_ParentId;
#endif

		#region Properties

#if SIMPLSHARP
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
		
		/// <summary>
		/// Gets the wrapped transmitter instance.
		/// </summary>
		Crestron.SimplSharpPro.DM.Endpoints.Transmitters.EndpointTransmitterBase IEndpointTransmitterBaseAdapter.Transmitter
		{
			get { return Transmitter; }
		}

		/// <summary>
		/// Gets the wrapped DMEndpointBase device.
		/// </summary>
		DMEndpointBase IDmEndpoint.Device { get { return Transmitter; } }
#endif

		#endregion

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
#if SIMPLSHARP
			OnTransmitterChanged = null;
#endif

			base.DisposeFinal(disposing);

#if SIMPLSHARP
			// Unsubscribe and unregister.
			SetTransmitter(null, null);
#endif
		}

		#region Methods

#if SIMPLSHARP
		/// <summary>
		/// Sets the wrapped transmitter.
		/// </summary>
		/// <param name="transmitter"></param>
		/// <param name="parentId">The id of the parent DM Switch device.</param>
		[PublicAPI]
		public void SetTransmitter(TTransmitter transmitter, int? parentId)
		{
			Unsubscribe(Transmitter);

			if (Transmitter != null)
				GenericBaseUtils.TearDown(Transmitter);

			m_ParentId = parentId;
			Transmitter = transmitter;

			eDeviceRegistrationUnRegistrationResponse result;
			if (Transmitter != null && !GenericBaseUtils.SetUp(Transmitter, this, out result))
				Logger.Log(eSeverity.Error, "Unable to register {0} - {1}", Transmitter.GetType().Name, result);

			Subscribe(Transmitter);

			ConfigureTransmitter(Transmitter);

			UpdateCachedOnlineStatus();
		}
#endif

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int output)
		{
			if (output != 1)
				throw new ArgumentOutOfRangeException("output");

			return new ConnectorInfo(output, eConnectionType.Audio | eConnectionType.Video);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return output == 1;
		}

		#endregion

		#region Private Methods 

#if SIMPLSHARP
		/// <summary>
		/// Called when the wrapped transmitter is assigned.
		/// </summary>
		/// <param name="transmitter"></param>
		protected virtual void ConfigureTransmitter(TTransmitter transmitter)
		{
		}
#endif

		#endregion

		#region IO

#if SIMPLSHARP
		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual ComPort GetComPort(int address)
		{
			string message = string.Format("{0} has no {1} with address {2}", this, typeof(ComPort).Name, address);
			throw new ArgumentOutOfRangeException("address", message);
		}

		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual IROutputPort GetIrOutputPort(int address)
		{
			string message = string.Format("{0} has no {1} with address {2}", this, typeof(IROutputPort).Name, address);
			throw new ArgumentOutOfRangeException("address", message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual Relay GetRelayPort(int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(Relay).Name);
			throw new ArgumentOutOfRangeException("address", message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual Versiport GetIoPort(int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(Versiport).Name);
			throw new ArgumentOutOfRangeException("address", message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public DigitalInput GetDigitalInputPort(int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(DigitalInput).Name);
			throw new ArgumentOutOfRangeException("address", message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="io"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual Cec GetCecPort(eInputOuptut io, int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(Cec).Name);
			throw new ArgumentOutOfRangeException("address", message);
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
			DMInput input = Transmitter == null ? null : Transmitter.DMInput;

			settings.Ipid = Transmitter == null ? (byte?)null : (byte)Transmitter.ID;
			settings.DmSwitch = m_ParentId;
			settings.DmInputAddress = input == null ? (int?)null : (int)input.Number;
#else
            settings.Ipid = null;
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
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			factory.LoadOriginators<ICardAdapter>();

			base.ApplySettingsFinal(settings, factory);

#if SIMPLSHARP
			TTransmitter transmitter = null;

			try
			{
				transmitter = DmEndpointFactoryUtils.InstantiateTransmitter(settings, factory, this);
			}
			catch (Exception e)
			{
				Logger.Log(eSeverity.Error, "Failed to instantiate internal {0} - {1}", typeof(TTransmitter).Name, e.Message);
			}

			SetTransmitter(transmitter, settings.DmSwitch);
#endif
		}

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(TSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new RouteSourceControl(this, 0));
		}

#if SIMPLSHARP
		/// <summary>
		/// Instantiates the transmitter with the given IPID against the control system.
		/// </summary>
		/// <param name="ipid"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		public abstract TTransmitter InstantiateTransmitter(byte ipid, CrestronControlSystem controlSystem);

		/// <summary>
		/// Instantiates the transmitter against the given DM Input and configures it with the given IPID.
		/// </summary>
		/// <param name="ipid"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public abstract TTransmitter InstantiateTransmitter(byte ipid, DMInput input);

		/// <summary>
		/// Instantiates the transmitter against the given DM Input.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public abstract TTransmitter InstantiateTransmitter(DMInput input);
#endif

		#endregion

		#region Protected / Private Methods

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

		#region Transmitter callbacks

#if SIMPLSHARP
		/// <summary>
		/// Subscribes to the transmitter events.
		/// </summary>
		/// <param name="transmitter"></param>
		protected virtual void Subscribe(TTransmitter transmitter)
		{
			if (transmitter == null)
				return;

			transmitter.OnlineStatusChange += TransmitterOnlineStatusChange;
		}

		/// <summary>
		/// Unsubscribes from the transmitter events.
		/// </summary>
		/// <param name="transmitter"></param>
		protected virtual void Unsubscribe(TTransmitter transmitter)
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
#endif

		/// <summary>
		/// Raises the OnActiveTransmissionStateChanged event.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <param name="transmitting"></param>
		protected void RaiseOnActiveTransmissionStateChanged(int output, eConnectionType type, bool transmitting)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(output, type, transmitting));
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
