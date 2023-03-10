using System;
using System.Collections.Generic;
using System.Text;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Protocol.SerialBuffers;

namespace ICD.Connect.Routing.Atlona
{
	/// <summary>
	/// AtUhdHdvs300DeviceSerialBuffer raises login prompts, delimits on \r\n and raises empty serials.
	/// </summary>
	public sealed class AtUhdHdvs300DeviceSerialBuffer : AbstractSerialBuffer
	{
		public event EventHandler OnLoginPrompt;
		public event EventHandler OnPasswordPrompt;
		public event EventHandler OnEmptyPrompt;

		private readonly StringBuilder m_RxData;

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		public AtUhdHdvs300DeviceSerialBuffer()
		{
			m_RxData = new StringBuilder();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Override to clear any current state.
		/// </summary>
		protected override void ClearFinal()
		{
			m_RxData.Clear();
		}

		/// <summary>
		/// Override to process the given item for chunking.
		/// </summary>
		/// <param name="data"></param>
		protected override IEnumerable<string> Process(string data)
		{
			while (true)
			{
				data = ParseLoginPrompts(data);

				int index = data.IndexOf("\r\n", StringComparison.Ordinal);

				if (index < 0)
				{
					m_RxData.Append(data);
					break;
				}

				m_RxData.Append(data.Substring(0, index));
				data = data.Substring(index + 2);

				string output = m_RxData.Pop();
				if (!string.IsNullOrEmpty(output))
					yield return output;
			}
		}

		private string ParseLoginPrompts(string data)
		{
			// Check for login prompt
			if (data.StartsWith("\r\nLogin:"))
			{
				OnLoginPrompt.Raise(this);
				return data.Substring("\r\nLogin:".Length);
			}

			// Check for password prompt
			if (data.StartsWith("\r\nPassword:"))
			{
				OnPasswordPrompt.Raise(this);
				return data.Substring("\r\nPassword:".Length);
			}

			// Check for login successful prompt
			if (data.StartsWith("\r\n"))
			{
				OnEmptyPrompt.Raise(this);
				return data.Substring("\r\n".Length);
			}

			return data;
		}

		#endregion
	}
}
