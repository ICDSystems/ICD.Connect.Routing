using System;
using System.Collections.Generic;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Extron.Devices.Endpoints
{
    public class DtpHdmiMidpointControl<TDevice> : AbstractRouteMidpointControl<TDevice>
        where TDevice : IDtpHdmiDevice
    {
        public DtpHdmiMidpointControl(TDevice parent, int id) : base(parent, id)
        {
        }

        public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;
        public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
        public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

        public override ConnectorInfo? GetInput(int output, eConnectionType type)
        {
            if (output != 1)
                return null;
            return Default();
        }

        public override IEnumerable<ConnectorInfo> GetInputs()
        {
            yield return Default();
        }

        public override IEnumerable<ConnectorInfo> GetOutputs()
        {
            yield return Default();
        }

        public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
        {
            yield return Default();
        }

        public override bool GetSignalDetectedState(int input, eConnectionType type)
        {
            if (input != 1)
                return false;
            return true;
        }

        private ConnectorInfo Default()
        {
            return new ConnectorInfo(1, eConnectionType.Audio | eConnectionType.Video);
        }
    }
}
