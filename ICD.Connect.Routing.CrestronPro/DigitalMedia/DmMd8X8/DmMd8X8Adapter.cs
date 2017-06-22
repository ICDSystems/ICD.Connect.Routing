using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd8X8
{
	public sealed class DmMd8X8Adapter : AbstractDmMdMNXNAdapter<DmMd8x8, DmMd8X8AdapterSettings>
	{
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ipid"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override DmMd8x8 InstantiateSwitcher(ushort ipid, CrestronControlSystem controlSystem)
		{
			return new DmMd8x8(ipid, controlSystem);
		}
	}
}
