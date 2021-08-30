#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd.DmMd6XN;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd.DmMd6X6
{
#if !NETSTANDARD
	public sealed class DmMd6X6Adapter : AbstractDmMd6XNAdapter<DmMd6x6, DmMd6X6AdapterSettings>
	{
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override DmMd6x6 InstantiateSwitcher(DmMd6X6AdapterSettings settings)
		{
			return settings.Ipid == null
				       ? null
				       : new DmMd6x6(settings.Ipid.Value, ProgramInfo.ControlSystem);
		}
	}
#else
	public sealed class DmMd6X6Adapter : AbstractDmMd6XNAdapter<DmMd6X6AdapterSettings>
	{
	}
#endif
}
