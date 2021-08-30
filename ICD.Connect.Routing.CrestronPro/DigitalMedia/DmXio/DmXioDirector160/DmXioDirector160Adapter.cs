using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmXio.DmXioDirectorBase;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmXio.DmXioDirector160
{
#if !NETSTANDARD
	public sealed class DmXioDirector160Adapter :
		AbstractDmXioDirectorBaseAdapter
			<Crestron.SimplSharpPro.DM.Streaming.DmXioDirector160, DmXioDirector160AdapterSettings>
#else
	public sealed class DmXioDirector160Adapter : AbstractDmXioDirectorBaseAdapter<DmXioDirector160AdapterSettings>
#endif
	{
#if !NETSTANDARD
		/// <summary>
		/// Creates a new instance of the wrapped internal director.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Streaming.DmXioDirector160 InstantiateDirector(
			DmXioDirector160AdapterSettings settings)
		{
			return settings.EthernetId.HasValue
				       ? new Crestron.SimplSharpPro.DM.Streaming.DmXioDirector160(settings.EthernetId.Value,
				                                                                  ProgramInfo.ControlSystem)
				       : null;
		}
#endif
	}
}
