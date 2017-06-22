using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd32X32
{
	public sealed class DmMd32X32Adapter : AbstractDmMdMNXNAdapter<DmMd32x32, DmMd32X32AdapterSettings>
	{
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ipid"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override DmMd32x32 InstantiateSwitcher(ushort ipid, CrestronControlSystem controlSystem)
		{
			return new DmMd32x32(ipid, controlSystem);
		}
	}
}
