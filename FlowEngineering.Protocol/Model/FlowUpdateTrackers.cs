namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.ConnectorAPI.FlowEngineering.Info;

	public class FlowUpdateTrackers : Dictionary<string, FlowUpdateTracker>
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
	}
}
