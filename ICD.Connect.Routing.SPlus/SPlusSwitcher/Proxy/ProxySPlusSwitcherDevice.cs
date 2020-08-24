using System;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API;
using ICD.Connect.API.Info;
using ICD.Connect.Devices.CrestronSPlus.Devices.SPlus;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.SPlus.SPlusSwitcher.EventArgs;
using ICD.Connect.Routing.SPlus.SPlusSwitcher.State;

namespace ICD.Connect.Routing.SPlus.SPlusSwitcher.Proxy
{
	public sealed class ProxySPlusSwitcherDevice : AbstractSPlusProxyDevice<ProxySPlusSwitcherDeviceSettings>, ISPlusSwitcher
	{

		#region ISPlusSwitcher
		public event EventHandler<SetRouteApiEventArgs> OnSetRoute;
		public event EventHandler<ClearRouteApiEventArgs> OnClearRoute;

		/// <summary>
		/// Sets the signal detection state on a input/type
		/// </summary>
		/// <param name="input"></param>
		/// <param name="state"></param>
		public void SetSignalDetectedStateFeedback(int input, bool state)
		{
			CallMethod(SPlusSwitcherApi.METHOD_SET_SIGNAL_DETECTED_STATE_FEEDBACK, input, state);
		}

		/// <summary>
		/// Sets the routed input on an output
		/// </summary>
		/// <param name="output"></param>
		/// <param name="input"></param>
		/// <param name="type"></param>
		public void SetInputForOutputFeedback(int output, int? input, eConnectionType type)
		{
			CallMethod(SPlusSwitcherApi.METHOD_SET_INPUT_FOR_OUTPUT_FEEDBACK, output, input, type);
		}

		/// <summary>
		/// Clears the switcher cache, so it can be re-built from scratch.
		/// </summary>
		public void ClearCache()
		{
			CallMethod(SPlusSwitcherApi.METHOD_CLEAR_CACHE);
		}

		public void SetState(SPlusSwitcherState state)
		{
			CallMethod(SPlusSwitcherApi.METHOD_SET_STATE, state);
		}

		#endregion

		#region API

		/// <summary>
		/// Override to build initialization commands on top of the current class info.
		/// </summary>
		/// <param name="command"></param>
		protected override void Initialize(ApiClassInfo command)
		{
			base.Initialize(command);

			ApiCommandBuilder.UpdateCommand(command)
							 .SubscribeEvent(SPlusSwitcherApi.EVENT_SET_ROUTE)
							 .SubscribeEvent(SPlusSwitcherApi.EVENT_CLEAR_ROUTE)
							 .Complete();
			RaiseOnRequestShimResync(this);
		}

		/// <summary>
		/// Updates the proxy with event feedback info.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="result"></param>
		protected override void ParseEvent(string name, ApiResult result)
		{
			base.ParseEvent(name, result);

			switch (name)
			{
				case SPlusSwitcherApi.EVENT_SET_ROUTE:
					RaiseSetRoute(result.GetValue<SetRouteEventArgsData>());
					break;
				case SPlusSwitcherApi.EVENT_CLEAR_ROUTE:
					RaiseClearRoute(result.GetValue<ClearRouteEventArgsData>());
					break;
			}
		}

		#endregion

		#region Private Methods

		private void RaiseSetRoute(SetRouteEventArgsData setRouteEventArgsData)
		{
			OnSetRoute.Raise(this, new SetRouteApiEventArgs(setRouteEventArgsData));
		}

		private void RaiseClearRoute(ClearRouteEventArgsData clearRouteEventArgs)
		{
			OnClearRoute.Raise(this, new ClearRouteApiEventArgs(clearRouteEventArgs));
		}

		#endregion
	}
}