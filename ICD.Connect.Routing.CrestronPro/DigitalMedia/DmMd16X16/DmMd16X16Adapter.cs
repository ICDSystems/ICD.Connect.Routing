using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd16X16
{
	public sealed class DmMd16X16Adapter : AbstractDmMdMNXNAdapter<DmMd16x16, DmMd16X16AdapterSettings>
	{
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ipid"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override DmMd16x16 InstantiateSwitcher(ushort ipid, CrestronControlSystem controlSystem)
		{
			return new DmMd16x16(ipid, controlSystem);
		}
	}
}
