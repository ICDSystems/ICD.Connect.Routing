#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMdNXM;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd4X14kE
{
	/// <summary>
	/// HdMd4X14kEAdapter wraps a HdMd4X14kE to provide a routing device.
	/// </summary>
#if SIMPLSHARP
	public sealed class HdMd4X14kEAdapter : AbstractHdMdNXMAdapter<HdMd4x14kE, HdMd4X14kEAdapterSettings>
#else
	public sealed class HdMd4X14kEAdapter : AbstractHdMdNXMAdapter<HdMd4X14kEAdapterSettings>
#endif
	{
#if SIMPLSHARP
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override HdMd4x14kE InstantiateSwitcher(HdMd4X14kEAdapterSettings settings)
		{
			return settings.Ipid == null
				       ? null
					   : new HdMd4x14kE(settings.Ipid.Value, settings.Address, ProgramInfo.ControlSystem);
		}
#endif
	}
}
