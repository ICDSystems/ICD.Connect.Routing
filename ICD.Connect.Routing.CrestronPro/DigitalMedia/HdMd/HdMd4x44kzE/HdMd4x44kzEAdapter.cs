using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd8xX4kzE;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd4x44kzE
{
	/// <summary>
	/// HdMd4x44kzEAdapter wraps a HdMd4x44kzE to provide a routing device.
	/// </summary>
#if !NETSTANDARD
// ReSharper disable once InconsistentNaming
	public sealed class HdMd4x44kzEAdapter : AbstractHdMd8xX4kzEAdapter<Crestron.SimplSharpPro.DM.HdMd4x44kzE, HdMd4x44kzEAdapterSettings>
#else
	// ReSharper disable once InconsistentNaming
	public sealed class HdMd4x44kzEAdapter : AbstractHdMd8xX4kzEAdapter<HdMd4x44kzEAdapterSettings>
#endif
	{
#if !NETSTANDARD
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.HdMd4x44kzE InstantiateSwitcher(HdMd4x44kzEAdapterSettings settings)
		{
			return settings.Ipid == null
				       ? null
				       : new Crestron.SimplSharpPro.DM.HdMd4x44kzE(settings.Ipid.Value, ProgramInfo.ControlSystem);
		}
#endif
	}
}