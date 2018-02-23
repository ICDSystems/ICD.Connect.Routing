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
		private readonly int m_Output;
		private readonly eConnectionType m_Type;

		/// <summary>
		/// The switcher control.
		/// </summary>
		[PublicAPI]
		public IRouteSwitcherControl Control { get { return m_Control; } }

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
		/// <param name="output"></param>
		/// <param name="type"></param>
		public SwitcherRouteChangeEventArgs(IRouteSwitcherControl control, int output, eConnectionType type)
		{
			m_Control = control;
			m_Type = type;
			m_Output = output;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="args"></param>
		public SwitcherRouteChangeEventArgs(SwitcherRouteChangeEventArgs args)
			: this(args.Control, args.Output, args.Type)
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
			builder.AppendProperty("Output", m_Output);
			builder.AppendProperty("Type", m_Type);

			return builder.ToString();
		}
	}
}
