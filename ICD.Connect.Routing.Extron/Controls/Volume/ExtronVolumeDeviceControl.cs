using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Audio.Controls;
using ICD.Connect.Routing.Extron.Devices.Switchers;

namespace ICD.Connect.Routing.Extron.Controls.Volume
{
	public class ExtronVolumeDeviceControl : AbstractVolumeRawLevelDeviceControl<IDtpCrosspointDevice>, IVolumeMuteFeedbackDeviceControl
	{
		private const string VOLUME_FEEDBACK_REGEX = @"Ds(?:G|H)(\d{5})\*(-?\d+)";
		private const string MUTE_FEEDBACK_REGEX = @"Ds(\d{5})\*(-?\d+)";
		
		private readonly eExtronVolumeObject m_VolumeObject;

		private float m_VolumeRaw;
		private bool m_IsMuted;

		public event EventHandler<BoolEventArgs> OnMuteStateChanged;

		public ExtronVolumeDeviceControl(IDtpCrosspointDevice parent, int id, eExtronVolumeObject volumeObject) : base(parent, id)
		{
			m_VolumeObject = volumeObject;
			Subscribe(parent);
		}

		#region Properties

		public override float VolumeRaw
		{
			get { return m_VolumeRaw; }
		}

		protected override float VolumeRawMinAbsolute
		{
			get
			{
				if (m_VolumeObject >= eExtronVolumeObject.Mic1InputGain &&
				    m_VolumeObject <= eExtronVolumeObject.Mic4InputGain)
					return -18;

				if (m_VolumeObject >= eExtronVolumeObject.VirtualReturnAGain &&
				    m_VolumeObject <= eExtronVolumeObject.VirtualReturnHGain)
					return -18;

				if (m_VolumeObject >= eExtronVolumeObject.Output1AnalogVolume &&
				    m_VolumeObject <= eExtronVolumeObject.Output4AnalogVolume)
					return -100;

				return 0;
			}
		}

		protected override float VolumeRawMaxAbsolute
		{
			get
			{
				if (m_VolumeObject >= eExtronVolumeObject.Mic1InputGain &&
				    m_VolumeObject <= eExtronVolumeObject.Mic4InputGain)
					return 80;

				if (m_VolumeObject >= eExtronVolumeObject.VirtualReturnAGain &&
				    m_VolumeObject <= eExtronVolumeObject.VirtualReturnHGain)
					return 24;

				if (m_VolumeObject >= eExtronVolumeObject.Output1AnalogVolume &&
				    m_VolumeObject <= eExtronVolumeObject.Output4AnalogVolume)
					return 0;

				return 0;
			}
		}

		public bool VolumeIsMuted { 
			get { return m_IsMuted; }
			set
			{
				if (value == m_IsMuted)
					return;
				
				m_IsMuted = value; 

				OnMuteStateChanged.Raise(this, new BoolEventArgs(m_IsMuted));
			}
		}

		#endregion

		#region Methods

		public override void SetVolumeRaw(float volume)
		{
			int volumeParam = (int) (volume * 10);
			int objectId = (int) m_VolumeObject;

			Parent.SendCommand("WG{0}*{1}AU", objectId, volumeParam);

			// Output analog volume needs to control left and right channel
			if(m_VolumeObject >= eExtronVolumeObject.Output1AnalogVolume && m_VolumeObject <= eExtronVolumeObject.Output4AnalogVolume)
				Parent.SendCommand("WG{0}*{1}AU", objectId + 1, volumeParam);
		}

		public void VolumeMuteToggle()
		{
			SetVolumeMute(!VolumeIsMuted);
		}

		public void SetVolumeMute(bool mute)
		{
			Parent.SendCommand("WM{0}*{1}AU", (int) m_VolumeObject, mute ? 1 : 0);
		}

		#endregion

		#region Parent Callbacks

		private void Subscribe(IDtpCrosspointDevice parent)
		{
			parent.OnResponseReceived += ParentOnResponseReceived;
			parent.OnInitializedChanged += ParentOnInitializedChanged;
		}

		private void Unsubscribe(IDtpCrosspointDevice parent)
		{
			parent.OnResponseReceived -= ParentOnResponseReceived;
			parent.OnInitializedChanged -= ParentOnInitializedChanged;
		}

		private void ParentOnResponseReceived(object sender, StringEventArgs args)
		{
			var match = Regex.Match(args.Data, VOLUME_FEEDBACK_REGEX);
			if (match.Success)
			{
				int objectId = int.Parse(match.Groups[1].Value);
				if (objectId == (int) m_VolumeObject)
				{
					m_VolumeRaw = int.Parse(match.Groups[2].Value) / 10f;
					VolumeFeedback(m_VolumeRaw);
				}
			}

			match = Regex.Match(args.Data, MUTE_FEEDBACK_REGEX);
			if(match.Success)
			{
				int objectId = int.Parse(match.Groups[1].Value);
				if (objectId == (int) m_VolumeObject)
				{
					VolumeIsMuted = match.Groups[2].Value == "1";
				}
			}
		}

		private void ParentOnInitializedChanged(object sender, BoolEventArgs e)
		{
			int volumeObjectId = (int) m_VolumeObject;
			Parent.SendCommand("WG{0}AU", volumeObjectId);
			Parent.SendCommand("WM{0}AU", volumeObjectId);
		}

		#endregion
	}
}
