using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;

namespace ICD.Connect.Routing.EventArguments
{
	public sealed class SwitcherRouteChangeEventArgs : EventArgs
	{
		private readonly IRouteSwitcherControl m_Control;
		private readonly int? m_Input;
		private readonly int m_Output;
		private readonly eConnectionType m_Type;
		private readonly IEnumerable<EndpointInfo> m_OldSourceEndpoints;
		private readonly IEnumerable<EndpointInfo> m_NewSourceEndpoints;
		private readonly IEnumerable<EndpointInfo> m_DestinationEndpoints;

		/// <summary>
		/// The switcher control.
		/// </summary>
		[PublicAPI]
		public IRouteSwitcherControl Control { get { return m_Control; } }

		/// <summary>
		/// The input address.
		/// </summary>
		public int? Input { get { return m_Input; } }

		/// <summary>
		/// The output address.
		/// </summary>
		[PublicAPI]
		public int Output { get { return m_Output; } }

		/// <summary>
		/// The old source endpoints
		/// </summary>
		[PublicAPI]
		public IEnumerable<EndpointInfo> OldSourceEndpoints{get { return m_OldSourceEndpoints; }}

		/// <summary>
		/// The new source endpoints
		/// </summary>
		[PublicAPI]
		public IEnumerable<EndpointInfo> NewSourceEndpoints{get { return m_NewSourceEndpoints; }}

		/// <summary>
		/// The endpoints in the changed route.
		/// </summary>
		[PublicAPI]
		public IEnumerable<EndpointInfo> DestinationEndpoints{get { return m_DestinationEndpoints; }}

		/// <summary>
		/// The connection type.
		/// </summary>
		[PublicAPI]
		public eConnectionType Type { get { return m_Type; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="input"></param>
		/// <param name="output"></param>
		/// <param name="oldRoute"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="newRoute"></param>
		public SwitcherRouteChangeEventArgs(IRouteSwitcherControl control, 
											int? input, 
											int output,
											IEnumerable<EndpointInfo> oldRoute, 
											IEnumerable<EndpointInfo> newRoute, 
											IEnumerable<EndpointInfo>destination,
											eConnectionType type)
		{
			m_Control = control;
			m_Type = type;
			m_Input = input;
			m_OldSourceEndpoints = oldRoute;
			m_NewSourceEndpoints = newRoute;
			m_DestinationEndpoints = destination;
			m_Output = output;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="args"></param>
		public SwitcherRouteChangeEventArgs(SwitcherRouteChangeEventArgs args)
			: this(args.Control, args.Input, args.Output, args.OldSourceEndpoints, args.NewSourceEndpoints, args.DestinationEndpoints, args.Type)
		{
		}

		/// <summary>
		/// Gets the string representation for this instance.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			ReprBuilder builder = new ReprBuilder(this);

			builder.AppendProperty("Control", m_Control);
			builder.AppendProperty("Input", m_Input);
			builder.AppendProperty("Output", m_Output);
			builder.AppendProperty("Endpoints", m_OldSourceEndpoints);
			builder.AppendProperty("Type", m_Type);

			return builder.ToString();
		}
	}
}
