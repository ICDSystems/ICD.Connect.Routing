using System;
using ICD.Connect.API.Attributes;
using ICD.Connect.Devices.Simpl;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.SPlus.SPlusSwitcher.EventArgs;
using ICD.Connect.Routing.SPlus.SPlusSwitcher.Proxy;
using ICD.Connect.Routing.SPlus.SPlusSwitcher.State;

namespace ICD.Connect.Routing.SPlus.SPlusSwitcher
{
	public interface ISPlusSwitcher : ISimplDevice
	{

		#region Events
		/// <summary>
		/// Event raised when the device wants the shim to set a route
		/// </summary>
		[ApiEvent(SPlusSwitcherApi.EVENT_SET_ROUTE, SPlusSwitcherApi.EVENT_SET_ROUTE_HELP)]
		event EventHandler<SetRouteApiEventArgs> OnSetRoute;

		/// <summary>
		/// Event raised when the device wants the shim to clear a route
		/// </summary>
		[ApiEvent(SPlusSwitcherApi.EVENT_CLEAR_ROUTE, SPlusSwitcherApi.EVENT_CLEAR_ROUTE_HELP)]
		event EventHandler<ClearRouteApiEventArgs> OnClearRoute;

		#endregion

		#region Methods

		/// <summary>
		/// Sets the signal detection state on a input/type
		/// </summary>
		/// <param name="input"></param>
		/// <param name="state"></param>
		[ApiMethod(SPlusSwitcherApi.METHOD_SET_SIGNAL_DETECTED_STATE_FEEDBACK, SPlusSwitcherApi.METHOD_SET_SIGNAL_DETECTED_STATE_FEEDBACK_HELP)]
		void SetSignalDetectedStateFeedback(int input, bool state);

		/// <summary>
		/// Sets the routed input on an output
		/// </summary>
		/// <param name="output"></param>
		/// <param name="input"></param>
		/// <param name="type"></param>
		[ApiMethod(SPlusSwitcherApi.METHOD_SET_INPUT_FOR_OUTPUT_FEEDBACK, SPlusSwitcherApi.METHOD_SET_INPUT_FOR_OUTPUT_FEEDBACK_HELP)]
		void SetInputForOutputFeedback(int output, int? input, eConnectionType type);

		/// <summary>
		/// Clears the switcher cache, so it can be re-built from scratch.
		/// </summary>
		[ApiMethod(SPlusSwitcherApi.METHOD_CLEAR_CACHE, SPlusSwitcherApi.METHOD_CLEAR_CACHE_HELP)]
		void ClearCache();

		[ApiMethod(SPlusSwitcherApi.METHOD_SET_STATE, SPlusSwitcherApi.METHOD_SET_STATE_HELP)]
		void SetState(SPlusSwitcherState state);

		#endregion

	}
}