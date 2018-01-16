#if SIMPLSHARP
using System;
using System.Collections.Generic;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kC
{
	public sealed class Dmc4kCAdapterRoutingControl : AbstractInputCardAdapterRoutingControl<Dmc4kCAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public Dmc4kCAdapterRoutingControl(Dmc4kCAdapter parent, int id)
			: base(parent, id)
		{
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int input)
		{
			if (input == 1)
				return new ConnectorInfo(input, eConnectionType.Audio | eConnectionType.Video);

			string message = string.Format("No input at address {0}", input);
			throw new ArgumentOutOfRangeException("input", message);
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			yield return GetInput(1);
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int output)
		{
			if (output == 1)
				return new ConnectorInfo(output, eConnectionType.Audio | eConnectionType.Video);

			string message = string.Format("No output at address {0}", output);
			throw new ArgumentOutOfRangeException("output", message);
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			yield return GetOutput(1);
		}

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			if (input != 1)
				throw new ArgumentException(string.Format("{0} only has 1 input", GetType().Name), "input");

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
				case eConnectionType.Audio | eConnectionType.Video:
					yield return GetOutput(1);
					break;

				default:
					throw new ArgumentException("type");
			}
		}

		/// <summary>
		/// Gets the input routed to the given output matching the given type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Type has multiple flags.</exception>
		public override ConnectorInfo? GetInput(int output, eConnectionType type)
		{
			if (output != 1)
				throw new ArgumentException(string.Format("{0} only has 1 output", GetType().Name), "output");

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
				case eConnectionType.Audio | eConnectionType.Video:
					return GetInput(1);

				default:
					throw new ArgumentException("type");
			}
		}
	}
}

#endif
