using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Collections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Providers.External;

namespace ICD.Connect.Routing.Telemetry
{
	public sealed class SwitcherExternalTelemetryProvider : AbstractExternalTelemetryProvider<IRouteSwitcherControl>
	{
		private readonly IcdHashSet<InputPort> m_InputPorts = new IcdHashSet<InputPort>();
		private readonly IcdHashSet<OutputPort> m_OutputPorts = new IcdHashSet<OutputPort>();

		#region Properties

		[CollectionTelemetry(SwitcherTelemetryNames.INPUT_PORTS)]
		public IEnumerable<InputPort> SwitcherInputPorts { get { return m_InputPorts; } }

		[CollectionTelemetry(SwitcherTelemetryNames.OUTPUT_PORTS)]
		public IEnumerable<OutputPort> SwitcherOutputPorts { get { return m_OutputPorts; } }

		#endregion

		#region Provider Callbacks

		/// <summary>
		/// Initializes the current telemetry state.
		/// </summary>
		public override void InitializeTelemetry()
		{
			base.InitializeTelemetry();

			IEnumerable<InputPort> inputPorts =
				Parent == null
					? Enumerable.Empty<InputPort>()
					: Parent.GetInputPorts();

			IEnumerable<OutputPort> outputPorts =
				Parent == null
					? Enumerable.Empty<OutputPort>()
					: Parent.GetOutputPorts();

			m_InputPorts.Clear();
			m_InputPorts.AddRange(inputPorts);

			m_OutputPorts.Clear();
			m_OutputPorts.AddRange(outputPorts);
		}

		#endregion
	}
}