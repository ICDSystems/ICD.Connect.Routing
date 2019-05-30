using System;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Info;
using ICD.Connect.Audio.Controls.Mute;
using ICD.Connect.Audio.Controls.Volume;
using ICD.Connect.Audio.EventArguments;
using ICD.Connect.Audio.Proxies.Controls.Mute;
using ICD.Connect.Audio.Proxies.Controls.Volume;
using ICD.Connect.Devices.Proxies.Controls;
using ICD.Connect.Devices.Proxies.Devices;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Proxy
{
	public sealed class ProxySPlusDestinationVolumeControl : AbstractProxyDeviceControl, IVolumeLevelDeviceControl, IVolumeMuteFeedbackDeviceControl
	{

		#region Events

		public event EventHandler<VolumeDeviceVolumeChangedEventArgs> OnVolumeChanged;
		public event EventHandler<MuteDeviceMuteStateChangedApiEventArgs> OnMuteStateChanged;

		#endregion

		#region Fields

		private bool m_VolumeIsMuted;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the current volume positon, 0 - 1
		/// </summary>
		public float VolumePosition { get { throw new NotImplementedException(); } }

		/// <summary>
		/// Gets the current volume, in string representation
		/// </summary>
		public string VolumeString { get { throw new NotImplementedException(); } }

		/// <summary>
		/// Gets the current volume, in the parent device's format
		/// </summary>
		public float VolumeLevel { get { throw new NotImplementedException(); } }

		/// <summary>
		/// VolumeLevelMaxRange is the best max volume we have for the control
		/// either the Max from the control or the absolute max for the control
		/// </summary>
		public float VolumeLevelMaxRange { get { throw new NotImplementedException(); } }

		/// <summary>
		/// VolumeLevelMinRange is the best min volume we have for the control
		/// either the Min from the control or the absolute min for the control
		/// </summary>
		public float VolumeLevelMinRange { get { throw new NotImplementedException(); } }

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
		public ProxySPlusDestinationVolumeControl(IProxyDeviceBase parent, int id) : base(parent, id)
		{
		}

		#region Methods

		/// <summary>
		/// Starts lowering the volume in steps of the given position, and continues until RampStop is called.
		/// </summary>
		/// <param name="decrement"></param>
		public void VolumePositionRampDown(float decrement)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Starts raising the volume in steps of the given position, and continues until RampStop is called.
		/// </summary>
		/// <param name="increment"></param>
		public void VolumePositionRampUp(float increment)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sets the volume position, from 0-1
		/// </summary>
		/// <param name="position"></param>
		public void SetVolumePosition(float position)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Stops any current ramp up/down in progress.
		/// </summary>
		public void VolumeRampStop()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Starts lowering the volume, and continues until RampStop is called.
		/// <see cref="IVolumeRampDeviceControl.VolumeRampStop"/> must be called after
		/// </summary>
		public void VolumeRampDown()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Starts raising the volume, and continues until RampStop is called.
		/// <see cref="IVolumeRampDeviceControl.VolumeRampStop"/> must be called after
		/// </summary>
		public void VolumeRampUp()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Lowers the volume one time
		/// Amount of the change varies between implementations - typically "1" raw unit
		/// </summary>
		public void VolumeDecrement()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Raises the volume one time
		/// Amount of the change varies between implementations - typically "1" raw unit
		/// </summary>
		public void VolumeIncrement()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sets the raw volume. This will be clamped to the min/max and safety min/max.
		/// </summary>
		/// <param name="volume"></param>
		public void SetVolumeLevel(float volume)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Increments the volume once.
		/// </summary>
		public void VolumeLevelIncrement(float incrementValue)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Decrements the volume once.
		/// </summary>
		public void VolumeLevelDecrement(float decrementValue)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Toggles the current mute state.
		/// </summary>
		public void VolumeMuteToggle()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sets the mute state.
		/// </summary>
		/// <param name="mute"></param>
		public void SetVolumeMute(bool mute)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region API

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
					RaiseVolumeChanged(result.GetValue<VolumeChangeState>());
					break;
				case VolumeMuteFeedbackDeviceControlApi.EVENT_MUTE_STATE_CHANGED:
					VolumeIsMuted = result.GetValue<bool>();
					break;

			}
		}

		/// <summary>
		/// Override to build initialization commands on top of the current class info.
		/// </summary>
		/// <param name="command"></param>
		protected override void Initialize(ApiClassInfo command)
		{
			base.Initialize(command);
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
				
			}
		}

		#endregion

		#region Private Methods

		private void RaiseVolumeChanged(VolumeChangeState state)
		{
			OnVolumeChanged.Raise(this, new VolumeDeviceVolumeChangedEventArgs(state));
		}

		private void RaiseMuteStateChanged(bool state)
		{
			OnMuteStateChanged.Raise(this, new MuteDeviceMuteStateChangedApiEventArgs(state));
		}

		#endregion
	}
}