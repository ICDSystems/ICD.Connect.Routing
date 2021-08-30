using System;
using ICD.Connect.Devices.Controls;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Misc.CrestronPro.Utils;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.CrestronPro.Cards;
using ICD.Connect.Settings;
using ICD.Connect.Routing.CrestronPro.Utils;
using ICD.Connect.Routing.Devices;
using ICD.Connect.Routing.EventArguments;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.API.Nodes;
using ICD.Common.Utils.Services.Logging;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
#endif

namespace ICD.Connect.Routing.CrestronPro.Receivers
{
	/// <summary>
	/// EndpointReceiverBaseAdapter wraps a EndpointReceiverBase to provide a routing device.
	/// </summary>
#if !NETSTANDARD
	public abstract class AbstractEndpointReceiverBaseAdapter<TReceiver, TSettings> : AbstractRouteMidpointDevice<TSettings>,
	                                                                                  IEndpointReceiverBaseAdapter
		                                                                                  <TReceiver>
		where TReceiver : EndpointReceiverBase
#else
    public abstract class AbstractEndpointReceiverBaseAdapter<TSettings> : AbstractRouteMidpointDevice<TSettings>
#endif
		where TSettings : IEndpointReceiverBaseAdapterSettings, new()
	{
		/// <summary>
		/// Called when a route changes.
		/// </summary>
		public override event EventHandler<RouteChangeEventArgs> OnRouteChange;

#if !NETSTANDARD
		/// <summary>
		/// Raised when the wrapped scaler changes.
		/// </summary>
		public event ReceiverChangeCallback OnReceiverChanged;

		private TReceiver m_Receiver;

		private int? m_ParentId;
#endif

		#region Properties

#if !NETSTANDARD
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

		/// <summary>
		/// Gets the wrapped DMEndpointBase device.
		/// </summary>
		public DMEndpointBase Device { get { return Receiver; } }
#endif

		#endregion

		#region Methods

		/// <summary>
		/// Release resources
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnRouteChange = null;
#if !NETSTANDARD
			OnReceiverChanged = null;
#endif

			base.DisposeFinal(disposing);

#if !NETSTANDARD
			// Unsbscribe and unregister
			SetScaler(null, null);
#endif
		}

#if !NETSTANDARD
		/// <summary>
		/// Sets the wrapped scaler.
		/// </summary>
		/// <param name="scaler"></param>
		/// <param name="parentId"></param>
		[PublicAPI]
		public void SetScaler(TReceiver scaler, int? parentId)
		{
			Unsubscribe(Receiver);

			if (Receiver != null)
				GenericBaseUtils.TearDown(Receiver);

			m_ParentId = parentId;
			Receiver = scaler;

			eDeviceRegistrationUnRegistrationResponse result;
			if (Receiver != null && !GenericBaseUtils.SetUp(Receiver, this, out result))
				Logger.Log(eSeverity.Error, "Unable to register {0} - {1}", Receiver.GetType().Name, result);

			Subscribe(Receiver);

			UpdateCachedOnlineStatus();
		}
#endif

		protected void RaiseRouteChange(int? oldInput, int? newInput, int output, eConnectionType type)
		{
			OnRouteChange.Raise(this, new RouteChangeEventArgs(oldInput, newInput, output, type));
		}

		#endregion

