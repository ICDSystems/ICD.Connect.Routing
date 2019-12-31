#if SIMPLSHARP
using System;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Utils;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

namespace ICD.Connect.Routing.CrestronPro.HDBaseT
{
	public sealed class DmRmc4K100C1GAdapterRouteMidpointControl : AbstractDmBasedTEndPointAdapterRouteMidpointControl<DmRmc4K100C1GAdapter, DmRmc4K100C1G>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public DmRmc4K100C1GAdapterRouteMidpointControl(DmRmc4K100C1GAdapter parent, int id)
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

		protected override void UpdateCache(SwitcherCache cache)
		{
			base.UpdateCache(cache);

			DMInput input = Device == null ? null : Device.DMInputOutput as DMInput;
			bool detected = input != null && input.VideoDetectedFeedback.BoolValue;

			cache.SetSourceDetectedState(1, eConnectionType.Audio | eConnectionType.Video | eConnectionType.Usb, detected);
		}
	}
}
#endif