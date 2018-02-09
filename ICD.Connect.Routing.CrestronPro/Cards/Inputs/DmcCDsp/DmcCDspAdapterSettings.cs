using System;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcCDsp
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class DmcCDspAdapterSettings : AbstractInputCardSettings
	{
		private const string FACTORY_NAME = "DmcCDsp";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmcCDspAdapter); } }
	}
}
