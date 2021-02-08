using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd4xX4kzE;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd4x14kzE
{
	/// <summary>
	/// HdMd4x14kzEAdapter wraps a HdMd4x14kzE to provide a routing device.
	/// </summary>
#if SIMPLSHARP
// ReSharper disable once InconsistentNaming
	public sealed class HdMd4x14kzEAdapter : AbstractHdMd4xX4kzEAdapter<Crestron.SimplSharpPro.DM.HdMd4x14kzE, HdMd4x14kzEAdapterSettings>
#else
	// ReSharper disable once InconsistentNaming
	public sealed class HdMd4x14kzEAdapter : AbstractHdMd4xX4kzEAdapter<HdMd4x14kzEAdapterSettings>
#endif
	{
#if SIMPLSHARP
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.HdMd4x14kzE InstantiateSwitcher(HdMd4x14kzEAdapterSettings settings)
		{
			return settings.Ipid == null
				       ? null
				       : new Crestron.SimplSharpPro.DM.HdMd4x14kzE(settings.Ipid.Value, ProgramInfo.ControlSystem);
		}
#endif
	}
}