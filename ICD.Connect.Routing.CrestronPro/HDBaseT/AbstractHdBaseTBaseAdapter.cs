using System;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Xml;
using ICD.Connect.API.Nodes;
using ICD.Connect.Misc.CrestronPro.Utils;
using ICD.Connect.Routing.CrestronPro.Cards;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.SettingsProperties;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
#endif
using ICD.Connect.Devices;
using ICD.Connect.Misc.CrestronPro.Devices;

namespace ICD.Connect.Routing.CrestronPro.HDBaseT
{
#if !NETSTANDARD
	public delegate void DeviceChangeCallback(IHdBaseTBaseAdapter sender, HDBaseTBase device);

	public abstract class AbstractHdBaseTBaseAdapter<TDevice, TSettings> : AbstractDevice<TSettings>, IHdBaseTBaseAdapter
		where TDevice : HDBaseTBase
#else
	public abstract class AbstractHdBaseTBaseAdapter<TSettings> : AbstractDevice<TSettings>, IHdBaseTBaseAdapter
#endif
		where TSettings : IHdBaseTBaseAdapterSettings, new()
	{
#if !NETSTANDARD
		/// <summary>
		/// Raised when the wrapped device changes.
		/// </summary>
		public event DeviceChangeCallback OnDeviceChanged;

		private TDevice m_Device;

		private int? m_ParentId;
		private int? m_EndpointId;
#endif

		#region Properties

#if !NETSTANDARD
		/// <summary>
		/// Gets the wrapped device.
		/// </summary>
		public TDevice Device
		{
			get { return m_Device; }
			private set
			{
				if (value == m_Device)
					return;

				m_Device = value;

				DeviceChangeCallback handler = OnDeviceChanged;
				if (handler != null)
					handler(this, m_Device);
			}
		}

		/// <summary>
		/// Gets the wrapped device.
		/// </summary>
		HDBaseTBase IHdBaseTBaseAdapter.Device { get { return Device; } }
#endif

		#endregion

		#region Methods

		/// <summary>
		/// Release resources
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
#if !NETSTANDARD
			OnDeviceChanged = null;
#endif

			base.DisposeFinal(disposing);

#if !NETSTANDARD
			// Unsbscribe and unregister
			SetDevice(null, null, null);
#endif
		}

#if !NETSTANDARD
		/// <summary>
		/// Sets the wrapped device.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="parentId"></param>
		/// <param name="endpointId"></param>
		[PublicAPI]
		public void SetDevice(TDevice device, int? parentId, int? endpointId)
		{
			Unsubscribe(Device);

			if (Device != null)
				GenericBaseUtils.TearDown(Device);

			m_ParentId = parentId;
			m_EndpointId = endpointId;
			Device = device;

			eDeviceRegistrationUnRegistrationResponse result;
			if (Device != null && !GenericBaseUtils.SetUp(Device, this, out result))
				Logger.Log(eSeverity.Error, "Unable to register {0} - {1}", Device.GetType().Name, result);

			Subscribe(Device);

			UpdateCachedOnlineStatus();
		}
#endif

