using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd8xX4kzE;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd8x44kzE
{
	/// <summary>
	/// HdMd8x44kzEAdapter wraps a HdMd8x44kzE to provide a routing device.
	/// </summary>
#if !NETSTANDARD
// ReSharper disable once InconsistentNaming
	public sealed class HdMd8x44kzEAdapter : AbstractHdMd8xX4kzEAdapter<Crestron.SimplSharpPro.DM.HdMd8x44kzE, HdMd8x44kzEAdapterSettings>
#else
	// ReSharper disable once InconsistentNaming
	public sealed class HdMd8x44kzEAdapter : AbstractHdMd8xX4kzEAdapter<HdMd8x44kzEAdapterSettings>
#endif
	{
#if !NETSTANDARD
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.HdMd8x44kzE InstantiateSwitcher(HdMd8x44kzEAdapterSettings settings)
		{
			return settings.Ipid == null
				       ? null
				       : new Crestron.SimplSharpPro.DM.HdMd8x44kzE(settings.Ipid.Value, ProgramInfo.ControlSystem);
		}
#endif
	}
}