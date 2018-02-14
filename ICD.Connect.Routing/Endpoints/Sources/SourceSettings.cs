using System;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Endpoints.Sources
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class SourceSettings : AbstractSourceSettings
	{
		private const string FACTORY_NAME = "Source";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(Source); } }
	}
}
