using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd4xX4kzE;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd4x24kzE
{
	/// <summary>
	/// HdMd4x24kzEAdapter wraps a HdMd4x24kzE to provide a routing device.
	/// </summary>
#if SIMPLSHARP
// ReSharper disable once InconsistentNaming
	public sealed class HdMd4x24kzEAdapter : AbstractHdMd4xX4kzEAdapter<Crestron.SimplSharpPro.DM.HdMd4x24kzE, HdMd4x24kzEAdapterSettings>
#else
	// ReSharper disable once InconsistentNaming
	public sealed class HdMd4x24kzEAdapter : AbstractHdMd4xX4kzEAdapter<HdMd4x24kzEAdapterSettings>
#endif
	{
#if SIMPLSHARP
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.HdMd4x24kzE InstantiateSwitcher(HdMd4x24kzEAdapterSettings settings)
		{
			return settings.Ipid == null
				       ? null
				       : new Crestron.SimplSharpPro.DM.HdMd4x24kzE(settings.Ipid.Value, ProgramInfo.ControlSystem);
		}
#endif
	}
}