#if NETFRAMEWORK
extern alias RealNewtonsoft;
using RealNewtonsoft.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using System;
using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.Routing.SPlus.SPlusSwitcher.State
{
	[JsonConverter(typeof(SPlusSwitcherStateConverter))]
	public sealed class SPlusSwitcherState
	{

		#region Fields
		/// <summary>
		/// Contains the input numbers for all inputs that are detecting a signal
		/// </summary>
		private readonly IcdHashSet<int> m_DetectedInputs;

		/// <summary>
		/// Routes for all audio outputs
		/// Key = Output Number
		/// Value = Input Number
		/// Unrouted outputs will not have keys
		/// </summary>
		private readonly Dictionary<int, int> m_AudioOutputRouting;

		/// <summary>
		/// Routes for all video outputs
		/// Key = Output Number
		/// Value = Input Number
		/// Unrouted outputs will not have keys
		/// </summary>
		private readonly Dictionary<int, int> m_VideoOutputRouting;

		/// <summary>
		/// Routes for all usb outputs
		/// Key = Output Number
		/// Value = Input Number
		/// Unrouted outputs will not have keys
		/// </summary>
		private readonly Dictionary<int, int> m_UsbOutputRouting;

		#endregion

		#region Properties

		public IcdHashSet<int> DetectedInputs {get { return m_DetectedInputs; }}

		public Dictionary<int, int> AudioOutputRouting {get { return m_AudioOutputRouting; }}

		public Dictionary<int, int> VideoOutputRouting { get { return m_VideoOutputRouting; } } 

		public Dictionary<int, int> UsbOutputRouting {get { return m_UsbOutputRouting; }}

		#endregion

		#region Constructors

		public SPlusSwitcherState()
		{
			m_DetectedInputs = new IcdHashSet<int>();
			m_AudioOutputRouting = new Dictionary<int, int>();
			m_VideoOutputRouting = new Dictionary<int, int>();
			m_UsbOutputRouting = new Dictionary<int, int>();
		}

		public SPlusSwitcherState(IcdHashSet<int> detectedInputs, Dictionary<int, int> audioOutputRouting,
		                          Dictionary<int, int> videoOutputRouting, Dictionary<int, int> usbOutputRouting)
		{
			m_DetectedInputs = detectedInputs;
			m_AudioOutputRouting = audioOutputRouting;
			m_VideoOutputRouting = videoOutputRouting;
			m_UsbOutputRouting = usbOutputRouting;
		}

		public SPlusSwitcherState(ushort[] detectedInputs, ushort[] audioOutputRouting, ushort[] videoOutputRouting,
		                          ushort[] usbOutputRouting)
		{
			m_DetectedInputs = new IcdHashSet<int>();
			m_AudioOutputRouting = new Dictionary<int, int>();
			m_VideoOutputRouting = new Dictionary<int, int>();
			m_UsbOutputRouting = new Dictionary<int, int>();

			//Add only detected inputs
			for (int i = 1; i < detectedInputs.Length; i++)
			{
				if (detectedInputs[i] != 0)
					m_DetectedInputs.Add(i);
			}

			//Add Outputs to appropriate Dictionaries
			AddOutputsToDictionary(audioOutputRouting, m_AudioOutputRouting);
			AddOutputsToDictionary(videoOutputRouting, m_VideoOutputRouting);
			AddOutputsToDictionary(usbOutputRouting, m_UsbOutputRouting);
		}

		#endregion

		#region Methods

		public void AddDetectedInputsRange(IEnumerable<int> inputs)
		{
			m_DetectedInputs.AddRange(inputs);
		}

		public void AddAudioOutputRoutingRange(IEnumerable<KeyValuePair<int,int>> audioOutputRouting )
		{
			m_AudioOutputRouting.AddRange(audioOutputRouting);
		}

		public void AddVideoOutputRoutingRange(IEnumerable<KeyValuePair<int, int>> videoOutputRouting)
		{
			m_VideoOutputRouting.AddRange(videoOutputRouting);
		}

		public void AddUsbOutputRoutingRange(IEnumerable<KeyValuePair<int, int>> usbOutputRouting)
		{
			m_UsbOutputRouting.AddRange(usbOutputRouting);
		}

		#endregion

		#region Static Methods

		private static void AddOutputsToDictionary(ushort[] inputArray, Dictionary<int, int> outputDictionary)
		{
			if (inputArray == null)
				throw new ArgumentNullException("inputArray");
			if (outputDictionary == null)
				throw new ArgumentNullException("outputDictionary");

			for (int i = 1; i < inputArray.Length; i++)
				if (inputArray[i] != 0)
					outputDictionary.Add(i, inputArray[i]);
		}

		#endregion

	}
}