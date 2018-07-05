using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;

namespace ICD.Connect.Routing.EventArguments
{
	public sealed class SwitcherRouteChangeEventArgs : EventArgs
	{
		private readonly IRouteSwitcherControl m_Control;
		private readonly int? m_OldInput;
		private readonly int? m_NewInput;
		private readonly int m_Output;
		private readonly eConnectionType m_Type;

		private readonly IcdHashSet<EndpointInfo> m_OldSourceEndpoints;
		private readonly IcdHashSet<EndpointInfo> m_NewSourceEndpoints;
		private readonly IcdHashSet<EndpointInfo> m_DestinationEndpoints;

		/// <summary>
		/// The switcher control.
		/// </summary>
		[PublicAPI]
		public IRouteSwitcherControl Control { get { return m_Control; } }

		/// <summary>
		/// The old input address.
		/// </summary>
		public int? OldInput { get { return m_OldInput; } }

		/// <summary>
		/// The new input address.
		/// </summary>
		public int? NewInput { get { return m_NewInput; } }

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
		/// <param name="oldInput"></param>
		/// <param name="newInput"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <param name="oldSourceEndpoints"></param>
		/// <param name="destinationEndpoints"></param>
		/// <param name="newSourceEndpoints"></param>
		public SwitcherRouteChangeEventArgs(IRouteSwitcherControl control,
		                                    int? oldInput,
		                                    int? newInput,
		                                    int output,
		                                    eConnectionType type,
		                                    IEnumerable<EndpointInfo> oldSourceEndpoints,
		                                    IEnumerable<EndpointInfo> newSourceEndpoints,
		                                    IEnumerable<EndpointInfo> destinationEndpoints)
		{
			m_Control = control;
			m_OldInput = oldInput;
			m_NewInput = newInput;
			m_Output = output;
			m_Type = type;

			m_OldSourceEndpoints = oldSourceEndpoints.ToIcdHashSet();
			m_NewSourceEndpoints = newSourceEndpoints.ToIcdHashSet();
			m_DestinationEndpoints = destinationEndpoints.ToIcdHashSet();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="args"></param>
		public SwitcherRouteChangeEventArgs(SwitcherRouteChangeEventArgs args)
			: this(args.Control, args.OldInput, args.NewInput, args.Output, args.Type, args.OldSourceEndpoints,
			       args.NewSourceEndpoints, args.DestinationEndpoints)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="args"></param>
		/// <param name="oldSourceEndpoints"></param>
		/// <param name="newSourceEndpoints"></param>
		/// <param name="destinationEndpoints"></param>
		public SwitcherRouteChangeEventArgs(IRouteSwitcherControl control, RouteChangeEventArgs args,
		                                    IEnumerable<EndpointInfo> oldSourceEndpoints,
		                                    IEnumerable<EndpointInfo> newSourceEndpoints,
		                                    IEnumerable<EndpointInfo> destinationEndpoints)
			: this(control, args.OldInput, args.NewInput, args.Output, args.Type, oldSourceEndpoints, newSourceEndpoints,
			       destinationEndpoints)
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
			builder.AppendProperty("OldInput", m_OldInput);
			builder.AppendProperty("NewInput", m_NewInput);
			builder.AppendProperty("Output", m_Output);
			builder.AppendProperty("Type", m_Type);

			return builder.ToString();
		}
	}
}
