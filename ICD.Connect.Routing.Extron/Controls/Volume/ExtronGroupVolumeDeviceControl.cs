﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Audio.Controls;
using ICD.Connect.Routing.Extron.Devices.Switchers;

namespace ICD.Connect.Routing.Extron.Controls.Volume
{
	public class ExtronGroupVolumeDeviceControl : AbstractExtronVolumeDeviceControl, IVolumeMuteFeedbackDeviceControl
	{
		private const string GROUP_FEEDBACK_REGEX = @"GrpmD(\d{1,2})\*(-?\d+)";

		private readonly int? m_VolumeGroupId;
		private readonly int? m_MuteGroupId;

		public ExtronGroupVolumeDeviceControl(IDtpCrosspointDevice parent, int id, string name,
		                                      eExtronVolumeType volumeType, int? volumeGroupId, int? muteGroupId)
			: base(parent, id, name, volumeType)
		{
			m_VolumeGroupId = volumeGroupId;
			m_MuteGroupId = muteGroupId;
			Subscribe(parent);
		}

		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			if(Parent != null)
				Unsubscribe(Parent);
		}

		#region Methods

		public override void SetVolumeRaw(float volume)
		{
			int volumeParam = (int) (volume * 10);
			if(m_VolumeGroupId != null)
				Parent.SendCommand("WD{0}*{1}GRPM", m_VolumeGroupId, volumeParam);
			else
				Log(eSeverity.Warning, "Attempted to set volume, but no volume group ID has been set");
		}

		public override void SetVolumeMute(bool mute)
		{
			if (m_MuteGroupId != null)
				Parent.SendCommand("WD{0}*{1}GRPM", m_MuteGroupId, mute ? 1 : 0);
			else
				Log(eSeverity.Warning, "Attempted to mute, but no mute group ID has been set");
		}

		protected override void Initialize()
		{
			if (m_VolumeGroupId != null)
				Parent.SendCommand("WD{0}GRPM", m_VolumeGroupId);
			if (m_MuteGroupId != null)
				Parent.SendCommand("WD{0}GRPM", m_MuteGroupId);
		}

		#endregion

		#region Parent Callbacks

		private void Subscribe(IDtpCrosspointDevice parent)
		{
			parent.OnResponseReceived += ParentOnResponseReceived;
		}

		private void Unsubscribe(IDtpCrosspointDevice parent)
		{
			parent.OnResponseReceived -= ParentOnResponseReceived;
		}

		private void ParentOnResponseReceived(object sender, StringEventArgs args)
		{
			var match = Regex.Match(args.Data, GROUP_FEEDBACK_REGEX);
			if (match.Success)
			{
				int groupId = int.Parse(match.Groups[1].Value);
				if (groupId == m_VolumeGroupId)
					SetVolumeRawProperty(int.Parse(match.Groups[2].Value) / 10f);
				else if (groupId == m_MuteGroupId)
					VolumeIsMuted = int.Parse(match.Groups[2].Value) == 1;
			}
		}

		#endregion
	}
}