#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMdNXM;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd4X24kE
{
	/// <summary>
	/// HdMd4X24kEAdapter wraps a HdMd4x24kE to provide a routing device.
	/// </summary>
#if !NETSTANDARD
	public sealed class HdMd4X24kEAdapter : AbstractHdMdNXMAdapter<HdMd4x24kE, HdMd4X24kEAdapterSettings>
#else
	public sealed class HdMd4X24kEAdapter : AbstractHdMdNXMAdapter<HdMd4X24kEAdapterSettings>
#endif
	{
#if !NETSTANDARD
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override HdMd4x24kE InstantiateSwitcher(HdMd4X24kEAdapterSettings settings)
		{
			return settings.Ipid == null
				       ? null
				       : new HdMd4x24kE(settings.Ipid.Value, settings.Address, ProgramInfo.ControlSystem);
		}
#endif
	}
}
