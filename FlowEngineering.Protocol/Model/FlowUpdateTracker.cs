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

		public void SetSuccess(SLProtocol protocol)
		{
			if (protocol == null)
			{
				throw new ArgumentNullException(nameof(protocol));
			}

			SetResult(protocol, true, String.Empty);
		}

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

		public void SetTimeout(SLProtocol protocol, TimeSpan time, string message)
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

				var responseMessage = new FlowInfoResponseMessage()
				{
					ProvisionedFlowId = ProvisionedFlowId,
					IsSuccess = success,
					Message = message,
				};

				FlowInfoMessage.Reply(
					protocol.SLNet.RawConnection,
					responseMessage,
					FlowInfoResponseMessage.Serializer);
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
