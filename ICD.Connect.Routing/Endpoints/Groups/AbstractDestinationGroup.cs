using System.Collections.Generic;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Endpoints.Groups
{
	public abstract class AbstractDestinationGroup<TSettings> : AbstractOriginator<TSettings>, IDestinationGroup
		where TSettings : AbstractDestinationGroupSettings, new()
	{
		public IEnumerable<int> Destinations { get; set; }

		public bool Remote { get; set; }

		public int Order { get; set; }

		public bool Disable { get; set; }

		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			Destinations = new List<int>();
			Order = int.MaxValue;
			Disable = false;
		}

		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Destinations = Destinations;
			settings.Order = Order;
			settings.Disable = Disable;
		}

		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			Destinations = settings.Destinations;
			Order = settings.Order;
			Disable = settings.Disable;
		}
	}
}
