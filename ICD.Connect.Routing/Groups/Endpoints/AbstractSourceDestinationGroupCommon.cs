using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Groups;

namespace ICD.Connect.Routing.Groups.Endpoints
{
	public abstract class AbstractSourceDestinationGroupCommon<TOriginator, TSettings> :
		AbstractGroup<TOriginator, TSettings>, ISourceDestinationGroupCommon
		where TOriginator : class, ISourceDestinationCommon
		where TSettings : ISourceDestinationGroupCommonSettings, new()
	{
		#region Properties

		/// <summary>
		/// Specifies which media types to use for this source/destination.
		/// </summary>
		public eConnectionType ConnectionType
		{
			get
			{
				eConnectionType output =
					GetItems().Aggregate(eConnectionType.None, (current, item) => current | item.ConnectionType);
				return EnumUtils.GetFlagsIntersection(output, ConnectionTypeMask);
			}
		}

		/// <summary>
		/// Masks the connection types inherited from the group items.
		/// </summary>
		public eConnectionType ConnectionTypeMask { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Gets the devices for this instance.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IDeviceBase> GetDevices()
		{
			return GetItems().SelectMany(i => i.GetDevices())
			                 .Distinct();
		}

		/// <summary>
		/// Gets the endpoints for this instance.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<EndpointInfo> GetEndpoints()
		{
			return GetItems().SelectMany(i => i.GetEndpoints())
			                 .Distinct();
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			ConnectionTypeMask = EnumUtils.GetFlagsAllValue<eConnectionType>();
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.ConnectionTypeMask = ConnectionTypeMask;
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			ConnectionTypeMask = settings.ConnectionTypeMask;
		}

		#endregion
	}
}
