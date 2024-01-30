namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Collections.Concurrent;
	using System.Linq;

	using Skyline.DataMiner.ConnectorAPI.FlowEngineering.Info;

	public class FlowUpdateTrackers : ConcurrentDictionary<string, FlowUpdateTracker>
	{
		private readonly FlowEngineeringManager _manager;

		public FlowUpdateTrackers(FlowEngineeringManager manager)
		{
			_manager = manager;
		}

		public bool TryGet(Func<FlowUpdateTracker, bool> predicate, out FlowUpdateTracker updateTracker)
		{
			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			updateTracker = Values.FirstOrDefault(predicate);
			return updateTracker != null;
		}

		public bool TryGet(Guid provisionedFlowId, out FlowUpdateTracker updateTracker)
		{
			return TryGet(x => x.ProvisionedFlowId == provisionedFlowId, out updateTracker);
		}

		public bool TryGet(ProvisionedFlow provisionedFlow, out FlowUpdateTracker updateTracker)
		{
			if (provisionedFlow == null)
			{
				throw new ArgumentNullException(nameof(provisionedFlow));
			}

			return TryGet(provisionedFlow.ID, out updateTracker);
		}

		public FlowUpdateTracker CreateTracker(FlowInfoMessage flowInfoMessage)
		{
			if (flowInfoMessage == null)
			{
				throw new ArgumentNullException(nameof(flowInfoMessage));
			}

			var tracker = new FlowUpdateTracker(this, flowInfoMessage);
			this[tracker.ID] = tracker;

			return tracker;
		}

		public bool Remove(FlowUpdateTracker tracker)
		{
			if (tracker == null)
			{
				throw new ArgumentNullException(nameof(tracker));
			}

			return TryRemove(tracker.ID, out _);
		}
	}
}
