using System;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Timers;
using ICD.Connect.Routing.Telemetry;
using ICD.Connect.Devices.Telemetry;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Providers.External;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem
{
	[UsedImplicitly]
	public sealed class ControlSystemExternalTelemetryProvider : AbstractExternalTelemetryProvider<IControlSystemDevice>
	{
		private const string PROGRAMMER_NAME = "ICD Systems";
		private const string SYSTEM_NAME = "ICD.Connect";

		private const long UPTIME_UPDATE_TIMER_INTERVAL = 10 * 60 * 1000;

		#region Events

		[EventTelemetry(DeviceTelemetryNames.DEVICE_UPTIME_CHANGED)]
		public event EventHandler<GenericEventArgs<TimeSpan>> OnProcessorUptimeChanged;

		[EventTelemetry(ControlSystemExternalTelemetryNames.PROGRAM_UPTIME_CHANGED)]
		public event EventHandler<GenericEventArgs<TimeSpan>> OnProgramUptimeChanged;

		#endregion

		private readonly IcdTimer m_UptimeUpdateTimer;

		#region Properties

		[PropertyTelemetry(DeviceTelemetryNames.DEVICE_UPTIME, null, DeviceTelemetryNames.DEVICE_UPTIME_CHANGED)]
		public TimeSpan ProcessorUptime { get { return ProcessorUtils.GetSystemUptime(); } }

		[PropertyTelemetry(ControlSystemExternalTelemetryNames.PROGRAM_UPTIME, null, ControlSystemExternalTelemetryNames.PROGRAM_UPTIME_CHANGED)]
		public TimeSpan ProgramUptime { get { return ProcessorUtils.GetProgramUptime(); } }

		[PropertyTelemetry(ControlSystemExternalTelemetryNames.PROGRAMMER_NAME, null, null)]
		public string ProgrammerName { get { return PROGRAMMER_NAME; } }

		[PropertyTelemetry(ControlSystemExternalTelemetryNames.SYSTEM_NAME, null, null)]
		public string SystemName { get { return SYSTEM_NAME; } }

		[PropertyTelemetry(ControlSystemExternalTelemetryNames.PROGRAM_SOURCE_FILE, null, null)]
		public string ProgramSourceFile { get { return ProgramUtils.ProgramFile; } }

		[PropertyTelemetry(ControlSystemExternalTelemetryNames.PROGRAM_COMPLIE_DATE, null, null)]
		public DateTime ProgramCompileDate { get { return ProgramUtils.CompiledDate; } }

		#endregion

		public ControlSystemExternalTelemetryProvider()
		{
			// Adjust the uptime counter every 10 minutes
			m_UptimeUpdateTimer = new IcdTimer();
			m_UptimeUpdateTimer.OnElapsed += UptimeUpdateTimerOnElapsed;
			m_UptimeUpdateTimer.Restart(UPTIME_UPDATE_TIMER_INTERVAL);
		}

		#region Private Methods

		private void UptimeUpdateTimerOnElapsed(object sender, EventArgs eventArgs)
		{
			OnProcessorUptimeChanged.Raise(this, new GenericEventArgs<TimeSpan>(ProcessorUptime));
			OnProgramUptimeChanged.Raise(this, new GenericEventArgs<TimeSpan>(ProgramUptime));
			m_UptimeUpdateTimer.Restart(UPTIME_UPDATE_TIMER_INTERVAL);
		}

		#endregion
	}
}
