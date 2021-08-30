#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMdxxxCe;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd400Ce
{
#if !NETSTANDARD
	public sealed class HdMd400CeAdapter : AbstractHdMdxxxCeAdapter<HdMd400CE, HdMd400CeAdapterSettings>
#else
	public sealed class HdMd400CeAdapter : AbstractHdMdxxxCeAdapter<HdMd400CeAdapterSettings>
#endif
	{
#if !NETSTANDARD
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override HdMd400CE InstantiateSwitcher(HdMd400CeAdapterSettings settings)
		{
			return settings.Ipid == null
				       ? null
				       : new HdMd400CE(settings.Ipid.Value, settings.Address, ProgramInfo.ControlSystem);
		}
#endif
	}
}
