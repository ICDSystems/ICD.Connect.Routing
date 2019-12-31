using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Audio.Controls.Volume;
using ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint;

namespace ICD.Connect.Routing.Extron.Controls.Volume
{
	public abstract class AbstractExtronVolumeDeviceControl : AbstractVolumeDeviceControl<IDtpCrosspointDevice>
	{
		private readonly string m_Name;
		private readonly eExtronVolumeType m_VolumeType;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <param name="volumeType"></param>
		protected AbstractExtronVolumeDeviceControl(IDtpCrosspointDevice parent, int id, string name, eExtronVolumeType volumeType)
			: base(parent, id)
		{
			m_Name = name;
			m_VolumeType = volumeType;
		}

		#region Properties

		public override string Name
		{
			get { return string.IsNullOrEmpty(m_Name) ? base.Name : m_Name; }
		}

		/// <summary>
		/// Gets the minimum supported volume level.
		/// </summary>
		public override float VolumeLevelMin
		{
			get { return ExtronVolumeUtils.GetMinVolume(m_VolumeType); }
		}

		/// <summary>
		/// Gets the maximum supported volume level.
		/// </summary>
		public override float VolumeLevelMax
		{
			get { return ExtronVolumeUtils.GetMaxVolume(m_VolumeType); }
		}

		protected virtual void Initialize()
		{
		}

		#endregion
		
		#region Methods

		/// <summary>
		/// Toggles the current mute state.
		/// </summary>
		public override void ToggleIsMuted()
		{
			SetIsMuted(!IsMuted);
		}

		/// <summary>
		/// Raises the volume one time
		/// Amount of the change varies between implementations - typically "1" raw unit
		/// </summary>
		public override void VolumeIncrement()
		{
			SetVolumeLevel(VolumeLevel + 1);
		}

		/// <summary>
		/// Lowers the volume one time
		/// Amount of the change varies between implementations - typically "1" raw unit
		/// </summary>
		public override void VolumeDecrement()
		{
			SetVolumeLevel(VolumeLevel - 1);
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

		#endregion

		#region Parent Callbacks

		protected override void Subscribe(IDtpCrosspointDevice parent)
		{
			base.Subscribe(parent);

			parent.OnInitializedChanged += ParentOnInitializedChanged;
		}

		protected override void Unsubscribe(IDtpCrosspointDevice parent)
		{
			base.Unsubscribe(parent);

			parent.OnInitializedChanged -= ParentOnInitializedChanged;
		}

		private void ParentOnInitializedChanged(object sender, BoolEventArgs e)
		{
			if (e.Data)
				Initialize();
		}

		#endregion
	}
}
