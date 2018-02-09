using System;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.RoutingGraphs
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class RoutingGraphSettings : AbstractRoutingGraphSettings
	{
		private const string FACTORY_NAME = "RoutingGraph";

		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(RoutingGraph); } }
	}
}
