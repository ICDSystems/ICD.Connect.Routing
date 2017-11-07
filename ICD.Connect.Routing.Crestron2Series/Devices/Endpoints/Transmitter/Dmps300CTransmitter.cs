namespace ICD.Connect.Routing.Crestron2Series.Devices.Endpoints.Transmitter
{
	public sealed class Dmps300CTransmitter : AbstractDmps300CDevice<Dmps300CTransmitterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public Dmps300CTransmitter()
		{
			Controls.Add(new Dmps300CTransmitterSourceControl(this));
		}
	}
}
