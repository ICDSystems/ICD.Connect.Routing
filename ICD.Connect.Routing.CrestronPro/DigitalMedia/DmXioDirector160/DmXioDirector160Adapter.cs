using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmXioDirectorBase;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmXioDirector160
{
#if SIMPLSHARP
	public sealed class DmXioDirector160Adapter :
		AbstractDmXioDirectorBaseAdapter
			<Crestron.SimplSharpPro.DM.Streaming.DmXioDirector160, DmXioDirector160AdapterSettings>
#else
	public sealed class DmXioDirector160Adapter : AbstractDmXioDirectorBaseAdapter<DmXioDirector160AdapterSettings>
#endif
	{
#if SIMPLSHARP
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
