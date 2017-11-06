using System;
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.Crestron2Series
{
    public class Dmps2ControlSystemSettings : AbstractDeviceSettings
    {
        private const string FACTORY_NAME = "Dmps2";

        /// <summary>
        /// Gets the originator factory name.
        /// </summary>
        public override string FactoryName { get { return FACTORY_NAME; } }

        /// <summary>
        /// Gets the type of the originator for this settings instance.
        /// </summary>
        public override Type OriginatorType { get { return typeof(Dmps2ControlSystem); } }
    }
}