namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.FlowEngineering.Protocol;
	using Skyline.DataMiner.Scripting;

	public abstract class Flows<T> : Dictionary<string, T>
		where T : Flow
	{
		protected FlowEngineeringManager _manager;

		protected Flows(FlowEngineeringManager manager)
		{
			_manager = manager;
		}

		public void Add(T flow)
		{
			if (flow == null)
			{
				throw new ArgumentNullException(nameof(flow));
			}

			Add(flow.Instance, flow);
		}

		public void AddRange(IEnumerable<T> flows)
		{
			if (flows == null)
			{
				throw new ArgumentNullException(nameof(flows));
			}

			foreach (var flow in flows)
			{
				Add(flow);
			}
		}

		public void ReplaceFlows(IEnumerable<T> newFlows)
		{
			Clear();
			AddRange(newFlows);
		}

		public abstract void LoadTable(SLProtocol protocol);

		public abstract void UpdateTable(SLProtocol protocol, bool includeStatistics = true);

		public abstract void UpdateStatistics(SLProtocol protocol);

		public abstract T RegisterFlowEngineeringFlow(ConnectorAPI.FlowEngineering.Info.FlowInfoMessage flowInfo, bool ignoreDestinationPort = false);

		public abstract T UnregisterFlowEngineeringFlow(ConnectorAPI.FlowEngineering.Info.FlowInfoMessage flowInfo, bool ignoreDestinationPort = false);
	}
}