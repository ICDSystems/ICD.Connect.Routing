namespace ICD.Connect.Routing.Crestron2Series.Devices.Endpoints.Receiver
{
    public sealed class Dmps300CReceiver : AbstractDmps300CEndpointDevice<Dmps300CReceiverSettings>
    {
		/// <summary>
		/// Constructor.
		/// </summary>
	    public Dmps300CReceiver()
	    {
		    Controls.Add(new Dmps300CReceiverDestinationControl(this));
	    }
    }
}
