using System;
using ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc4kScalerCDsp
{
	/// <summary>
	/// Settings for the DmRmc4kScalerCDspAdapter.
	/// </summary>
	[KrangSettings(FACTORY_NAME)]
// ReSharper disable once InconsistentNaming
	public sealed class DmRmc4kScalerCDspAdapterSettings : AbstractDmRmcScalerCBaseAdapterSettings
	{
		private const string FACTORY_NAME = "DmRmc4kScalerCDsp";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmRmc4kScalerCDspAdapter); } }
	}
}
