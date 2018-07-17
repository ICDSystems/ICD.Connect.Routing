using System;
using System.Collections.Generic;
using System.Text;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Audio.Controls;
using ICD.Connect.Routing.Extron.Devices.Switchers;

namespace ICD.Connect.Routing.Extron.Controls.Volume
{
	public abstract class AbstractExtronVolumeDeviceControl : AbstractVolumeRawLevelDeviceControl<IDtpCrosspointDevice>, IVolumeMuteFeedbackDeviceControl
	{
		public event EventHandler<BoolEventArgs> OnMuteStateChanged;

		private readonly string m_Name;
		private readonly eExtronVolumeType m_VolumeType;

		private float m_VolumeRaw;
		private bool m_IsMuted;
		
		protected AbstractExtronVolumeDeviceControl(IDtpCrosspointDevice parent, int id, string name, eExtronVolumeType volumeType)
			: base(parent, id)
		{
			m_Name = name;
			m_VolumeType = volumeType;

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

		public bool VolumeIsMuted
		{ 
			get { return m_IsMuted; }
			protected set
			{
				if (value == m_IsMuted)
					return;
				
				m_IsMuted = value; 

				OnMuteStateChanged.Raise(this, new BoolEventArgs(m_IsMuted));
			}
		}

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

		protected virtual void Initialize()
		{
		}

		#endregion
		
		#region Methods

		public void VolumeMuteToggle()
		{
			SetVolumeMute(!VolumeIsMuted);
		}

		public abstract void SetVolumeMute(bool mute);

		/// <summary>
		/// Sets the VolumeRaw property and calls VolumeFeedback with that value.
		/// Only need this since C# won't let me add a setter to an abstract property
		/// </summary>
		/// <param name="value"></param>
		protected void SetVolumeRawProperty(float value)
		{
			m_VolumeRaw = value;
			VolumeFeedback(m_VolumeRaw);
		}

		#endregion

		#region Parent Callbacks

		private void Subscribe(IDtpCrosspointDevice parent)
		{
			parent.OnInitializedChanged += ParentOnInitializedChanged;
		}

		private void Unsubscribe(IDtpCrosspointDevice parent)
		{
			parent.OnInitializedChanged -= ParentOnInitializedChanged;
		}

		private void ParentOnInitializedChanged(object sender, BoolEventArgs e)
		{
			if(e.Data)
				Initialize();
		}

		#endregion
	}
}
