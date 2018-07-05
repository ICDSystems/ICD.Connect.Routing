#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
#endif
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMdxxxCe;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd300Ce
{
#if SIMPLSHARP
	public sealed class HdMd300CeAdapter : AbstractHdMdxxxCeAdapter<HdMd300CE, HdMd300CeAdapterSettings>
#else
	public sealed class HdMd300CeAdapter : AbstractHdMdxxxCeAdapter<HdMd300CeAdapterSettings>
#endif
	{
#if SIMPLSHARP
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override HdMd300CE InstantiateSwitcher(HdMd300CeAdapterSettings settings)
		{
			return settings.Ipid == null
					   ? null
					   : new HdMd300CE(settings.Ipid.Value, settings.Address, ProgramInfo.ControlSystem);
		}
#endif
	}
}
