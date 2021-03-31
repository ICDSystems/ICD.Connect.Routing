using System;
using ICD.Connect.Audio.Controls.Microphone;
using ICD.Connect.Audio.Controls.Volume;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Microphone
{
	public sealed class Dmps3MicrophoneDeviceControl : AbstractMicrophoneDeviceControl<ControlSystemDevice>
	{
		private readonly string m_Name;

#if SIMPLSHARP
		private readonly Dmps3Microphone m_Microphone;
#endif

		#region Properties

		public override string Name { get { return string.IsNullOrEmpty(m_Name) ? base.Name : m_Name; } }

		/// <summary>
		/// Gets the minimum supported volume level.
		/// </summary>
		public override float VolumeLevelMin { get { return 0; } }

		/// <summary>
		/// Gets the maximum supported volume level.
		/// </summary>
		public override float VolumeLevelMax { get { return 0; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		/// <param name="uuid"></param>
		/// <param name="name"></param>
		/// <param name="inputAddress"></param>
		public Dmps3MicrophoneDeviceControl(ControlSystemDevice parent, int id, Guid uuid, string name, uint inputAddress)
			: base(parent, id, uuid)
		{
			SupportedVolumeFeatures = eVolumeFeatures.Mute |
			                          eVolumeFeatures.MuteAssignment |
			                          eVolumeFeatures.MuteFeedback;

			m_Name = name;
#if SIMPLSHARP
			m_Microphone = Parent.ControlSystem.Microphones[inputAddress] as Dmps3Microphone;
#endif
		}

		#region Methods

		/// <summary>
		/// Sets the gain level.
		/// </summary>
		/// <param name="level"></param>
		public override void SetAnalogGainLevel(float level)
		{
#if SIMPLSHARP
			m_Microphone.Gain.ShortValue = (short)(level * 10);
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Sets the muted state.
		/// </summary>
		/// <param name="mute"></param>
		public override void SetIsMuted(bool mute)
		{
#if SIMPLSHARP
			if (mute)
				m_Microphone.MuteOn();
			else
				m_Microphone.MuteOff();
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Toggles the current mute state.
		/// </summary>
		public override void ToggleIsMuted()
		{
			SetIsMuted(!IsMuted);
		}

		/// <summary>
		/// Sets the raw volume level in the device volume representation.
		/// </summary>
		/// <param name="level"></param>
		public override void SetVolumeLevel(float level)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Raises the volume one time
		/// Amount of the change varies between implementations - typically "1" raw unit
		/// </summary>
		public override void VolumeIncrement()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Lowers the volume one time
		/// Amount of the change varies between implementations - typically "1" raw unit
		/// </summary>
		public override void VolumeDecrement()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Starts ramping the volume, and continues until stop is called or the timeout is reached.
		/// If already ramping the current timeout is updated to the new timeout duration.
		/// </summary>
		/// <param name="increment">Increments the volume if true, otherwise decrements.</param>
		/// <param name="timeout"></param>
		public override void VolumeRamp(bool increment, long timeout)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Stops any current ramp up/down in progress.
		/// </summary>
		public override void VolumeRampStop()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Sets the phantom power state.
		/// </summary>
		/// <param name="power"></param>
		public override void SetPhantomPower(bool power)
		{
#if SIMPLSHARP
			if (power)
				m_Microphone.PhantomPowerOn();
			else
				m_Microphone.PhantomPowerOff();
#else
			throw new NotSupportedException();
#endif
		}

		#endregion

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Subscribe(ControlSystemDevice parent)
		{
			base.Subscribe(parent);

			if (parent == null)
				return;

#if SIMPLSHARP
			parent.ControlSystem.MicrophoneChange += ControlSystemOnMicrophoneChange;
#endif
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Unsubscribe(ControlSystemDevice parent)
		{
			base.Unsubscribe(parent);

			if (parent == null)
				return;

#if SIMPLSHARP
			parent.ControlSystem.MicrophoneChange -= ControlSystemOnMicrophoneChange;
#endif
		}

#if SIMPLSHARP
		/// <summary>
		/// Called when an microphone change event is raised.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void ControlSystemOnMicrophoneChange(MicrophoneBase device, GenericEventArgs args)
		{
			if (device != m_Microphone)
				return;

			IsMuted = m_Microphone.MuteOnFeedBack.GetBoolValueOrDefault();
			PhantomPower = m_Microphone.PhantomPowerOnFeedBack.GetBoolValueOrDefault();
			AnalogGainLevel = m_Microphone.GainFeedBack.GetShortValueOrDefault() / 10.0f;
		}
#endif

		#endregion
	}
}
