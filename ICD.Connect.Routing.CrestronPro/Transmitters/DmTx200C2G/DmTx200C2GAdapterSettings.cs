using System;
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200C2G
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class DmTx200C2GAdapterSettings : AbstractDmTx200BaseAdapterSettings
	{
		private const string FACTORY_NAME = "DmTx200C2G";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmTx200C2GAdapter); } }
	}
}
