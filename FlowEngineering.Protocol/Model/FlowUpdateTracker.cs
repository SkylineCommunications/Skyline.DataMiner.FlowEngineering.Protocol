namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Runtime.CompilerServices;
	using System.Threading.Tasks;
	using System.Timers;

	using Skyline.DataMiner.ConnectorAPI.FlowEngineering.Enums;
	using Skyline.DataMiner.ConnectorAPI.FlowEngineering.Info;
	using Skyline.DataMiner.Scripting;

	public sealed class FlowUpdateTracker : IDisposable
	{
		private readonly FlowUpdateTrackers _parent;
		private readonly TaskCompletionSource<(bool, string)> _tcs = new TaskCompletionSource<(bool, string)>();

		private bool incomingFlowSuccess = false;
		private bool outgoingFlowSuccess = false;

		internal FlowUpdateTracker(FlowUpdateTrackers parent, FlowInfoMessage flowInfoMessage)
		{
			_parent = parent ?? throw new ArgumentNullException(nameof(parent));

			FlowInfoMessage = flowInfoMessage ?? throw new ArgumentNullException(nameof(flowInfoMessage));
		}

		public FlowInfoMessage FlowInfoMessage { get; }

		public bool ResultReceived { get; private set; }

		public string ID => FlowInfoMessage.Guid;

		public Guid ProvisionedFlowId => FlowInfoMessage.ProvisionedFlowId;

		public ActionType Action => FlowInfoMessage.ActionType;

		public Task<(bool success, string message)> Task => _tcs.Task;

		/// <summary>
		/// Gets or sets the additional data associated with this object.
		/// </summary>
		public object Tag { get; set; }

		public TaskAwaiter<(bool success, string message)> GetAwaiter()
		{
			return Task.GetAwaiter();
		}

		/// <summary>
		/// Reply to the FlowInfoMessage that configuring the flows was successful.
		/// </summary>
		/// <param name="protocol">The SLProtocol instance.</param>
		public void SetSuccess(SLProtocol protocol)
		{
			if (protocol == null)
			{
				throw new ArgumentNullException(nameof(protocol));
			}

			SetResult(protocol, true, String.Empty);
		}

		/// <summary>
		/// Registers successful configuration of the incoming flow.
		/// Responds with success to the FlowInfoMessage if configuring the outgoing flow was also successful, or if outgoing flow was not necessary.
		/// </summary>
		/// <param name="protocol">The SLProtocol instance.</param>
		public void SetSuccessForIncomingFlow(SLProtocol protocol)
		{
			if (protocol == null)
			{
				throw new ArgumentNullException(nameof(protocol));
			}

			incomingFlowSuccess = true;

			if (outgoingFlowSuccess || !FlowInfoMessage.IsOutgoing)
			{
				SetSuccess(protocol);
			}
		}

		/// <summary>
		/// Registers successful configuration of the outgoing flow.
		/// Responds with success to the FlowInfoMessage if configuring the incoming flow was also successful, or if incoming flow was not necessary.
		/// </summary>
		/// <param name="protocol">The SLProtocol instance.</param>
		public void SetSuccessForOutgoingFlow(SLProtocol protocol)
		{
			if (protocol == null)
			{
				throw new ArgumentNullException(nameof(protocol));
			}

			outgoingFlowSuccess = true;

			if (incomingFlowSuccess || !FlowInfoMessage.IsIncoming)
			{
				SetSuccess(protocol);
			}
		}

		/// <summary>
		/// Reply to the FlowInfoMessage that configuring the flows has failed with the specified message.
		/// </summary>
		/// <param name="protocol">The SLProtocol instance.</param>
		/// <param name="message">A message that describes what went wrong.</param>
		public void SetFailed(SLProtocol protocol, string message)
		{
			if (protocol == null)
			{
				throw new ArgumentNullException(nameof(protocol));
			}

			if (String.IsNullOrEmpty(message))
			{
				throw new ArgumentException($"'{nameof(message)}' cannot be null or empty.", nameof(message));
			}

			SetResult(protocol, false, message);
		}

		/// <summary>
		/// Starts a timer that will automatically reply to the FlowInfoMessage with a fail after the specified timeout time if no other reply was sent yet.
		/// </summary>
		/// <param name="protocol">The SLProtocol instance.</param>
		/// <param name="time">The duration after which the failure response should be sent.</param>
		/// <param name="message">A message that describes what went wrong.</param>
		public void AutoFailAfterTimeout(SLProtocol protocol, TimeSpan time, string message)
		{
			if (String.IsNullOrWhiteSpace(message))
			{
				throw new ArgumentException($"'{nameof(message)}' cannot be null or whitespace.", nameof(message));
			}

			var timer = new Timer(time.TotalMilliseconds);

			timer.Elapsed += (s, e) =>
			{
				try
				{
					SetFailed(protocol, message);
				}
				finally
				{
					timer.Dispose();
				}
			};

			timer.Start();
		}

		private void SetResult(SLProtocol protocol, bool success, string message)
		{
			try
			{
				if (ResultReceived)
				{
					return;
				}

				ResultReceived = true;

				_tcs.SetResult((success, message));

				FlowInfoMessage.ReplyResult(protocol, success, message);
			}
			finally
			{
				_parent.Remove(this);
			}
		}

		#region IDisposable

		~FlowUpdateTracker()
		{
			Dispose();
		}

		public void Dispose()
		{
			_parent.Remove(this);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
