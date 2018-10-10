#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
#endif
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMdxxxCe;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd400Ce
{
#if SIMPLSHARP
	public sealed class HdMd400CeAdapter : AbstractHdMdxxxCeAdapter<HdMd400CE, HdMd400CeAdapterSettings>
#else
	public sealed class HdMd400CeAdapter : AbstractHdMdxxxCeAdapter<HdMd400CeAdapterSettings>
#endif
	{
#if SIMPLSHARP
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
