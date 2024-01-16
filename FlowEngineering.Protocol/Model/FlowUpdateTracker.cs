namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Runtime.CompilerServices;
	using System.Threading.Tasks;

	using Skyline.DataMiner.ConnectorAPI.FlowEngineering.Enums;
	using Skyline.DataMiner.ConnectorAPI.FlowEngineering.Info;
	using Skyline.DataMiner.Scripting;

	public class FlowUpdateTracker
	{
		private readonly FlowUpdateTrackers _parent;
		private readonly TaskCompletionSource<(bool, string)> _tcs = new TaskCompletionSource<(bool, string)>();

		internal FlowUpdateTracker(FlowUpdateTrackers parent, FlowInfoMessage flowInfoMessage, ProvisionedFlow provisionedFlow)
		{
			_parent = parent ?? throw new ArgumentNullException(nameof(parent));

			FlowInfoMessage = flowInfoMessage ?? throw new ArgumentNullException(nameof(flowInfoMessage));
			ProvisionedFlow = provisionedFlow ?? throw new ArgumentNullException(nameof(provisionedFlow));
		}

		public FlowInfoMessage FlowInfoMessage { get; }

		public ProvisionedFlow ProvisionedFlow { get; }

		public string ID => FlowInfoMessage.Guid;

		public ActionType Action => FlowInfoMessage.ActionType;

		public Task<(bool success, string message)> Task => _tcs.Task;

		/// <summary>
		/// Gets or sets the additional data associated with this object.
		/// </summary>
		public object Tag { get; set; }

		public void SetResult(SLProtocol protocol, bool success, string message)
		{
			try
			{
				_tcs.SetResult((success, message));

				var responseMessage = new FlowInfoResponseMessage()
				{
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
				_parent.Remove(ID);
			}
		}

		public TaskAwaiter<(bool success, string message)> GetAwaiter()
		{
			return Task.GetAwaiter();
		}
	}
}
