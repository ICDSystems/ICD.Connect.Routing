using System.Text.RegularExpressions;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Audio.Controls.Volume;
using ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint;

namespace ICD.Connect.Routing.Extron.Controls.Volume
{
	public sealed class ExtronGroupVolumeDeviceControl : AbstractExtronVolumeDeviceControl
	{
		private const string GROUP_FEEDBACK_REGEX = @"GrpmD(\d{1,2})\*(-?\d+)";

		private readonly int? m_VolumeGroupId;
		private readonly int? m_MuteGroupId;

		/// <summary>
		/// Returns the features that are supported by this volume control.
		/// </summary>
		public override eVolumeFeatures SupportedVolumeFeatures
		{
			get
			{
				return eVolumeFeatures.Mute |
					   eVolumeFeatures.MuteAssignment |
					   eVolumeFeatures.MuteFeedback |
					   eVolumeFeatures.Volume |
					   eVolumeFeatures.VolumeAssignment |
					   eVolumeFeatures.VolumeFeedback;
			}
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <param name="volumeType"></param>
		/// <param name="volumeGroupId"></param>
		/// <param name="muteGroupId"></param>
		public ExtronGroupVolumeDeviceControl(IDtpCrosspointDevice parent, int id, string name,
		                                      eExtronVolumeType volumeType, int? volumeGroupId, int? muteGroupId)
			: base(parent, id, name, volumeType)
		{
			m_VolumeGroupId = volumeGroupId;
			m_MuteGroupId = muteGroupId;
		}

		#region Methods

		public override void SetVolumeLevel(float level)
		{
			int volumeParam = (int) (level * 10);
			if (m_VolumeGroupId != null)
				Parent.SendCommand("WD{0}*{1}GRPM", m_VolumeGroupId, volumeParam);
			else
				Log(eSeverity.Warning, "Attempted to set volume, but no volume group ID has been set");
		}

		public override void SetIsMuted(bool mute)
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

		protected override void Subscribe(IDtpCrosspointDevice parent)
		{
			base.Subscribe(parent);

			parent.OnResponseReceived += ParentOnResponseReceived;
		}

		protected override void Unsubscribe(IDtpCrosspointDevice parent)
		{
			base.Unsubscribe(parent);

			parent.OnResponseReceived -= ParentOnResponseReceived;
		}

		private void ParentOnResponseReceived(object sender, StringEventArgs args)
		{
			var match = Regex.Match(args.Data, GROUP_FEEDBACK_REGEX);
			if (!match.Success)
				return;

			int groupId = int.Parse(match.Groups[1].Value);

			if (groupId == m_VolumeGroupId)
				VolumeLevel = int.Parse(match.Groups[2].Value) / 10f;
			else if (groupId == m_MuteGroupId)
				IsMuted = int.Parse(match.Groups[2].Value) == 1;
		}

		#endregion
	}
}
