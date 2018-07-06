using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Audio.Controls;
using ICD.Connect.Routing.Extron.Devices.Switchers;

namespace ICD.Connect.Routing.Extron.Controls.Volume
{
	public class ExtronGroupVolumeDeviceControl : AbstractVolumeRawLevelDeviceControl<IDtpCrosspointDevice>, IVolumeMuteFeedbackDeviceControl
	{
		private const string GROUP_FEEDBACK_REGEX = @"GrpmD(\d{1,2})\*(-?\d+)";

		/// <summary>
		/// Raised when the mute status is changed.
		/// </summary>
		public event EventHandler<BoolEventArgs> OnMuteStateChanged;

		private readonly eExtronVolumeType m_VolumeType;
		private readonly int? m_VolumeGroupId;
		private readonly int? m_MuteGroupId;
		
		private float m_VolumeRaw;
		private bool m_IsMuted;

		public ExtronGroupVolumeDeviceControl(IDtpCrosspointDevice parent, int id, eExtronVolumeType volumeType, int? volumeGroupId, int? muteGroupId)
			: base(parent, id)
		{
			m_VolumeType = volumeType;
			m_VolumeGroupId = volumeGroupId;
			m_MuteGroupId = muteGroupId;
			Subscribe(parent);
		}

		#region Properties

		public override float VolumeRaw
		{
			get { return m_VolumeRaw; }
		}

		protected override float VolumeRawMinAbsolute
		{
			get { return ExtronVolumeUtils.GetMinVolume(m_VolumeType); }
		}

		protected override float VolumeRawMaxAbsolute
		{
			get { return ExtronVolumeUtils.GetMaxVolume(m_VolumeType); }
		}

		public bool VolumeIsMuted
		{
			get { return m_IsMuted; }
			set
			{
				if (value == m_IsMuted)
					return;

				m_IsMuted = value;

				OnMuteStateChanged.Raise(this, new BoolEventArgs(value));
			}
		}

		#endregion

		#region Methods

		public override void SetVolumeRaw(float volume)
		{
			int volumeParam = (int) (volume * 10);
			if(m_VolumeGroupId != null)
				Parent.SendCommand("WD{0}*{1}GRPM", m_VolumeGroupId, volumeParam);
		}

		public void VolumeMuteToggle()
		{
			SetVolumeMute(!VolumeIsMuted);
		}

		public void SetVolumeMute(bool mute)
		{
			if (m_MuteGroupId != null)
				Parent.SendCommand("WD{0}*{1}GRPM", m_MuteGroupId, mute ? 1 : 0);
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
			var match = Regex.Match(args.Data, GROUP_FEEDBACK_REGEX);
			if (match.Success)
			{
				int groupId = int.Parse(match.Groups[1].Value);
				if (groupId == m_VolumeGroupId)
				{
					m_VolumeRaw = int.Parse(match.Groups[2].Value) / 10f;
					VolumeFeedback(m_VolumeRaw);
				}
				else if (groupId == m_MuteGroupId)
					VolumeIsMuted = int.Parse(match.Groups[2].Value) == 1;
			}
		}

		private void ParentOnInitializedChanged(object sender, BoolEventArgs e)
		{
			if (m_VolumeGroupId != null)
				Parent.SendCommand("WD{0}GRPM", m_VolumeGroupId);
			if (m_MuteGroupId != null)
				Parent.SendCommand("WD{0}GRPM", m_MuteGroupId);
		}

		#endregion
	}
}
