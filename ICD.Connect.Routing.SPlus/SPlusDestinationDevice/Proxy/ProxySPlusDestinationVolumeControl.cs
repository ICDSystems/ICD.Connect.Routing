using System;
using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Info;
using ICD.Connect.API.Nodes;
using ICD.Connect.Audio.Console.Mute;
using ICD.Connect.Audio.Console.Volume;
using ICD.Connect.Audio.Controls.Mute;
using ICD.Connect.Audio.Controls.Volume;
using ICD.Connect.Audio.EventArguments;
using ICD.Connect.Audio.Proxies.Controls.Mute;
using ICD.Connect.Audio.Proxies.Controls.Volume;
using ICD.Connect.Devices.Proxies.Controls;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Proxy
{
	public sealed class ProxySPlusDestinationVolumeControl : AbstractProxyDeviceControl, IVolumeLevelDeviceControl, IVolumeMuteFeedbackDeviceControl
	{
		private const double TOLERANCE = 0.001;

		#region Events

		public event EventHandler<VolumeDeviceVolumeChangedEventArgs> OnVolumeChanged;
		public event EventHandler<MuteDeviceMuteStateChangedApiEventArgs> OnMuteStateChanged;

		#endregion

		#region Fields

		private bool m_VolumeIsMuted;

		private float? m_MaxVolume;

		private float? m_MinVolume;
		private float m_VolumePercent;
		private string m_VolumeString;
		private float m_VolumeLevel;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the current volume positon, 0 - 1
		/// </summary>
		public float VolumePercent { get { return m_VolumePercent; }
			private set
			{
				if (Math.Abs(m_VolumePercent - value) < TOLERANCE)
					return;

				m_VolumePercent = value;

				RaiseOnVolumeChanged();
			} }

		/// <summary>
		/// Gets the current volume, in string representation
		/// </summary>
		public string VolumeString { get { return m_VolumeString; }
			private set
			{
				if (m_VolumeString == value)
					return;

				m_VolumeString = value;

				RaiseOnVolumeChanged();
			} }

		/// <summary>
		/// Gets the current volume, in the parent device's format
		/// </summary>
		public float VolumeLevel
		{
			get { return m_VolumeLevel; }
			private set
			{
				if (Math.Abs(m_VolumeLevel - value) < TOLERANCE)
					return;

				m_VolumeLevel = value;

				RaiseOnVolumeChanged();
			}
		}

		/// <summary>
		/// VolumeLevelMaxRange is the best max volume we have for the control
		/// either the Max from the control or the absolute max for the control
		/// </summary>
		public float VolumeLevelMax { get { return m_MaxVolume ?? float.MaxValue; } }

		/// <summary>
		/// VolumeLevelMinRange is the best min volume we have for the control
		/// either the Min from the control or the absolute min for the control
		/// </summary>
		public float VolumeLevelMin { get { return m_MinVolume ?? float.MinValue; } }

		/// <summary>
		/// Gets the muted state.
		/// </summary>
		public bool VolumeIsMuted { get { return m_VolumeIsMuted; }
			private set
			{
				if (value == m_VolumeIsMuted)
					return;
				m_VolumeIsMuted = value;
				OnMuteStateChanged.Raise(this, new MuteDeviceMuteStateChangedApiEventArgs(value));
			}}

		#endregion
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public ProxySPlusDestinationVolumeControl(IProxySPlusDestinationDevice parent, int id) : base(parent, id)
		{
		}

		#region Methods

		/// <summary>
		/// Starts lowering the volume in steps of the given position, and continues until RampStop is called.
		/// </summary>
		/// <param name="decrement"></param>
		public void VolumePercentRampDown(float decrement)
		{
			CallMethod(VolumeLevelDeviceControlApi.METHOD_VOLUME_LEVEL_RAMP_PERCENT_DOWN, decrement);
		}

		/// <summary>
		/// Starts raising the volume in steps of the given position, and continues until RampStop is called.
		/// </summary>
		/// <param name="increment"></param>
		public void VolumePercentRampUp(float increment)
		{
			CallMethod(VolumeLevelDeviceControlApi.METHOD_VOLUME_LEVEL_RAMP_PERCENT_UP, increment);
		}

		/// <summary>
		/// Sets the volume position, from 0-1
		/// </summary>
		/// <param name="percent"></param>
		public void SetVolumePercent(float percent)
		{
			CallMethod(VolumeLevelDeviceControlApi.METHOD_SET_VOLUME_PERCENT, percent);
		}

		/// <summary>
		/// Stops any current ramp up/down in progress.
		/// </summary>
		public void VolumeRampStop()
		{
			CallMethod(VolumeRampDeviceControlApi.METHOD_VOLUME_RAMP_STOP);
		}

		/// <summary>
		/// Starts lowering the volume, and continues until RampStop is called.
		/// <see cref="IVolumeRampDeviceControl.VolumeRampStop"/> must be called after
		/// </summary>
		public void VolumeRampDown()
		{
			CallMethod(VolumeRampDeviceControlApi.METHOD_VOLUME_RAMP_DOWN);
		}

		/// <summary>
		/// Starts raising the volume, and continues until RampStop is called.
		/// <see cref="IVolumeRampDeviceControl.VolumeRampStop"/> must be called after
		/// </summary>
		public void VolumeRampUp()
		{
			CallMethod(VolumeRampDeviceControlApi.METHOD_VOLUME_RAMP_UP);
		}

		/// <summary>
		/// Lowers the volume one time
		/// Amount of the change varies between implementations - typically "1" raw unit
		/// </summary>
		public void VolumeDecrement()
		{
			CallMethod(VolumeRampDeviceControlApi.METHOD_VOLUME_DECREMENT);
		}

		/// <summary>
		/// Raises the volume one time
		/// Amount of the change varies between implementations - typically "1" raw unit
		/// </summary>
		public void VolumeIncrement()
		{
			CallMethod(VolumeRampDeviceControlApi.METHOD_VOLUME_INCREMENT);
		}

		/// <summary>
		/// Sets the raw volume. This will be clamped to the min/max and safety min/max.
		/// </summary>
		/// <param name="volume"></param>
		public void SetVolumeLevel(float volume)
		{
			CallMethod(VolumeLevelDeviceControlApi.METHOD_SET_VOLUME_LEVEL, volume);
		}

		/// <summary>
		/// Toggles the current mute state.
		/// </summary>
		public void VolumeMuteToggle()
		{
			CallMethod(VolumeMuteBasicDeviceControlApi.METHOD_VOLUME_MUTE_TOGGLE);
		}

		/// <summary>
		/// Sets the mute state.
		/// </summary>
		/// <param name="mute"></param>
		public void SetVolumeMute(bool mute)
		{
			CallMethod(VolumeMuteDeviceControlApi.METHOD_SET_VOLUME_MUTE, mute);
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
			                 .SubscribeEvent(VolumeLevelDeviceControlApi.EVENT_VOLUME_CHANGED)
			                 .SubscribeEvent(VolumeMuteFeedbackDeviceControlApi.EVENT_MUTE_STATE_CHANGED)
							 .GetProperty(VolumeLevelDeviceControlApi.PROPERTY_VOLUME_LEVEL_MAX)
							 .GetProperty(VolumeLevelDeviceControlApi.PROPERTY_VOLUME_LEVEL_MIN)
							 .GetProperty(VolumeLevelDeviceControlApi.PROPERTY_VOLUME_LEVEL)
							 .GetProperty(VolumeLevelDeviceControlApi.PROPERTY_VOLUME_PERCENT)
							 .GetProperty(VolumeLevelDeviceControlApi.PROPERTY_VOLUME_STRING)
			                 .Complete();
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
				case VolumeLevelDeviceControlApi.EVENT_VOLUME_CHANGED:
					HandleVolumeChangeEvent(result.GetValue<VolumeChangeState>());
					break;
				case VolumeMuteFeedbackDeviceControlApi.EVENT_MUTE_STATE_CHANGED:
					VolumeIsMuted = result.GetValue<bool>();
					break;

			}
		}

		/// <summary>
		/// Updates the proxy with a property result.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="result"></param>
		protected override void ParseProperty(string name, ApiResult result)
		{
			base.ParseProperty(name, result);

			switch (name)
			{
				case VolumeLevelDeviceControlApi.PROPERTY_VOLUME_LEVEL_MAX:
					m_MaxVolume = result.GetValue<float>();
					break;
				case VolumeLevelDeviceControlApi.PROPERTY_VOLUME_LEVEL_MIN:
					m_MinVolume = result.GetValue<float>();
					break;
				case VolumeLevelDeviceControlApi.PROPERTY_VOLUME_LEVEL:
					VolumeLevel = result.GetValue<float>();
					break;
				case VolumeLevelDeviceControlApi.PROPERTY_VOLUME_PERCENT:
					VolumePercent = result.GetValue<float>();
					break;
				case VolumeLevelDeviceControlApi.PROPERTY_VOLUME_STRING:
					VolumeString = result.GetValue<string>();
					break;
			}
		}

		#endregion

		#region Private Methods

		private void RaiseOnVolumeChanged()
		{
			OnVolumeChanged.Raise(this, new VolumeDeviceVolumeChangedEventArgs(VolumeLevel,VolumePercent, VolumeString));
		}

		private void HandleVolumeChangeEvent(VolumeChangeState volumeState)
		{
			bool changed = false;
			changed |= Math.Abs(volumeState.VolumePercent - VolumePercent) > TOLERANCE;
			changed |= Math.Abs(volumeState.VolumeLevel - VolumeLevel) > TOLERANCE;
			changed |= !volumeState.VolumeString.Equals(VolumeString);

			if (!changed)
				return;

			VolumePercent = volumeState.VolumePercent;
			VolumeLevel = volumeState.VolumeLevel;
			VolumeString = volumeState.VolumeString;

			OnVolumeChanged.Raise(this, new VolumeDeviceVolumeChangedEventArgs(volumeState));


		}

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			VolumeLevelDeviceControlConsole.BuildConsoleStatus(this, addRow);
			VolumeMuteFeedbackDeviceControlConsole.BuildConsoleStatus(this, addRow);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			foreach (IConsoleCommand command in VolumeLevelDeviceControlConsole.GetConsoleCommands(this))
				yield return command;

			foreach (IConsoleCommand command in VolumeMuteFeedbackDeviceControlConsole.GetConsoleCommands(this))
				yield return command;
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}