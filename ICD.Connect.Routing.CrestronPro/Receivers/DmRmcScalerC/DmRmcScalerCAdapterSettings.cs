using System;
using ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerC
{
	/// <summary>
	/// Settings for the DmRmcScalerCAdapter.
	/// </summary>
	[KrangSettings(FACTORY_NAME)]
	public sealed class DmRmcScalerCAdapterSettings : AbstractDmRmcScalerCBaseAdapterSettings
	{
		private const string FACTORY_NAME = "DmRmcScalerC";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmRmcScalerCAdapter); } }
	}
}
