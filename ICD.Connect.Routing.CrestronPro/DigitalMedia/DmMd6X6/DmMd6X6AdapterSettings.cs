﻿using System;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd6XN;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd6X6
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class DmMd6X6AdapterSettings : AbstractDmMd6XNAdapterSettings
	{
		private const string FACTORY_NAME = "DmMd6X6";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmMd6X6Adapter); } }
	}
}