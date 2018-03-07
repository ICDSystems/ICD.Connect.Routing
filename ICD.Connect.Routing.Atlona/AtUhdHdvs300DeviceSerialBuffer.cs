using System;
using System.Collections.Generic;
using System.Text;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Protocol.SerialBuffers;

namespace ICD.Connect.Routing.Atlona
{
	/// <summary>
	/// AtUhdHdvs300DeviceSerialBuffer raises login prompts, delimits on \r\n and raises empty serials.
	/// </summary>
	public sealed class AtUhdHdvs300DeviceSerialBuffer : ISerialBuffer
	{
		public event EventHandler<StringEventArgs> OnCompletedSerial;
		public event EventHandler OnLoginPrompt;
		public event EventHandler OnPasswordPrompt;
		public event EventHandler OnEmptyPrompt;

		private readonly StringBuilder m_RxData;
		private readonly Queue<string> m_Queue;

		private readonly SafeCriticalSection m_QueueSection;
		private readonly SafeCriticalSection m_ParseSection;

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		public AtUhdHdvs300DeviceSerialBuffer()
		{
			m_RxData = new StringBuilder();
			m_Queue = new Queue<string>();

			m_QueueSection = new SafeCriticalSection();
			m_ParseSection = new SafeCriticalSection();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Enqueues the serial data.
		/// </summary>
		/// <param name="data"></param>
		public void Enqueue(string data)
		{
			m_QueueSection.Execute(() => m_Queue.Enqueue(data));
			Parse();
		}

		/// <summary>
		/// Clears all queued data in the buffer.
		/// </summary>
		public void Clear()
		{
			m_ParseSection.Enter();
			m_QueueSection.Enter();

			try
			{
				m_RxData.Clear();
				m_Queue.Clear();
			}
			finally
			{
				m_ParseSection.Leave();
				m_QueueSection.Leave();
			}
		}

		#endregion

		/// <summary>
		/// Searches the enqueued serial data for the delimiter character.
		/// Complete strings are raised via the OnCompletedString event.
		/// </summary>
		private void Parse()
		{
			if (!m_ParseSection.TryEnter())
				return;

			try
			{
				string data = null;

				while (m_QueueSection.Execute(() => m_Queue.Dequeue(out data)))
				{
					while (true)
					{
						// Check for login prompt
						if (data.StartsWith("\r\nLogin:"))
						{
							OnLoginPrompt.Raise(this);
							data = data.Substring("\r\nLogin:".Length);
						}

						// Check for password prompt
						if (data.StartsWith("\r\nPassword:"))
						{
							OnPasswordPrompt.Raise(this);
							data = data.Substring("\r\nPassword:".Length);
						}

						// Check for login successful prompt
						if (data.StartsWith("\r\n"))
						{
							OnEmptyPrompt.Raise(this);
							data = data.Substring("\r\n".Length);
						}

						int index = data.IndexOf("\r\n");

						if (index < 0)
						{
							m_RxData.Append(data);
							break;
						}

						m_RxData.Append(data.Substring(0, index));
						data = data.Substring(index + 2);

						string output = m_RxData.Pop();
						if (string.IsNullOrEmpty(output))
							continue;

						OnCompletedSerial.Raise(this, new StringEventArgs(output));
					}
				}
			}
			finally
			{
				m_ParseSection.Leave();
			}
		}
	}
}
