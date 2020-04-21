using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Endpoints.Destinations
{
	public abstract class AbstractDestination<TSettings> : AbstractSourceDestinationCommon<TSettings>, IDestination
		where TSettings : IDestinationSettings, new()
	{
		#region Properties

		/// <summary>
		/// Gets the category for this originator type (e.g. Device, Port, etc)
		/// </summary>
		public override string Category { get { return "Destination"; } }

		public string DestinationGroupString { get; private set; }

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the destinations represented by this instance.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IDestination> IDestinationBase.GetDestinations()
		{
			yield return this;
		}

		IEnumerable<IDestination> IDestinationBase.GetDestinations(eConnectionType type)
		{
			if (ConnectionType.HasFlags(type))
				yield return this;
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			DestinationGroupString = settings.DestinationGroupString;
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.DestinationGroupString = DestinationGroupString;
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			DestinationGroupString = null;
		}

		#endregion
	}
}
