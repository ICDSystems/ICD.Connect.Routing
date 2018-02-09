using System;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kC
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class Dmc4kCAdapterSettings : AbstractInputCardSettings
	{
		private const string FACTORY_NAME = "Dmc4kC";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(Dmc4kCAdapter); } }
	}
}
