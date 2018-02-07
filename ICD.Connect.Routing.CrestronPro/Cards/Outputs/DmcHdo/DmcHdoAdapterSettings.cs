﻿using System;
using ICD.Common.Properties;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.DmcHdo
{
	public sealed class DmcHdoAdapterSettings : AbstractOutputCardSettings
	{
		private const string FACTORY_NAME = "DmcHdo";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmcHdoAdapter); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static DmcHdoAdapterSettings FromXml(string xml)
		{
			DmcHdoAdapterSettings output = new DmcHdoAdapterSettings();
			output.ParseXml(xml);
			return output;
		}
	}
}
