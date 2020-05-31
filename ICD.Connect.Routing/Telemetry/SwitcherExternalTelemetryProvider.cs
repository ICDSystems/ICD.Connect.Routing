using System;
using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Telemetry.Providers;

namespace ICD.Connect.Routing.Telemetry
{
	public sealed class SwitcherExternalTelemetryProvider : ISwitcherExternalTelemetryProvider
	{
		private readonly IcdHashSet<InputPort> m_InputPorts = new IcdHashSet<InputPort>();
		private readonly IcdHashSet<OutputPort> m_OutputPorts = new IcdHashSet<OutputPort>();

		private IRouteSwitcherControl m_Parent;

		#region Properties

		public IEnumerable<InputPort> SwitcherInputPorts { get { return m_InputPorts; } }
		public IEnumerable<OutputPort> SwitcherOutputPorts { get { return m_OutputPorts; } }

		#endregion

		#region Provider Callbacks

		public void SetParent(ITelemetryProvider provider)
		{
			var switcherControl = provider as IRouteSwitcherControl;
			if (switcherControl == null)
				throw new ArgumentException("Provider for Switcher Telemetry must be IRouteSwitcherControl.");

			m_Parent = switcherControl;
			m_InputPorts.Clear();
			m_OutputPorts.Clear();
			m_InputPorts.AddRange(m_Parent.GetInputPorts());
			m_OutputPorts.AddRange(m_Parent.GetOutputPorts());

		}

		#endregion
	}
}