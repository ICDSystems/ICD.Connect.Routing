using System;
using ICD.Common.Properties;
using ICD.Connect.API.EventArguments;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.SPlus.SPlusSwitcher.Proxy;

namespace ICD.Connect.Routing.SPlus.SPlusSwitcher.EventArgs
{
	[Serializable]
	public sealed class SetRouteEventArgsData
	{
		[UsedImplicitly]
		public int Output { get; set; }
		
		[UsedImplicitly]
		public int Input { get; set; }

		[UsedImplicitly]
		public eConnectionType Type { get; set; }

		[UsedImplicitly]
		public SetRouteEventArgsData()
		{
		}

		public SetRouteEventArgsData(int output, int input, eConnectionType type)
		{
			Output = output;
			Input = input;
			Type = type;
		}
	}

	public sealed class SetRouteApiEventArgs : AbstractGenericApiEventArgs<SetRouteEventArgsData>
	{

		public int Output {get { return Data.Output; }}
		public int Input {get { return Data.Input; }}
		public eConnectionType Type { get { return Data.Type; }}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="data"></param>
		public SetRouteApiEventArgs(SetRouteEventArgsData data) : base(SPlusSwitcherApi.EVENT_SET_ROUTE, data)
		{
		}

		public SetRouteApiEventArgs(int output, int input, eConnectionType type)
			: base(SPlusSwitcherApi.EVENT_SET_ROUTE, new SetRouteEventArgsData(output, input, type))
		{
		}
	}
}