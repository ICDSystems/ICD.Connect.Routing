using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.Controls.Streaming
{
	public abstract class AbstractStreamRouteSwitcherControl<T> : AbstractStreamRouteMidpointControl<T>, IStreamRouteSwitcherControl
		where T : IDeviceBase
	{
		private List<InputPort> m_InputPorts;
		private List<OutputPort> m_OutputPorts;

		protected AbstractStreamRouteSwitcherControl(T parent, int id)
			: base(parent, id)
		{
		}

		/// <summary>
		/// Returns switcher port objects to get details about the input ports on this switcher.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<InputPort> GetInputPorts()
		{
			if (m_InputPorts == null || m_InputPorts.Count == 0)
				m_InputPorts = GetInputs().Select(input => CreateInputPort(input))
				                          .ToList();
			return m_InputPorts.ToList();
		}

		protected virtual InputPort CreateInputPort(ConnectorInfo input)
		{
			return new InputPort
			{
				Address = input.Address,
				ConnectionType = input.ConnectionType,
				InputId = string.Format("Input {0}", input.Address),
				InputIdFeedbackSupported = true
			};
		}

		[NotNull]
		public InputPort GetInputPort(int address)
		{
			return GetInputPorts().First(i => i.Address == address);
		}

		/// <summary>
		/// Returns switcher port objects to get details about the output ports on this switcher.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<OutputPort> GetOutputPorts()
		{
			if (m_OutputPorts == null || m_OutputPorts.Count == 0)
				m_OutputPorts = GetOutputs().Select(output => CreateOutputPort(output))
				                            .ToList();
			return m_OutputPorts.ToList();
		}

		protected virtual OutputPort CreateOutputPort(ConnectorInfo output)
		{
			bool supportsVideo = output.ConnectionType.HasFlag(eConnectionType.Video);
			bool supportsAudio = output.ConnectionType.HasFlag(eConnectionType.Audio);

			return new OutputPort
			{
				Address = output.Address,
				ConnectionType = output.ConnectionType,
				OutputId = string.Format("Output {0}", output.Address),
				OutputIdFeedbackSupport = true,
				VideoOutputSource = supportsVideo ? GetActiveSourceIdName(output, eConnectionType.Video) : null,
				VideoOutputSourceFeedbackSupport = supportsVideo,
				AudioOutputSource = supportsAudio ? GetActiveSourceIdName(output, eConnectionType.Audio) : null,
				AudioOutputSourceFeedbackSupport = supportsAudio
			};
		}

		[NotNull]
		public OutputPort GetOutputPort(int address)
		{
			return GetOutputPorts().First(o => o.Address == address);
		}

		protected string GetActiveSourceIdName(ConnectorInfo info, eConnectionType type)
		{
			ConnectorInfo? activeInput = GetInput(info.Address, type);
			if (activeInput == null)
				return null;

			InputPort port = GetInputPort(activeInput.Value.Address);
			return string.Format("{0} {1}", port.InputId, port.InputName);
		}

		public bool Route(RouteOperation info)
		{
			throw new NotImplementedException();
		}

		public bool ClearOutput(int output, eConnectionType type)
		{
			throw new NotImplementedException();
		}
	}
}
