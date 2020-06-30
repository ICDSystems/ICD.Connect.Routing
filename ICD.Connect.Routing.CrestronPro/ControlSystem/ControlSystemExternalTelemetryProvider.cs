﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Timers;
using ICD.Connect.Telemetry.Providers.External;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem
{
	[UsedImplicitly]
	public sealed class ControlSystemExternalTelemetryProvider : AbstractExternalTelemetryProvider<IControlSystemDevice>, IControlSystemExternalTelemetryProvider
	{
		private const string DHCP_ON_TEXT = "ON";

		// Drew said to make these constants for now.
		private const string PROGRAMMER_NAME = "ICD Systems";
		private const string SYSTEM_NAME = "Metlife.RoomOS";

		private readonly IcdTimer m_UptimeUpdateTimer;
		private const long UPTIME_UPDATE_TIMER_INTERVAL = 10 * 60 * 1000;

		private string[] m_Hostnames;
		private string[] m_IpAddresses;
		private string m_DhcpStatus;

		public event EventHandler<BoolEventArgs> OnDhcpStatusChanged;
		public event EventHandler<StringEventArgs> OnProcessorIpAddressChanged;
		public event EventHandler<StringEventArgs> OnProcessorHostnameChanged;
		public event EventHandler<StringEventArgs> OnSystemNameChanged;
		public event EventHandler<StringEventArgs> OnIpAddressSecondaryChanged;
		public event EventHandler<StringEventArgs> OnProcessorHostnameCustomChanged;
		public event EventHandler<GenericEventArgs<TimeSpan>> OnProcessorUptimeChanged;
		public event EventHandler<GenericEventArgs<TimeSpan>> OnProgramUptimeChanged;

		public bool DhcpStatus { get { return !string.IsNullOrEmpty(IcdEnvironment.DhcpStatus) && IcdEnvironment.DhcpStatus.Equals(DHCP_ON_TEXT, StringComparison.OrdinalIgnoreCase); } }

		public string ProcessorFirmwareVersion { get { return ProcessorUtils.ModelVersion.ToString(); } }
		public string ProcessorFirmwareDate { get { return ProcessorUtils.ModelVersionDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"); } }
		public string ProcessorMacAddress { get { return IcdEnvironment.MacAddresses.First(); } }
		public string ProcessorIpAddress { get { return GetPrimaryIp(); } }
		public string ProcessorHostname { get { return GetPrimaryHostname(); } }

		public TimeSpan ProcessorUptime { get { return ProcessorUtils.GetSystemUptime(); } }
		public TimeSpan ProgramUptime { get { return ProcessorUtils.GetProgramUptime(); } }

		public string ProgrammerName { get { return PROGRAMMER_NAME; } }
		public string SystemName { get { return SYSTEM_NAME; } }
		public string ProgramSourceFile { get { return ProgramUtils.ProgramFile; } }
		public string ProgramCompileDate { get { return ProgramUtils.CompiledDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"); } }
		public string ProcessorIpAddressSecondary { get { return GetSecondaryIp(); } }
		public string ProcessorHostnameCustom { get { return GetSecondaryHostname(); } }

		public ControlSystemExternalTelemetryProvider()
		{
			IcdEnvironment.OnEthernetEvent += IcdEnvironmentOnEthernetEvent;

			// Adjust the uptime counter every 10 minutes
			m_UptimeUpdateTimer = new IcdTimer();
			m_UptimeUpdateTimer.OnElapsed += UptimeUpdateTimerOnElapsed;
			m_UptimeUpdateTimer.Restart(UPTIME_UPDATE_TIMER_INTERVAL);
		}

		private void UptimeUpdateTimerOnElapsed(object sender, EventArgs eventArgs)
		{
			OnProcessorUptimeChanged.Raise(this, new GenericEventArgs<TimeSpan>(ProcessorUptime));
			OnProgramUptimeChanged.Raise(this, new GenericEventArgs<TimeSpan>(ProgramUptime));
			m_UptimeUpdateTimer.Restart(UPTIME_UPDATE_TIMER_INTERVAL);
		}

		private IEnumerable<string> GetHostnames()
		{
			return m_Hostnames ?? (m_Hostnames = IcdEnvironment.Hostname.ToArray());
		}

		private IEnumerable<string> GetIpAddresses()
		{
			return m_IpAddresses ?? (m_IpAddresses = IcdEnvironment.NetworkAddresses.ToArray());
		}

		private string GetDhcpStatus()
		{
			return m_DhcpStatus ?? (m_DhcpStatus = IcdEnvironment.DhcpStatus);
		}

		private string GetPrimaryHostname()
		{
			string hostname;
			return GetHostnames().TryElementAt(0, out hostname) ? hostname : string.Empty;
		}

		private string GetSecondaryHostname()
		{
			string hostname;
			return GetHostnames().TryElementAt(1, out hostname) ? hostname : string.Empty;
		}

		private string GetPrimaryIp()
		{
			string ip;
			return GetIpAddresses().TryElementAt(0, out ip) ? ip : string.Empty;
		}

		private string GetSecondaryIp()
		{
			string ip;
			return GetIpAddresses().TryElementAt(1, out ip) ? ip : string.Empty;
		}

		private void IcdEnvironmentOnEthernetEvent(IcdEnvironment.eEthernetAdapterType adapter, IcdEnvironment.eEthernetEventType type)
		{
			string primaryIp = GetPrimaryIp();
			string secondaryIp = GetSecondaryIp();
			string primaryHostname = GetPrimaryHostname();
			string secondaryHostname = GetSecondaryHostname();
			string dhcpStatus = GetDhcpStatus();

			m_IpAddresses = null;
			m_Hostnames = null;
			m_DhcpStatus = null;

			string newPrimaryIp = GetPrimaryIp();
			string newSecondaryIp = GetSecondaryIp();
			string newPrimaryHostname = GetPrimaryHostname();
			string newSecondaryHostname = GetSecondaryHostname();
			string newDhcpStatus = GetDhcpStatus();

			if(primaryIp != newPrimaryIp)
				OnProcessorIpAddressChanged.Raise(this, new StringEventArgs(newPrimaryIp));

			if(secondaryIp != newSecondaryIp)
				OnIpAddressSecondaryChanged.Raise(this, new StringEventArgs(newSecondaryIp));

			if(primaryHostname != newPrimaryHostname)
				OnIpAddressSecondaryChanged.Raise(this, new StringEventArgs(newPrimaryHostname));

			if(secondaryHostname != newSecondaryHostname)
				OnProcessorHostnameCustomChanged.Raise(this, new StringEventArgs(newSecondaryHostname));

			if(dhcpStatus != newDhcpStatus)
				OnDhcpStatusChanged.Raise(this, new BoolEventArgs(IcdEnvironment.DhcpStatus != string.Empty));
		}
	}
}