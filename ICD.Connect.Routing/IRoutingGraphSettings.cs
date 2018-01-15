﻿using ICD.Connect.Settings;

namespace ICD.Connect.Routing
{
	public interface IRoutingGraphSettings : ISettings
	{
		#region Properties

		SettingsCollection ConnectionSettings { get; }
		SettingsCollection StaticRouteSettings { get; }
		SettingsCollection SourceSettings { get; }
		SettingsCollection DestinationSettings { get; }
		SettingsCollection DestinationGroupSettings { get; }

		#endregion
	}
}