using System.Collections.Generic;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Utils;

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
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Gets the input for the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs(int output, eConnectionType type)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Update the cache with the current state of the card.
		/// </summary>
		/// <param name="cache"></param>
		protected override void UpdateCache(SwitcherCache cache)
		{
			throw new System.NotImplementedException();
		}
	}
}