		#region IO

#if !NETSTANDARD
		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public ComPort GetComPort(int address)
		{
			IComPorts comPort = Receiver as IComPorts;

			if (comPort != null && address >= 1 && address <= comPort.NumberOfComPorts)
				return comPort.ComPorts[(uint)address];

			string message = string.Format("{0} has no {1} with address {2}", this, typeof(ComPort).Name, address);
			throw new ArgumentOutOfRangeException("address", message);
		}

		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public IROutputPort GetIrOutputPort(int address)
		{
			IIROutputPorts irPort = Receiver as IIROutputPorts;

			if (irPort != null && address >= 1 && address <= irPort.NumberOfIROutputPorts)
				return irPort.IROutputPorts[(uint)address];

			string message = string.Format("{0} has no {1} with address {2}", this, typeof(IROutputPort).Name, address);
			throw new ArgumentOutOfRangeException("address", message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public Relay GetRelayPort(int address)
		{
			IRelayPorts relayPort = Receiver as IRelayPorts;

			if (relayPort != null && address >= 1 && address <= relayPort.NumberOfRelayPorts)
				return relayPort.RelayPorts[(uint)address];

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
			// No DM endpoints currently implement versiports, but added for future use
			
			// ReSharper disable once ExpressionIsAlwaysNull
			// ReSharper disable once SuspiciousTypeConversion.Global
			IIOPorts ioPorts = Receiver as IIOPorts;

			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			if (ioPorts != null && address >= 1 && address <= ioPorts.NumberOfVersiPorts)
				// ReSharper disable once HeuristicUnreachableCode
				return ioPorts.VersiPorts[(uint)address];

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
			IDigitalInputPorts ioPorts = Receiver as IDigitalInputPorts;

			if (ioPorts != null && address >= 1 && address <= ioPorts.NumberOfDigitalInputPorts)
				return ioPorts.DigitalInputPorts[(uint)address];

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

#if !NETSTANDARD
			DMOutput input = m_Receiver == null ? null : m_Receiver.DMOutput;

			settings.Ipid = m_Receiver == null ? (byte)0 : (byte)m_Receiver.ID;
			settings.DmSwitch = m_ParentId;
			settings.DmOutputAddress = input == null ? (int?)null : (int)input.Number;
#else
            settings.Ipid = 0;
            settings.DmOutputAddress = null;
#endif
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

#if !NETSTANDARD
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

#if !NETSTANDARD
			TReceiver scaler = null;

			try
			{
				scaler = DmEndpointFactoryUtils.InstantiateReceiver(settings, factory, this);
			}
			catch (Exception e)
			{
				Logger.Log(eSeverity.Error, "Failed to instantiate internal {0} - {1}", typeof(TReceiver).Name, e.Message);
			}

			SetScaler(scaler, settings.DmSwitch);
#endif
		}

		/// <summary>
		/// Override to add additional/different controls (ie RouteSwitcherControl) to the device
		/// <remarks>This is called from the abstract contructor, so the concrete constructor
		/// will not have been called yet. Use with caution.</remarks>
		/// </summary>
		protected virtual IRouteMidpointControl CreateRouteControl()
		{
			return new RouteMidpointControl(this, 0);
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

			IRouteMidpointControl control = CreateRouteControl();
			addControl(control);
		}

#if !NETSTANDARD
		/// <summary>
		/// Instantiates the receiver with the given IPID against the control system.
		/// </summary>
		/// <param name="ipid"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		public abstract TReceiver InstantiateReceiver(byte ipid, CrestronControlSystem controlSystem);

		/// <summary>
		/// Instantiates the receiver against the given DM Ouptut and configures it with the given IPID.
		/// </summary>
		/// <param name="ipid"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		public abstract TReceiver InstantiateReceiver(byte ipid, DMOutput output);

		/// <summary>
		/// Instantiates the receiver against the given DM Output.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public abstract TReceiver InstantiateReceiver(DMOutput output);
#endif

		#endregion

		#region Scaler Callbacks

#if !NETSTANDARD
		/// <summary>
		/// Subscribe to the scaler events.
		/// </summary>
		/// <param name="scaler"></param>
		protected virtual void Subscribe(TReceiver scaler)
		{
			if (scaler == null)
				return;

			scaler.OnlineStatusChange += ScalerOnlineStatusChange;
		}

		/// <summary>
		/// Unsubscribes from the scaler events.
		/// </summary>
		/// <param name="scaler"></param>
		protected virtual void Unsubscribe(TReceiver scaler)
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
#if !NETSTANDARD
			return Receiver != null && Receiver.IsOnline;
#else
            return false;
#endif
		}

		#endregion

		#region Console

#if !NETSTANDARD
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
