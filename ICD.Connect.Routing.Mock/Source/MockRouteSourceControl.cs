﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Services;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Mock.Source
{
	public sealed class MockRouteSourceControl : AbstractRouteSourceControl<IDevice>
	{
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		private ConnectorInfo[] m_Outputs;
		private readonly Dictionary<int, Dictionary<eConnectionType, bool>> m_TransmissionStates;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public MockRouteSourceControl(IDevice parent, int id)
			: base(parent, id)
		{
			m_Outputs = new ConnectorInfo[0];
			m_TransmissionStates = new Dictionary<int, Dictionary<eConnectionType, bool>>();

			foreach (ConnectorInfo output in GetOutputs())
				SetActiveTransmissionState(output.Address, output.ConnectionType, true);
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
			return m_TransmissionStates.ContainsKey(output) && m_TransmissionStates[output].GetDefault(type, false);
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			return
				ServiceProvider.GetService<IRoutingGraph>()
				               .Connections.GetConnections()
				               .Where(c => c.Source.Device == Parent.Id && c.Source.Control == Id)
				               .Select(c => new ConnectorInfo(c.Source.Address, c.ConnectionType));
		}

		/// <summary>
		/// Sets the outputs.
		/// </summary>
		/// <param name="outputs"></param>
		public void SetOutputs(IEnumerable<ConnectorInfo> outputs)
		{
			m_Outputs = outputs.ToArray();
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

			yield return new GenericConsoleCommand<int, eConnectionType>(
				"AddOutput",
				"Adds an output to the device",
				(a, b) => AddOutput(a, b));

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

		private void AddOutput(int address, eConnectionType type)
		{
			SetOutputs(GetOutputs().Append(new ConnectorInfo(address, type)));
		}

		#endregion
	}
}
