using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.Controls
{
	public sealed class RoutingGraphMidpointConnectionComponent
	{

		private readonly RoutingGraphSourceConnectionComponent m_SourceComponent;

		private readonly RoutingGraphDestinationConnectionComponent m_DestinationComponent;

		/// <summary>
		/// Create a midpoint component to use the routing graph to get inputs/outputs
		/// </summary>
		/// <param name="midpointControl">Midpoint control this is being used for</param>
		public RoutingGraphMidpointConnectionComponent(IRouteMidpointControl midpointControl) : this(midpointControl,
		                                                                                   EnumUtils
			                                                                                   .GetFlagsAllValue<eConnectionType>())
		{
		}

		/// <summary>
		/// Create a midpoint component to use the routing graph to get inputs/outputs
		/// </summary>
		/// <param name="midpointControl">Midpoint control this is being used for</param>
		/// <param name="connectionTypeMask">Connection Type Mask to restrict connections returned regardless of routing graph</param>
		public RoutingGraphMidpointConnectionComponent(IRouteMidpointControl midpointControl, eConnectionType connectionTypeMask)
		{
			m_SourceComponent = new RoutingGraphSourceConnectionComponent(midpointControl, connectionTypeMask);
			m_DestinationComponent = new RoutingGraphDestinationConnectionComponent(midpointControl, connectionTypeMask);
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public ConnectorInfo GetOutput(int address)
		{
			return m_SourceComponent.GetOutput(address);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public bool ContainsOutput(int output)
		{
			return m_SourceComponent.ContainsOutput(output);
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ConnectorInfo> GetOutputs()
		{
			return m_SourceComponent.GetOutputs();
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public ConnectorInfo GetInput(int input)
		{
			return m_DestinationComponent.GetInput(input);
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public bool ContainsInput(int input)
		{
			return m_DestinationComponent.ContainsInput(input);
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ConnectorInfo> GetInputs()
		{
			return m_DestinationComponent.GetInputs();
		}
	}
}
