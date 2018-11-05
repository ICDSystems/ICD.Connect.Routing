using System;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;

namespace ICD.Connect.Routing.EventArguments
{
	public sealed class SwitcherRouteChangeEventArgs : EventArgs
	{
		private readonly IRouteSwitcherControl m_Control;
		private readonly int? m_OldInput;
		private readonly int? m_NewInput;
		private readonly int m_Output;
		private readonly eConnectionType m_Type;

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
		public SwitcherRouteChangeEventArgs(IRouteSwitcherControl control, int? oldInput, int? newInput, int output, eConnectionType type)
		{
			m_Control = control;
			m_OldInput = oldInput;
			m_NewInput = newInput;
			m_Output = output;
			m_Type = type;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="args"></param>
		public SwitcherRouteChangeEventArgs(SwitcherRouteChangeEventArgs args)
			: this(args.Control, args.OldInput, args.NewInput, args.Output, args.Type)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="args"></param>
		public SwitcherRouteChangeEventArgs(IRouteSwitcherControl control, RouteChangeEventArgs args)
			: this(control, args.OldInput, args.NewInput, args.Output, args.Type)
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
