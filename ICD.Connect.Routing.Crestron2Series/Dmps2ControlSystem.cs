using System;
using System.Text;
using Crestron.SimplSharp; // For Basic SIMPL# Classes
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.Crestron2Series
{
    public sealed class Dmps2ControlSystem : AbstractDevice<Dmps2ControlSystemSettings>
    {
        /// <summary>
        /// SIMPL+ can only execute the default constructor. If you have variables that require initialization, please
        /// use an Initialize method
        /// </summary>
        public Dmps2ControlSystem()
        {
            
        }

        /// <summary>
        /// Gets the current online status of the device.
        /// </summary>
        /// <returns></returns>
        protected override bool GetIsOnlineStatus()
        {
            return false;
        }
    }
}
