#if SIMPLSHARP
#endif
using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.DM;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.EventArguments;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Input.Microphone
{
	public abstract class AbstractDmps3MicrophoneDeviceControl : AbstractDeviceControl<ControlSystemDevice>
	{
		public event EventHandler<BoolEventArgs> OnMuteStateChanged;
		public event EventHandler<BoolEventArgs> OnPhantomPowerStateChanged;

		private readonly string m_Name;
		private bool m_IsMuted;
		private bool m_IsPowered;
#if SIMPLSHARP
		protected readonly Dmps3Microphone Microphone;
#endif
		protected AbstractDmps3MicrophoneDeviceControl(ControlSystemDevice parent, int id, string name, uint inputAddress)
			: base(parent, id)
		{
			m_Name = name;
#if SIMPLSHARP
				Microphone = Parent.ControlSystem.Microphones[inputAddress] as Dmps3Microphone;
#endif
			Subscribe(parent);
		}

		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			if (Parent != null)
				Unsubscribe(Parent);
		}

	#region Properties

		public override string Name
		{
			get { return string.IsNullOrEmpty(m_Name) ? base.Name : m_Name; }
		}

		public bool MicrophoneIsMuted
		{
			get { return m_IsMuted; }
			private set
			{
				if (value == m_IsMuted)
					return;

				m_IsMuted = value;

				OnMuteStateChanged.Raise(this, new BoolEventArgs(m_IsMuted));
			}
		}

		public bool MicrophoneIsPowered
		{
			get { return m_IsPowered; }
			private set
			{
				if (value == m_IsPowered)
					return;

				m_IsPowered = value;

				OnPhantomPowerStateChanged.Raise(this, new BoolEventArgs(m_IsPowered));
			}
		}
	#endregion
			
	#region Methods

		public void SetGainLevel(float volume)
		{
#if SIMPLSHARP
			Microphone.Gain.ShortValue = (short)(volume * 10);
#endif
		}

		public void VolumeMuteToggle()
		{
			SetMicrophoneMute(!MicrophoneIsMuted);
		}

		public void PhantomPowerToggle()
		{
			SetPhantomPower(!MicrophoneIsPowered);
		}

		public void SetMicrophoneMute(bool mute)
		{
#if SIMPLSHARP
			if (Microphone.MuteOnFeedBack.BoolValue & !mute)
			{
				Microphone.MuteOff();
				MicrophoneIsMuted = false;
			}
			else if (Microphone.MuteOffFeedBack.BoolValue & mute)
			{
				Microphone.MuteOn();
				MicrophoneIsMuted = true;
			}
#endif
		}

		public void SetPhantomPower(bool power)
		{
#if SIMPLSHARP
			if (Microphone.PhantomPowerOnFeedBack.BoolValue & !power)
			{
				Microphone.PhantomPowerOff();
				MicrophoneIsPowered = false;
			}
			else if (Microphone.PhantomPowerOffFeedBack.BoolValue & power)
			{
				Microphone.PhantomPowerOn();
				MicrophoneIsPowered = true;
			}
#endif
		}

	#endregion

	#region Parent Callbacks

		private void Subscribe(ControlSystemDevice parent)
		{
			parent.OnIsOnlineStateChanged += ParentOnIsOnlineStateChanged;
		}

		private void Unsubscribe(ControlSystemDevice parent)
		{
			parent.OnIsOnlineStateChanged += ParentOnIsOnlineStateChanged;
		}

		private void ParentOnIsOnlineStateChanged(object sender, DeviceBaseOnlineStateApiEventArgs e)
		{
#if SIMPLSHARP
			MicrophoneIsMuted = Microphone.MuteOnFeedBack.BoolValue;
			MicrophoneIsPowered = Microphone.PhantomPowerOffFeedBack.BoolValue;
#endif
		}

	#endregion

	#region Console

			/// <summary>
			/// Gets the child console commands.
			/// </summary>
			/// <returns></returns>
			public override IEnumerable<IConsoleCommand> GetConsoleCommands()
			{
				yield return new GenericConsoleCommand<float>("SetGainLevel", "SetGainLevel <FLOAT>", (r) => SetGainLevel(r));
				yield return new GenericConsoleCommand<bool>("SetMicrophoneMute", "SetMicrophoneMute <BOOL>", (r) => SetMicrophoneMute(r));
				yield return new GenericConsoleCommand<bool>("SetPhantomPower", "SetPhantomPower <BOOL>", r => SetPhantomPower(r));

			}

	#endregion

	}
}
