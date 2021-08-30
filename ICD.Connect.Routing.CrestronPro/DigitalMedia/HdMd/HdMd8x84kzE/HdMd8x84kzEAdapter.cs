using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd8xX4kzE;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd8x84kzE
{
	/// <summary>
	/// HdMd8x84kzEAdapter wraps a HdMd8x84kzE to provide a routing device.
	/// </summary>
#if !NETSTANDARD
// ReSharper disable once InconsistentNaming
	public sealed class HdMd8x84kzEAdapter : AbstractHdMd8xX4kzEAdapter<Crestron.SimplSharpPro.DM.HdMd8x84kzE, HdMd8x84kzEAdapterSettings>
#else
	// ReSharper disable once InconsistentNaming
	public sealed class HdMd8x84kzEAdapter : AbstractHdMd8xX4kzEAdapter<HdMd8x84kzEAdapterSettings>
#endif
	{
#if !NETSTANDARD
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.HdMd8x84kzE InstantiateSwitcher(HdMd8x84kzEAdapterSettings settings)
		{
			return settings.Ipid == null
				       ? null
				       : new Crestron.SimplSharpPro.DM.HdMd8x84kzE(settings.Ipid.Value, ProgramInfo.ControlSystem);
		}
#endif
	}
}