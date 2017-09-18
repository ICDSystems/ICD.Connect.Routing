#if SIMPLSHARP
using System;
using System.Collections.Generic;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcHd
{
	public sealed class DmcHdAdapterRoutingControl : AbstractInputCardAdapterRoutingControl<DmcHdAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcHdAdapterRoutingControl(DmcHdAdapter parent, int id)
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
		/// Gets the input for the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs(int output, eConnectionType type)
		{
			if (output != 1)
			{
				string message = string.Format("No output at address {0}", output);
				throw new ArgumentOutOfRangeException("output", message);
			}

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
	}
}
#endif
