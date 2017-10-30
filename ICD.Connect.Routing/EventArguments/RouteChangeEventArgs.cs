using System;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.EventArguments
{
	/// <summary>
	/// Used when an output starts routing a different input.
	/// </summary>
	public sealed class RouteChangeEventArgs : EventArgs
	{
		private readonly int m_Output;
		private readonly eConnectionType m_Type;

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
		/// <param name="output"></param>
		/// <param name="type"></param>
		public RouteChangeEventArgs(int output, eConnectionType type)
		{
			m_Type = type;
			m_Output = output;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="args"></param>
		public RouteChangeEventArgs(RouteChangeEventArgs args)
			: this(args.Output, args.Type)
		{
		}

		/// <summary>
		/// Gets the string representation for this instance.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			ReprBuilder builder = new ReprBuilder(this);

			builder.AppendProperty("Output", m_Output);
			builder.AppendProperty("Type", m_Type);

			return builder.ToString();
		}
	}
}
