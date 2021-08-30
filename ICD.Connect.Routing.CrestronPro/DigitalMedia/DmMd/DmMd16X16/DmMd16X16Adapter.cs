#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd.DmMdMNXN;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd.DmMd16X16
{
#if !NETSTANDARD
	public sealed class DmMd16X16Adapter : AbstractDmMdMNXNAdapter<DmMd16x16, DmMd16X16AdapterSettings>
	{
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override DmMd16x16 InstantiateSwitcher(DmMd16X16AdapterSettings settings)
		{
			return settings.Ipid == null 
				   ? null 
				   : new DmMd16x16(settings.Ipid.Value, ProgramInfo.ControlSystem);
		}
	}
#else
    public sealed class DmMd16X16Adapter : AbstractDmMdMNXNAdapter<DmMd16X16AdapterSettings>
    {
    }
#endif
}
