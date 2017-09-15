#if SIMPLSHARP
using System;
using System.Collections.Generic;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcC
{
	public sealed class DmcCAdapterRoutingControl : AbstractInputCardAdapterRoutingControl<DmcCAdapter>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmcCAdapterRoutingControl(DmcCAdapter parent, int id)
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
			return new ConnectorInfo(1, eConnectionType.Audio | eConnectionType.Video);
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
				throw new ArgumentOutOfRangeException("output");

			yield return GetInput(1);
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int input)
		{
			return new ConnectorInfo(1, eConnectionType.Audio | eConnectionType.Video);
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
		/// Update the cache with the current state of the card.
		/// </summary>
		/// <param name="cache"></param>
		protected override void UpdateCache(SwitcherCache cache)
		{
			throw new NotImplementedException();
		}
	}
}
#endif
