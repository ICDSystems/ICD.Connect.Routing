using System;
using System.Linq;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Timers;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.Telemetry.DeviceInfo;
using ICD.Connect.Routing.Extron.Controls.Routing;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Extron.Devices.Switchers.In1804
{
	public abstract class AbstractIn1804Device<TSettings> : AbstractExtronSwitcherDevice<TSettings>, IIn1804Device
		where TSettings : IIn1804DeviceSettings, new()
	{
		#region Members

		private const string INFORMATION_REQUEST_MODEL_NAME = "1I";
		private const string INFORMATION_REQUEST_FULL_FIRMWARE_VERSION = "*Q";
		private const string INFORMATION_REQUEST_DHCP_MODE = "WDH\r";
		private const string INFORMATION_REQUEST_IP_ADDRESS = "WCI\r";
		private const string INFORMATION_REQUEST_SUBNET_MASK = "WCS\r";
		private const string INFORMATION_REQUEST_IP_GATEWAY = "WCG\r";
		private const string INFORMATION_REQUEST_MAC_ADDRESS = "WCH\r";

		private const string INFORMATION_REQUEST_MODEL_NAME_KEY = "Inf01*";
		private const string INFORMATION_REQUEST_FULL_FIRMWARE_VERSION_KEY = "Bld";
		private const string INFORMATION_REQUEST_DHCP_MODE_KEY = "Idh";
		private const string INFORMATION_REQUEST_IP_ADDRESS_KEY = "Ipi ";
		private const string INFORMATION_REQUEST_SUBNET_MASK_KEY = "Ips ";
		private const string INFORMATION_REQUEST_IP_GATEWAY_KEY = "Ipg ";
		private const string INFORMATION_REQUEST_MAC_ADDRESS_KEY = "Iph ";

		/// <summary>
		/// Poll the device for general information every hour.
		/// </summary>
		private const long INFORMATION_REQUEST_INTERVAL = 1 * 60 * 60 * 1000;

		private readonly SafeTimer m_InformationRequestTimer;

		private readonly string[] m_InformationRequestStrings =
		{
			INFORMATION_REQUEST_MODEL_NAME_KEY,
			INFORMATION_REQUEST_FULL_FIRMWARE_VERSION_KEY,
			INFORMATION_REQUEST_DHCP_MODE_KEY,
			INFORMATION_REQUEST_IP_ADDRESS_KEY,
			INFORMATION_REQUEST_SUBNET_MASK_KEY,
			INFORMATION_REQUEST_IP_GATEWAY_KEY,
			INFORMATION_REQUEST_MAC_ADDRESS_KEY
		};

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractIn1804Device()
		{
			m_InformationRequestTimer = SafeTimer.Stopped(SendInformationRequests);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(TSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new In1804SwitcherControl(this, 0, 4, 2, false));
		}

		/// <summary>
		/// Override to add actions on StartSettings
		/// This should be used to start communications with devices and perform initial actions
		/// </summary>
		protected override void StartSettingsFinal()
		{
			base.StartSettingsFinal();

			m_InformationRequestTimer.Reset(0, INFORMATION_REQUEST_INTERVAL);
		}

		/// <summary>
		/// Polls the device for general information.
		/// </summary>
		private void SendInformationRequests()
		{
			SendCommand(INFORMATION_REQUEST_MODEL_NAME);
			SendCommand(INFORMATION_REQUEST_FULL_FIRMWARE_VERSION);
			SendCommand(INFORMATION_REQUEST_DHCP_MODE);
			SendCommand(INFORMATION_REQUEST_IP_ADDRESS);
			SendCommand(INFORMATION_REQUEST_SUBNET_MASK);
			SendCommand(INFORMATION_REQUEST_IP_GATEWAY);
			SendCommand(INFORMATION_REQUEST_MAC_ADDRESS);
		}

		/// <summary>
		/// Called when we receive a complete response from the device.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected override void BufferOnCompletedSerial(object sender, StringEventArgs args)
		{
			base.BufferOnCompletedSerial(sender, args);

			string responseKey = m_InformationRequestStrings.FirstOrDefault(key => args.Data.StartsWith(key));
			if (responseKey == null)
				return;

			string response = args.Data.Replace(responseKey, "");

			switch (responseKey)
			{
				case INFORMATION_REQUEST_MODEL_NAME_KEY:
					MonitoredDeviceInfo.Model = response;
					break;
				case INFORMATION_REQUEST_FULL_FIRMWARE_VERSION_KEY:
					MonitoredDeviceInfo.FirmwareVersion = response;
					break;
				case INFORMATION_REQUEST_DHCP_MODE_KEY:
					MonitoredDeviceInfo.NetworkInfo.Adapters.GetOrAddAdapter(1).Dhcp = response == "1";
					break;
				case INFORMATION_REQUEST_IP_ADDRESS_KEY:
					MonitoredDeviceInfo.NetworkInfo.Adapters.GetOrAddAdapter(1).Ipv4Gateway = response;
					break;
				case INFORMATION_REQUEST_SUBNET_MASK_KEY:
					MonitoredDeviceInfo.NetworkInfo.Adapters.GetOrAddAdapter(1).Ipv4SubnetMask = response;
					break;
				case INFORMATION_REQUEST_IP_GATEWAY_KEY:
					MonitoredDeviceInfo.NetworkInfo.Adapters.GetOrAddAdapter(1).Ipv4Gateway = response;
					break;
				case INFORMATION_REQUEST_MAC_ADDRESS_KEY:
					MonitoredDeviceInfo.NetworkInfo.Adapters.GetOrAddAdapter(1).MacAddress = IcdPhysicalAddress.Parse(response);
					break;
			}
		}

		#endregion
	}
}