		#endregion

#if !NETSTANDARD
		#region IO

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public ComPort GetComPort(int address)
		{
			if (Device == null)
				throw new InvalidOperationException("No device instantiated");

			return Device.ComPorts[(uint)address];
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual IROutputPort GetIrOutputPort(int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(IROutputPort).Name);
			throw new NotSupportedException(message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public Relay GetRelayPort(int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(Relay).Name);
			throw new NotSupportedException(message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public Versiport GetIoPort(int address)
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

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="io"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual Cec GetCecPort(eInputOuptut io, int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(Cec).Name);
			throw new NotSupportedException(message);
		}

		#endregion
#endif

		#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

#if !NETSTANDARD
			settings.Ipid = m_Device == null ? (byte)0 : (byte)m_Device.ID;
			settings.DmSwitch = m_ParentId;
			settings.DmEndpoint = m_EndpointId;
#else
			settings.Ipid = 0;
			settings.DmSwitch = null;
			settings.DmEndpoint = null;
#endif
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

#if !NETSTANDARD
			SetDevice(null, null, null);
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
			TDevice device = null;

			try
			{
				device = InstantiateDevice(settings, factory);
			}
			catch (Exception e)
			{
				Logger.Log(eSeverity.Error, "Failed to instantiate internal {0} - {1}", typeof(TDevice).Name, e.Message);
			}

			SetDevice(device, settings.DmSwitch, settings.DmEndpoint);
#endif
		}

#if !NETSTANDARD
		/// <summary>
		/// Instantiates the device with the given settings.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="deviceFactory"></param>
		/// <returns></returns>
		public abstract TDevice InstantiateDevice(TSettings settings, IDeviceFactory deviceFactory);
#endif

		#endregion

		#region Scaler Callbacks

#if !NETSTANDARD
		/// <summary>
		/// Subscribe to the device events.
		/// </summary>
		/// <param name="device"></param>
		private void Subscribe(TDevice device)
		{
			if (device == null)
				return;

			device.OnlineStatusChange += DeviceOnlineStatusChange;
		}

		/// <summary>
		/// Unsubscribes from the device events.
		/// </summary>
		/// <param name="device"></param>
		private void Unsubscribe(TDevice device)
		{
			if (device == null)
				return;

			device.OnlineStatusChange -= DeviceOnlineStatusChange;
		}

		/// <summary>
		/// Called when the device online status changes.
		/// </summary>
		/// <param name="currentDevice"></param>
		/// <param name="args"></param>
		private void DeviceOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
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
			return Device != null && Device.IsOnline;
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

			addRow("IPID", m_Device == null ? null : StringUtils.ToIpIdString((byte)m_Device.ID));
			addRow("DM Parent", m_ParentId);
			addRow("DM Endpoint", m_EndpointId);

			DMInputOutputBase output = Device == null ? null : Device.DMInputOutput;
			addRow("DM Address", output == null ? null : output.Number.ToString());
		}
#endif

		#endregion
	}

	public abstract class AbstractHdBaseTBaseAdapterSettings : AbstractDeviceSettings, IHdBaseTBaseAdapterSettings
	{
		private const string IPID_ELEMENT = "IPID";
		private const string DM_SWITCH_ELEMENT = "DmParent";
		private const string DM_ENDPOINT_ELEMENT = "DmEndpoint";

		[CrestronByteSettingsProperty]
		public byte? Ipid { get; set; }

		[OriginatorIdSettingsProperty(typeof(IDmParent))]
		public int? DmSwitch { get; set; }

		[OriginatorIdSettingsProperty(typeof(IDmEndpoint))]
		public int? DmEndpoint { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(IPID_ELEMENT, Ipid == null ? null : StringUtils.ToIpIdString((byte)Ipid));
			writer.WriteElementString(DM_SWITCH_ELEMENT, IcdXmlConvert.ToString(DmSwitch));
			writer.WriteElementString(DM_ENDPOINT_ELEMENT, IcdXmlConvert.ToString(DmEndpoint));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Ipid = XmlUtils.TryReadChildElementContentAsByte(xml, IPID_ELEMENT);
			DmSwitch = XmlUtils.TryReadChildElementContentAsInt(xml, DM_SWITCH_ELEMENT);
			DmEndpoint = XmlUtils.TryReadChildElementContentAsInt(xml, DM_ENDPOINT_ELEMENT);
		}
	}

	public interface IHdBaseTBaseAdapter : IDevice, IPortParent
	{
#if !NETSTANDARD
		/// <summary>
		/// Raised when the wrapped device changes.
		/// </summary>
		event DeviceChangeCallback OnDeviceChanged;

		/// <summary>
		/// Gets the wrapped device.
		/// </summary>
		HDBaseTBase Device { get; }
#endif
	}

	public interface IHdBaseTBaseAdapterSettings : IDeviceSettings
	{
		byte? Ipid { get; set; }

		int? DmSwitch { get; set; }

		int? DmEndpoint { get; set; }
	}
}
