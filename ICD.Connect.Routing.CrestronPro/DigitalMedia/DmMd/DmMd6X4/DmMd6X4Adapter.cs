#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd6XN;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd6X4
{
#if SIMPLSHARP
	public sealed class DmMd6X4Adapter : AbstractDmMd6XNAdapter<DmMd6x4, DmMd6X4AdapterSettings>
	{
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override DmMd6x4 InstantiateSwitcher(DmMd6X4AdapterSettings settings)
		{
			return settings.Ipid == null
				       ? null
				       : new DmMd6x4(settings.Ipid.Value, ProgramInfo.ControlSystem);
		}
	}
#else
	public sealed class DmMd6X4Adapter : AbstractDmMd6XNAdapter<DmMd6X4AdapterSettings>
	{
	}
#endif
}
