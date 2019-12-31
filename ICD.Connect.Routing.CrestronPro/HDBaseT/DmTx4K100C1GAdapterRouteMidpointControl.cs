#if SIMPLSHARP
using System;
using ICD.Connect.Routing.Connections;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;

namespace ICD.Connect.Routing.CrestronPro.HDBaseT
{
	public sealed class DmTx4K100C1GAdapterRouteMidpointControl : AbstractDmBasedTEndPointAdapterRouteMidpointControl<DmTx4K100C1GAdapter, DmTx4K100C1G>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmTx4K100C1GAdapterRouteMidpointControl(DmTx4K100C1GAdapter parent, int id)
			: base(parent, id)
		{
		}

		#region Methods

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int input)
		{
			if (!ContainsInput(input))
				throw new ArgumentOutOfRangeException("input");

			return new ConnectorInfo(input, eConnectionType.Audio | eConnectionType.Video);
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int address)
		{
			if (!ContainsOutput(address))
				throw new ArgumentOutOfRangeException("address");

			return new ConnectorInfo(address, eConnectionType.Audio | eConnectionType.Video);
		}

		#endregion
	}
}
#endif