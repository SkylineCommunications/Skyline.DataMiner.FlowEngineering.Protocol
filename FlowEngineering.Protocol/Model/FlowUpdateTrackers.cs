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

		public bool TryGet(ProvisionedFlow provisionedFlow, out FlowUpdateTracker updateTracker)
		{
			if (provisionedFlow == null)
			{
				throw new ArgumentNullException(nameof(provisionedFlow));
			}

			updateTracker = Values.FirstOrDefault(x => x.ProvisionedFlow == provisionedFlow);
			return updateTracker != null;
		}

		public FlowUpdateTracker CreateTracker(FlowInfoMessage flowInfoMessage, ProvisionedFlow provisionedFlow)
		{
			if (flowInfoMessage == null)
			{
				throw new ArgumentNullException(nameof(flowInfoMessage));
			}

			if (provisionedFlow == null)
			{
				throw new ArgumentNullException(nameof(provisionedFlow));
			}

			var tracker = new FlowUpdateTracker(this, flowInfoMessage, provisionedFlow);
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
