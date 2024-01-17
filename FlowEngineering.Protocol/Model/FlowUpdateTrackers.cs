namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Collections.Concurrent;

	using Skyline.DataMiner.ConnectorAPI.FlowEngineering.Info;

	public class FlowUpdateTrackers : ConcurrentDictionary<string, FlowUpdateTracker>
	{
		private readonly FlowEngineeringManager _manager;

		public FlowUpdateTrackers(FlowEngineeringManager manager)
		{
			_manager = manager;
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
