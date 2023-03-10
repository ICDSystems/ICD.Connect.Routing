using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmXio.DmXioDirectorBase;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmXio.DmXioDirectorEnterprise
{
#if !NETSTANDARD
	public sealed class DmXioDirectorEnterpriseAdapter :
		AbstractDmXioDirectorBaseAdapter
			<Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorEnterprise, DmXioDirectorEnterpriseAdapterSettings>
#else
	public sealed class DmXioDirectorEnterpriseAdapter : AbstractDmXioDirectorBaseAdapter<DmXioDirectorEnterpriseAdapterSettings>
#endif
	{
#if !NETSTANDARD
		/// <summary>
		/// Creates a new instance of the wrapped internal director.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorEnterprise InstantiateDirector(
			DmXioDirectorEnterpriseAdapterSettings settings)
		{
			return settings.EthernetId.HasValue
				       ? new Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorEnterprise(settings.EthernetId.Value,
				                                                                         ProgramInfo.ControlSystem)
				       : null;
		}
#endif
	}
}
