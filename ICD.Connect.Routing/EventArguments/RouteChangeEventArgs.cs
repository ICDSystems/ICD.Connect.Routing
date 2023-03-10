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
		private readonly int? m_OldInput;
		private readonly int? m_NewInput;
		private readonly int m_Output;
		private readonly eConnectionType m_Type;

		/// <summary>
		/// The previous input address.
		/// </summary>
		[PublicAPI]
		public int? OldInput { get { return m_OldInput; } }

		/// <summary>
		/// The new input address.
		/// </summary>
		[PublicAPI]
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
		/// <param name="oldInput"></param>
		/// <param name="newInput"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		public RouteChangeEventArgs(int? oldInput, int? newInput, int output, eConnectionType type)
		{
			m_OldInput = oldInput;
			m_NewInput = newInput;
			m_Type = type;
			m_Output = output;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="args"></param>
		public RouteChangeEventArgs(RouteChangeEventArgs args)
			: this(args.OldInput, args.NewInput, args.Output, args.Type)
		{
		}

		/// <summary>
		/// Gets the string representation for this instance.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			ReprBuilder builder = new ReprBuilder(this);

			builder.AppendProperty("Old Input", m_OldInput);
			builder.AppendProperty("New Input", m_NewInput);
			builder.AppendProperty("Output", m_Output);
			builder.AppendProperty("Type", m_Type);

			return builder.ToString();
		}
	}
}
