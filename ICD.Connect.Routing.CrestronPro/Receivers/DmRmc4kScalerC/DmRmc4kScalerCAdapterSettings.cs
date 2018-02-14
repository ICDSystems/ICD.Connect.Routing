using System;
using ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc4kScalerC
{
	/// <summary>
	/// Settings for the DmRmcScalerCAdapter.
	/// </summary>
	[KrangSettings(FACTORY_NAME)]
// ReSharper disable once InconsistentNaming
	public sealed class DmRmc4kScalerCAdapterSettings : AbstractDmRmcScalerCAdapterSettings
	{
		private const string FACTORY_NAME = "DmRmc4kScalerC";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmRmc4kScalerCAdapter); } }
	}
}
