using System;
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
		// Drew said to make these constants for now.
		private const string PROGRAMMER_NAME = "ICD Systems";
		private const string SYSTEM_NAME = "Metlife.RoomOS";

		private const long UPTIME_UPDATE_TIMER_INTERVAL = 10 * 60 * 1000;

		public event EventHandler<BoolEventArgs> OnDhcpStatusChanged;
		public event EventHandler<StringEventArgs> OnProcessorIpAddressChanged;
		public event EventHandler<StringEventArgs> OnProcessorIpAddressSecondaryChanged;
		public event EventHandler<StringEventArgs> OnProcessorHostnameChanged;
		public event EventHandler<StringEventArgs> OnProcessorHostnameSecondaryChanged;
		public event EventHandler<GenericEventArgs<TimeSpan>> OnProcessorUptimeChanged;
		public event EventHandler<GenericEventArgs<TimeSpan>> OnProgramUptimeChanged;
		public event EventHandler<StringEventArgs> OnSystemNameChanged;

		private readonly IcdTimer m_UptimeUpdateTimer;

		public bool DhcpStatus { get { return IcdEnvironment.DhcpStatus; } }

		public Version ProcessorFirmwareVersion { get { return ProcessorUtils.ModelVersion; } }
		public DateTime ProcessorFirmwareDate { get { return ProcessorUtils.ModelVersionDate; } }
		public string ProcessorMacAddress { get { return IcdEnvironment.MacAddresses.FirstOrDefault() ?? string.Empty; } }
		public string ProcessorIpAddress { get { return IcdEnvironment.NetworkAddresses.FirstOrDefault() ?? string.Empty; } }
		public string ProcessorIpAddressSecondary { get { return IcdEnvironment.NetworkAddresses.Skip(1).FirstOrDefault() ?? string.Empty; } }
		public string ProcessorHostname { get { return IcdEnvironment.Hostnames.FirstOrDefault() ?? string.Empty; } }
		public string ProcessorHostnameSecondary { get { return IcdEnvironment.Hostnames.Skip(1).FirstOrDefault() ?? string.Empty; } }

		public TimeSpan ProcessorUptime { get { return ProcessorUtils.GetSystemUptime(); } }
		public TimeSpan ProgramUptime { get { return ProcessorUtils.GetProgramUptime(); } }

		public string ProgrammerName { get { return PROGRAMMER_NAME; } }
		public string SystemName { get { return SYSTEM_NAME; } }
		public string ProgramSourceFile { get { return ProgramUtils.ProgramFile; } }
		public DateTime ProgramCompileDate { get { return ProgramUtils.CompiledDate; } }

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

		private void IcdEnvironmentOnEthernetEvent(IcdEnvironment.eEthernetAdapterType adapter, IcdEnvironment.eEthernetEventType type)
		{
			OnDhcpStatusChanged.Raise(this, new BoolEventArgs(DhcpStatus));
			OnProcessorIpAddressChanged.Raise(this, new StringEventArgs(ProcessorIpAddress));
			OnProcessorIpAddressSecondaryChanged.Raise(this, new StringEventArgs(ProcessorIpAddressSecondary));
			OnProcessorHostnameChanged.Raise(this, new StringEventArgs(ProcessorHostname));
			OnProcessorHostnameSecondaryChanged.Raise(this, new StringEventArgs(ProcessorHostnameSecondary));
		}
	}
}
