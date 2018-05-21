using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.SPlusShims;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings.SPlusShims.GlobalEvents;

namespace ICD.Connect.Routing.SPlus
{
	public delegate ushort SPlusSwitcherShimGetSignalDetectedState(ushort input);

	public delegate ushort SPlusSwitcherShimGetInputCount();

	public delegate ushort SPlusSwitcherShimGetOutputCount();

	public delegate ushort SPlusSwitcherShimGetInputForOutput(ushort output);

	public delegate ushort SPlusSwitcherShimRoute(ushort input, ushort output);

	public delegate ushort SPlusSwitcherShimClearOutput(ushort output);


	[PublicAPI("SPlus")]
	public sealed class SPlusSwitcherShim : AbstractSPlusDeviceShim<SPlusSwitcher>
	{
		#region Callbacks

		[PublicAPI("S+")]
		public SPlusSwitcherShimGetSignalDetectedState GetSignalDetectedStateCallback { get; set; }

		[PublicAPI("S+")]
		public SPlusSwitcherShimGetInputCount GetInputCountCallback { get; set; }

		[PublicAPI("S+")]
		public SPlusSwitcherShimGetOutputCount GetOutputCountCallback { get; set; }

		[PublicAPI("S+")]
		public SPlusSwitcherShimGetInputForOutput GetInputForOutputCallback { get; set; }

		[PublicAPI("S+")]
		public SPlusSwitcherShimRoute RouteCallback { get; set; }

		[PublicAPI("S+")]
		public SPlusSwitcherShimClearOutput ClearOutputCallback { get; set; }

		#endregion

		#region Private Members

		private SPlusSwitcherControl m_Control;

		#endregion

		#region Public Methods

		/// <summary>
		/// Sets the wrapped originator.
		/// </summary>
		/// <param name="id"></param>
		public override void SetOriginator(int id)
		{
			base.SetOriginator(id);

			m_Control = Originator.Controls.GetControl<SPlusSwitcherControl>();
		}

		/// <summary>
		/// Triggers a source detection check for the given input.
		/// </summary>
		/// <param name="input"></param>
		[PublicAPI("SPlus")]
		public void UpdateSourceDetection(ushort input)
		{
			if (m_Control == null)
				return;

			m_Control.UpdateSourceDetection(input, eConnectionType.Video | eConnectionType.Audio);
		}

		/// <summary>
		/// Triggers an input routing check for the given output.
		/// </summary>
		/// <param name="output"></param>
		[PublicAPI("SPlus")]
		public void UpdateSwitcherOutput(ushort output)
		{
			if (m_Control == null)
				return;

			ushort? input = GetInputForOutputCallback == null ? (ushort?)null : GetInputForOutputCallback(output);

			if (input == null)
				m_Control.ClearOutput(output, eConnectionType.Video | eConnectionType.Audio);
			else
				m_Control.SetInputForOutput(output, (int)input, eConnectionType.Video | eConnectionType.Audio);
		}

		/// <summary>
		/// Sets the online state of the current switcher device.
		/// </summary>
		/// <param name="online"></param>
		[PublicAPI("SPlus")]
		public void SetDeviceOnline(ushort online)
		{
			if (m_Control == null)
				return;

			m_Control.Parent.SetOnlineStatus(online != 0);
		}
         
		#endregion

		#region Protected / Private Methods

		/// <summary>
		/// Subscribes to the originator events.
		/// </summary>
		/// <param name="originator"></param>
		protected override void Subscribe(SPlusSwitcher originator)
		{
			base.Subscribe(originator);

			if(originator == null)
				return;

			Subscribe(originator.Controls.GetControl<SPlusSwitcherControl>());
		}

		/// <summary>
		/// Unsubscribes from the originator events.
		/// </summary>
		/// <param name="originator"></param>
		protected override void Unsubscribe(SPlusSwitcher originator)
		{
			base.Unsubscribe(originator);

			if (originator == null)
				return;

			Unsubscribe(originator.Controls.GetControl<SPlusSwitcherControl>());
		}

		protected override void EnvironmentLoaded(EnvironmentLoadedEventInfo environmentLoadedEventInfo)
		{
			base.EnvironmentLoaded(environmentLoadedEventInfo);

			SetOriginator(Originator.Id);
		}

		protected override void EnvironmentUnloaded(EnvironmentUnloadedEventInfo environmentUnloadedEventInfo)
		{
			base.EnvironmentUnloaded(environmentUnloadedEventInfo);

			SetOriginator(0);
		}

		#endregion

		#region Control Callbacks

		/// <summary>
		/// Subscribe to the switcher control events.
		/// </summary>
		/// <param name="switcherControl"></param>
		private void Subscribe(SPlusSwitcherControl switcherControl)
		{
			if (switcherControl == null)
				return;

			switcherControl.GetSignalDetectedStateCallback = SwitcherControlGetSignalDetectedStateCallback;
			switcherControl.GetInputsCallback = SwitcherControlGetInputsCallback;
			switcherControl.GetOutputsCallback = SwitcherControlGetOutputsCallback;
			switcherControl.RouteCallback = SwitcherControlRouteCallback;
			switcherControl.ClearOutputCallback = SwitcherControlClearOutputCallback;
		}

		/// <summary>
		/// Unsubscribe from the switcher control events.
		/// </summary>
		/// <param name="switcherControl"></param>
		private void Unsubscribe(SPlusSwitcherControl switcherControl)
		{
			if (switcherControl == null)
				return;

			switcherControl.GetSignalDetectedStateCallback = null;
			switcherControl.GetInputsCallback = null;
			switcherControl.GetOutputsCallback = null;
			switcherControl.RouteCallback = null;
			switcherControl.ClearOutputCallback = null;
		}

		private bool SwitcherControlClearOutputCallback(int output, eConnectionType type)
		{
			return (ClearOutputCallback((ushort)output) != 0);
		}

		private bool SwitcherControlRouteCallback(RouteOperation info)
		{
			return RouteCallback != null && (RouteCallback((ushort)info.LocalInput, (ushort)info.LocalOutput) != 0);
		}

		private IEnumerable<ConnectorInfo> SwitcherControlGetOutputsCallback()
		{
			int outputCount = GetOutputCountCallback == null ? 0 : GetOutputCountCallback();
			for (int index = 0; index < outputCount; index++)
				yield return new ConnectorInfo(index + 1, eConnectionType.Video | eConnectionType.Audio);
		}

		private IEnumerable<ConnectorInfo> SwitcherControlGetInputsCallback()
		{
			int inputCount = GetInputCountCallback == null ? 0 : GetInputCountCallback();
			for (int index = 0; index < inputCount; index++)
				yield return new ConnectorInfo(index + 1, eConnectionType.Video | eConnectionType.Audio);
		}

		private bool SwitcherControlGetSignalDetectedStateCallback(int input, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .SelectMulti(f => SwitcherControlGetSignalDetectedStateCallback(input, f))
				                .Unanimous(false);
			}

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
					return GetSignalDetectedStateCallback != null && (GetSignalDetectedStateCallback((ushort)input) != 0);
			}

			return false;
		}

		#endregion
	}
}
