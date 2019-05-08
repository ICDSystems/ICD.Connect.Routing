using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.RoutingGraphs;

namespace ICD.Connect.Routing.Mock.Source
{
	public sealed class MockRouteSourceControl : AbstractRouteSourceControl<IDeviceBase>
	{
		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		private readonly Dictionary<int, Dictionary<eConnectionType, bool>> m_TransmissionStates;

		private IRoutingGraph m_CachedRoutingGraph;

		/// <summary>
		/// Gets the routing graph.
		/// </summary>
		public IRoutingGraph RoutingGraph
		{
			get { return m_CachedRoutingGraph = m_CachedRoutingGraph ?? ServiceProvider.GetService<IRoutingGraph>(); }
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public MockRouteSourceControl(IDeviceBase parent, int id)
			: base(parent, id)
		{
			m_TransmissionStates = new Dictionary<int, Dictionary<eConnectionType, bool>>();
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnActiveTransmissionStateChanged = null;

			base.DisposeFinal(disposing);
		}

		#region Methods

		/// <summary>
		/// Returns true if the device is actively transmitting on the given output.
		/// This is NOT the same as sending video, since some devices may send an
		/// idle signal by default.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetActiveTransmissionState(int output, eConnectionType type)
		{
			// Default to true
			return !m_TransmissionStates.ContainsKey(output) || m_TransmissionStates[output].GetDefault(type, true);
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int address)
		{
			Connection connection = RoutingGraph.Connections.GetOutputConnection(new EndpointInfo(Parent.Id, Id, address));
			if (connection == null)
				throw new ArgumentOutOfRangeException("address");

			return new ConnectorInfo(connection.Source.Address, connection.ConnectionType);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return RoutingGraph.Connections.GetOutputConnection(new EndpointInfo(Parent.Id, Id, output)) != null;
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			return RoutingGraph.Connections
							   .GetOutputConnections(Parent.Id, Id)
							   .Select(c => new ConnectorInfo(c.Source.Address, c.ConnectionType));
		}

		/// <summary>
		/// Sets the active transmission state for the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <param name="state"></param>
		[PublicAPI]
		public void SetActiveTransmissionState(int output, eConnectionType type, bool state)
		{
			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
			{
				if (state == GetActiveTransmissionState(output, flag))
					continue;

				if (!m_TransmissionStates.ContainsKey(output))
					m_TransmissionStates[output] = new Dictionary<eConnectionType, bool>();

				m_TransmissionStates[output][flag] = state;

				OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(output, flag, state));
			}
		}

		#endregion

		#region Console

		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			string help = string.Format("SetTransmissionState <output> <{0}> <true/false>",
			                            StringUtils.ArrayFormat(EnumUtils.GetValues<eConnectionType>()));

			yield return
				new GenericConsoleCommand<int, eConnectionType, bool>("SetTransmissionState", help,
				                                                      (a, b, c) => SetActiveTransmissionState(a, b, c));
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}
