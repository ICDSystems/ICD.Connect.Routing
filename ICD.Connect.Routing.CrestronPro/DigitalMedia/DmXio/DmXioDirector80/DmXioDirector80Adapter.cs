using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmXio.DmXioDirectorBase;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmXio.DmXioDirector80
{
#if !NETSTANDARD
	public sealed class DmXioDirector80Adapter :
		AbstractDmXioDirectorBaseAdapter
			<Crestron.SimplSharpPro.DM.Streaming.DmXioDirector80, DmXioDirector80AdapterSettings>
#else
	public sealed class DmXioDirector80Adapter : AbstractDmXioDirectorBaseAdapter<DmXioDirector80AdapterSettings>
#endif
	{
#if !NETSTANDARD
		/// <summary>
		/// Creates a new instance of the wrapped internal director.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Streaming.DmXioDirector80 InstantiateDirector(
			DmXioDirector80AdapterSettings settings)
		{
			return settings.EthernetId.HasValue
				       ? new Crestron.SimplSharpPro.DM.Streaming.DmXioDirector80(settings.EthernetId.Value,
				                                                                 ProgramInfo.ControlSystem)
				       : null;
		}
#endif
	}
}
