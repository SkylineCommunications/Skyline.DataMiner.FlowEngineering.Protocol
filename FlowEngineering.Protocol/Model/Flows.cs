namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.FlowEngineering.Protocol;
	using Skyline.DataMiner.Scripting;
	using Skyline.DataMiner.ConnectorAPI.FlowEngineering.Info;

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

		public bool Remove(T flow)
		{
			if (flow == null)
			{
				throw new ArgumentNullException(nameof(flow));
			}

			return Remove(flow.Instance);
		}

		public void ReplaceFlows(IEnumerable<T> newFlows)
		{
			if (newFlows == null)
			{
				throw new ArgumentNullException(nameof(newFlows));
			}

			Clear();
			AddRange(newFlows);
		}

		public abstract void LoadTable(SLProtocol protocol);

		public abstract void UpdateTable(SLProtocol protocol, bool includeStatistics = true);

		public abstract void UpdateStatistics(SLProtocol protocol);

		public abstract T RegisterFlowEngineeringFlow(FlowInfoMessage flowInfo, string instance, bool ignoreDestinationPort = false);

		public abstract T UnregisterFlowEngineeringFlow(FlowInfoMessage flowInfo);
	}
}