using ICD.Connect.Routing.Extron.Devices.DtpCrosspointBase;

namespace ICD.Connect.Routing.Extron.Devices.DtpCrosspoint84
{
	public sealed class DtpCrosspoint84 : AbstractDtpCrosspointDevice<DtpCrosspoint84Settings>
	{
		public DtpCrosspoint84()
		{
			Controls.Add(new DtpCrosspoint84SwitcherControl(this, 0));
		}
	}
}