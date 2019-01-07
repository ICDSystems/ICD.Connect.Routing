using System;
using ICD.Common.Properties;
using ICD.Connect.API.EventArguments;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.SPlus.SPlusSwitcher.Proxy;

namespace ICD.Connect.Routing.SPlus.SPlusSwitcher.EventArgs
{
	public sealed class ClearRouteEventArgsData
	{
		[UsedImplicitly]
		public int Output { get; set; }

		[UsedImplicitly]
		public eConnectionType Type { get; set; }

		[UsedImplicitly]
		public ClearRouteEventArgsData()
		{
		}

		public ClearRouteEventArgsData(int output, eConnectionType type)
		{
			Output = output;
			Type = type;
		}
	}

	public sealed class ClearRouteApiEventArgs : AbstractGenericApiEventArgs<ClearRouteEventArgsData>
	{

		public  int Output { get { return Data.Output; }}

		public eConnectionType Type { get { return Data.Type; }}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="data"></param>
		public ClearRouteApiEventArgs(ClearRouteEventArgsData data) : base(SPlusSwitcherApi.EVENT_CLEAR_ROUTE, data)
		{
		}

		public ClearRouteApiEventArgs(int output, eConnectionType type)
			: base(SPlusSwitcherApi.EVENT_CLEAR_ROUTE, new ClearRouteEventArgsData(output, type))
		{
			
		}
	}
}