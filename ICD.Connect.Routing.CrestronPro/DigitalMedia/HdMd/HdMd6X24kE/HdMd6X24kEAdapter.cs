#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMdNXM;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd6X24kE
{
	/// <summary>
	/// HdMd6X24kEAdapter wraps a HdMd6X24kE to provide a routing device.
	/// </summary>
#if !NETSTANDARD
	public sealed class HdMd6X24kEAdapter : AbstractHdMdNXMAdapter<HdMd6x24kE, HdMd6X24kEAdapterSettings>
#else
	public sealed class HdMd6X24kEAdapter : AbstractHdMdNXMAdapter<HdMd6X24kEAdapterSettings>
#endif
	{
#if !NETSTANDARD
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override HdMd6x24kE InstantiateSwitcher(HdMd6X24kEAdapterSettings settings)
		{
			return settings.Ipid == null
				       ? null
					   : new HdMd6x24kE(settings.Ipid.Value, settings.Address, ProgramInfo.ControlSystem);
		}
#endif
	}
}
