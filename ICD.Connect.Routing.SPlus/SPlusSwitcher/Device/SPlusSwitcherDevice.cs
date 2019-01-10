using System;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.Simpl;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.SPlus.SPlusSwitcher.Controls;
using ICD.Connect.Routing.SPlus.SPlusSwitcher.EventArgs;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.SPlus.SPlusSwitcher.Device
{
	public sealed class SPlusSwitcherDevice : AbstractSimplDevice<SPlusSwitcherDeviceSettings>, ISPlusSwitcher
	{
		private const int SWITCHER_CONTROL_ID = 0;

		private SPlusSwitcherControl m_SwitcherControl;

		#region Shim

		#region Events to Shim

		/// <summary>
		/// Event raised when the device want the shim to set a route
		/// </summary>
		public event EventHandler<SetRouteApiEventArgs> OnSetRoute;


		/// <summary>
		/// Event raised when the device wants the shim to clear a route
		/// </summary>
		public event EventHandler<ClearRouteApiEventArgs> OnClearRoute;

		#endregion

		#region Methods from Shim

		/// <summary>
		/// From Shim, sets the signal detection state on a input/type
		/// </summary>
		/// <param name="input"></param>
		/// <param name="state"></param>
		public void SetSignalDetectedStateFeedback(int input, bool state)
		{
			if (m_SwitcherControl != null)
				m_SwitcherControl.SetSignalDetectedState(input, state);
		}

		/// <summary>
		/// Sets the routed input on an output
		/// </summary>
		/// <param name="output"></param>
		/// <param name="input"></param>
		/// <param name="type"></param>
		public void SetInputForOutputFeedback(int output, int? input, eConnectionType type)
		{
			if (m_SwitcherControl != null)
				m_SwitcherControl.SetInputForOutput(output, input, type);
		}

		/// <summary>
		/// Clears the switcher cache, so it can be re-built from scratch.
		/// </summary>
		public void ClearCache()
		{
			if (m_SwitcherControl != null)
				m_SwitcherControl.ClearCache();
		}

		#endregion

		#endregion


		#region Switcher Control

		#region Methods Called from Control

		internal bool Route(int output, int input, eConnectionType type)
		{
			OnSetRoute.Raise(this, new SetRouteApiEventArgs(output, input, type));
			// todo: better return value here
			return true;
		}

		internal bool ClearOutput(int output, eConnectionType type)
		{
			OnClearRoute.Raise(this, new ClearRouteApiEventArgs(output, type));
			// todo: better return value here
			return true;
		}

		#endregion

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(SPlusSwitcherDeviceSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_SwitcherControl = new SPlusSwitcherControl(this, SWITCHER_CONTROL_ID, settings.InputCount, settings.OutputCount,
			                                             settings.SwitcherLayers, settings.SupportsSourceDetection);

			Controls.Add(m_SwitcherControl);
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(SPlusSwitcherDeviceSettings settings)
		{
			base.CopySettingsFinal(settings);

			if (m_SwitcherControl == null)
			{
				throw new InvalidOperationException("Cannot copy settings without switcher control");
			}

			settings.InputCount = m_SwitcherControl.InputCount;
			settings.OutputCount = m_SwitcherControl.OutputCount;
			settings.SwitcherLayers = m_SwitcherControl.SwitcherLayers;
			settings.SupportsSourceDetection = m_SwitcherControl.SupportsSourceDetection;
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			if (m_SwitcherControl == null)
				return;

			Controls.Remove(SWITCHER_CONTROL_ID);
			m_SwitcherControl.Dispose();
			m_SwitcherControl = null;

		}

		#endregion

	}
}
