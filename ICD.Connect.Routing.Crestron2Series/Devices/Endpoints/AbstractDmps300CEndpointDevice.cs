using ICD.Connect.Routing.Crestron2Series.Devices.ControlSystem;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Crestron2Series.Devices.Endpoints
{
	public abstract class AbstractDmps300CEndpointDevice<TSettings> : AbstractDmps300CDevice<TSettings>,
	                                                                  IDmps300CEndpointDevice
		where TSettings : IDmps300CEndpointDeviceSettings, new()
	{
		private Dmps300CControlSystem m_Parent;

		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_Parent = null;
			Address = null;
		}

		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Device = m_Parent == null ? 0 : m_Parent.Id;
		}

		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_Parent = factory.GetOriginatorById<Dmps300CControlSystem>(settings.Device);
			Address = m_Parent.Address;
		}
	}
}
