#if SIMPLSHARP
using System;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Common.Properties;
using ICD.Common.Services;
using ICD.Common.Services.Logging;
using ICD.Connect.Devices.Extensions;
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Routing.CrestronPro.DigitalMedia;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.Utils
{
	/// <summary>
	/// Provides a centralized collection of tools for instantiating DM devices.
	/// </summary>
	public static class DmEndpointFactoryUtils
	{
		private static ILoggerService Logger { get { return ServiceProvider.GetService<ILoggerService>(); } }

		/// <summary>
		/// Determines the best way to instantiate a card based on the available information.
		/// Instantiates via parent Switch if specified, otherwise uses the ControlSystem.
		/// </summary>
		/// <typeparam name="TCard"></typeparam>
		/// <param name="cresnetId"></param>
		/// <param name="cardNumber"></param>
		/// <param name="switcherId"></param>
		/// <param name="factory"></param>
		/// <param name="instantiateExternal"></param>
		/// <param name="instantiateInternal"></param>
		/// <returns></returns>
		[CanBeNull]
		public static TCard InstantiateCard<TCard>(byte? cresnetId, int? cardNumber, int? switcherId,
												   IDeviceFactory factory,
												   Func<byte, CrestronControlSystem, TCard> instantiateExternal,
												   Func<int, Switch, TCard> instantiateInternal)
		{
			if (switcherId == null)
			{
				if (cresnetId == null)
					Logger.AddEntry(eSeverity.Error, "Failed to instantiate {0} - no CresnetID", typeof(TCard).Name);
				else
					return instantiateExternal((byte)cresnetId, ProgramInfo.ControlSystem);
			}
			else
			{
				if (cardNumber == null)
				{
					Logger.AddEntry(eSeverity.Error, "Failed to instantiate {0} - no DM input address", typeof(TCard).Name);
				}
				else
				{
					IDmSwitcherAdapter switcher = factory.GetOriginatorById<IDmSwitcherAdapter>((int)switcherId);

					if (switcher == null)
					{
						Logger.AddEntry(eSeverity.Error, "Failed to instantiate {0} - Device {1} is not a {2}",
										typeof(TCard).Name, switcherId, typeof(IDmSwitcherAdapter).Name);
					}
					else
					{
						return instantiateInternal((int)cardNumber, switcher.Switcher);
					}
				}
			}

			return default(TCard);
		}

		/// <summary>
		/// Determines the best way to instantiate a DMEndpoint based on the available information.
		/// Instantiates via parent DM Switch if specified, otherwise uses the ControlSystem.
		/// </summary>
		/// <typeparam name="TEndpoint"></typeparam>
		/// <param name="ipid"></param>
		/// <param name="dmAddress"></param>
		/// <param name="dmSwitchId"></param>
		/// <param name="factory"></param>
		/// <param name="instantiate1">Instantiate via control system with specified IPID</param>
		/// <param name="instantiate2">Instantiate via DMInput with specified IPID</param>
		/// <param name="instantiate3">Instantiate via DMInput</param>
		/// <returns></returns>
		[CanBeNull]
		public static TEndpoint InstantiateEndpoint<TEndpoint>(byte? ipid, int? dmAddress, int? dmSwitchId,
		                                                       IDeviceFactory factory,
		                                                       Func<byte, CrestronControlSystem, TEndpoint> instantiate1,
		                                                       Func<byte, DMInput, TEndpoint> instantiate2,
		                                                       Func<DMInput, TEndpoint> instantiate3)
		{
			if (dmSwitchId == null)
			{
				if (ipid == null)
					Logger.AddEntry(eSeverity.Error, "Failed to instantiate {0} - no IPID", typeof(TEndpoint).Name);
				else
					return instantiate1((byte)ipid, ProgramInfo.ControlSystem);
			}
			else
			{
				if (dmAddress == null)
				{
					Logger.AddEntry(eSeverity.Error, "Failed to instantiate {0} - no DM input address", typeof(TEndpoint).Name);
				}
				else
				{
					IDmParent provider = factory.GetDeviceById((int)dmSwitchId) as IDmParent;
					if (provider == null)
					{
						Logger.AddEntry(eSeverity.Error, "Failed to instantiate {0} - Device {1} is not a {2}",
						                typeof(TEndpoint).Name, dmSwitchId, typeof(IDmParent).Name);
					}
					else
					{
						DMInput input = provider.GetDmInput((int)dmAddress);
						return ipid == null ? instantiate3(input) : instantiate2((byte)ipid, input);
					}
				}
			}

			return default(TEndpoint);
		}

		/// <summary>
		/// Determines the best way to instantiate a DMEndpoint based on the available information.
		/// Instantiates via parent DM Switch if specified, otherwise uses the ControlSystem.
		/// </summary>
		/// <typeparam name="TEndpoint"></typeparam>
		/// <param name="ipid"></param>
		/// <param name="dmAddress"></param>
		/// <param name="dmSwitchId"></param>
		/// <param name="factory"></param>
		/// <param name="instantiate1">Instantiate via control system with specified IPID</param>
		/// <param name="instantiate2">Instantiate via DMOutput with specified IPID</param>
		/// <param name="instantiate3">Instantiate via DMOutput</param>
		/// <returns></returns>
		[CanBeNull]
		public static TEndpoint InstantiateEndpoint<TEndpoint>(byte? ipid, int? dmAddress, int? dmSwitchId,
		                                                       IDeviceFactory factory,
		                                                       Func<byte, CrestronControlSystem, TEndpoint> instantiate1,
		                                                       Func<byte, DMOutput, TEndpoint> instantiate2,
		                                                       Func<DMOutput, TEndpoint> instantiate3)
		{
			if (dmSwitchId == null)
			{
				if (ipid == null)
					Logger.AddEntry(eSeverity.Error, "Failed to instantiate {0} - no IPID", typeof(TEndpoint).Name);
				else
					return instantiate1((byte)ipid, ProgramInfo.ControlSystem);
			}
			else
			{
				if (dmAddress == null)
				{
					Logger.AddEntry(eSeverity.Error, "Failed to instantiate {0} - no DM output address", typeof(TEndpoint).Name);
				}
				else
				{
					IDmParent provider = factory.GetDeviceById((int)dmSwitchId) as IDmParent;
					if (provider == null)
					{
						Logger.AddEntry(eSeverity.Error, "Failed to instantiate {0} - Device {1} is not a {2}",
						                typeof(TEndpoint).Name, dmSwitchId, typeof(IDmParent).Name);
					}
					else
					{
						DMOutput output = provider.GetDmOutput((int)dmAddress);
						return ipid == null ? instantiate3(output) : instantiate2((byte)ipid, output);
					}
				}
			}

			return default(TEndpoint);
		}
	}
}
#endif
