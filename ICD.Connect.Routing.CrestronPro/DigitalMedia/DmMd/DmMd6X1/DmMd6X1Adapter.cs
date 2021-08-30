#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd.DmMd6XN;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd.DmMd6X1
{
#if !NETSTANDARD
	public sealed class DmMd6X1Adapter : AbstractDmMd6XNAdapter<DmMd6x1, DmMd6X1AdapterSettings>
	{
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override DmMd6x1 InstantiateSwitcher(DmMd6X1AdapterSettings settings)
		{
			return settings.Ipid == null
				       ? null
				       : new DmMd6x1(settings.Ipid.Value, ProgramInfo.ControlSystem);
		}
	}
#else
	public sealed class DmMd6X1Adapter : AbstractDmMd6XNAdapter<DmMd6X1AdapterSettings>
	{
	}
#endif
}
