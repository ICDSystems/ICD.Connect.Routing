using System;
using ICD.Connect.Audio.Controls.Microphone;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro.Extensions;
#else
using System;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Microphone
{
	public sealed class Dmps3MicrophoneDeviceControl : AbstractMicrophoneDeviceControl<ControlSystemDevice>
	{
		private readonly string m_Name;

#if SIMPLSHARP
		private readonly Dmps3Microphone m_Microphone;
#endif

		public override string Name { get { return string.IsNullOrEmpty(m_Name) ? base.Name : m_Name; } }

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
			m_Name = name;
#if SIMPLSHARP
			m_Microphone = Parent.ControlSystem.Microphones[inputAddress] as Dmps3Microphone;
#endif
		}

		#region Methods

		/// <summary>
		/// Sets the gain level.
		/// </summary>
		/// <param name="volume"></param>
		public override void SetGainLevel(float volume)
		{
#if SIMPLSHARP
			m_Microphone.Gain.ShortValue = (short)(volume * 10);
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Sets the muted state.
		/// </summary>
		/// <param name="mute"></param>
		public override void SetMuted(bool mute)
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
			GainLevel = m_Microphone.GainFeedBack.GetShortValueOrDefault() / 10.0f;
		}
#endif

		#endregion
	}
}
