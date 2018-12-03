using ICD.Common.Utils.Xml;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DeviceSupport;
#endif
using System;
using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices.Controls;

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
		private readonly Dmps3Microphone m_Microphone;
#endif

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <param name="inputAddress"></param>
		/// <param name="xml"></param>
		protected AbstractDmps3MicrophoneDeviceControl(ControlSystemDevice parent, int id, string name, uint inputAddress,
		                                               string xml)
			: base(parent, id)
		{
			m_Name = name;
#if SIMPLSHARP
			m_Microphone = Parent.ControlSystem.Microphones[inputAddress] as Dmps3Microphone;
#endif
			SetMicrophoneDefaultsFromXml(xml);
			Subscribe(parent);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			if (Parent != null)
				Unsubscribe(Parent);
		}

		#region Properties

		public override string Name { get { return string.IsNullOrEmpty(m_Name) ? base.Name : m_Name; } }

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
			m_Microphone.Gain.ShortValue = (short)(volume * 10);
#endif
		}

		public void MicrophoneMuteToggle()
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
			if (mute)
				m_Microphone.MuteOn();
			else
				m_Microphone.MuteOff();
#endif
		}

		public void SetPhantomPower(bool power)
		{
#if SIMPLSHARP
			if (power)
				m_Microphone.PhantomPowerOn();
			else
				m_Microphone.PhantomPowerOff();
#endif
		}

		#endregion

		#region Private Methods

		// TODO - Move to DMPS3 xml utils
		private void SetMicrophoneDefaultsFromXml(string controlElement)
		{
			bool? defaultMute = XmlUtils.TryReadChildElementContentAsBoolean(controlElement, "DefaultMute");
			ushort? defaultGain = XmlUtils.TryReadChildElementContentAsUShort(controlElement, "DefaultGain");
			bool? defaultPower = XmlUtils.TryReadChildElementContentAsBoolean(controlElement, "DefaultPower");

			if (defaultMute.HasValue)
				SetMicrophoneMute(defaultMute.Value);

			if (defaultGain.HasValue)
				SetGainLevel(defaultGain.Value);

			if (defaultPower.HasValue)
				SetPhantomPower(defaultPower.Value);
		}

		#endregion

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Subscribe(ControlSystemDevice parent)
		{
#if SIMPLSHARP
			parent.ControlSystem.MicrophoneChange += ControlSystemOnMicrophoneChange;
#endif
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Unsubscribe(ControlSystemDevice parent)
		{
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

			MicrophoneIsMuted = m_Microphone.MuteOnFeedBack.BoolValue;
			MicrophoneIsPowered = m_Microphone.PhantomPowerOnFeedBack.BoolValue;
		}
#endif

#endregion

#region Console

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			yield return new GenericConsoleCommand<float>("SetGainLevel", "SetGainLevel <FLOAT>", r => SetGainLevel(r));
			yield return
				new GenericConsoleCommand<bool>("SetMicrophoneMute", "SetMicrophoneMute <BOOL>", r => SetMicrophoneMute(r));
			yield return new GenericConsoleCommand<bool>("SetPhantomPower", "SetPhantomPower <BOOL>", r => SetPhantomPower(r));

		}

#endregion
	}
}
