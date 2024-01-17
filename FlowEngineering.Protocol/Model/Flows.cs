namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.FlowEngineering.Protocol;
	using Skyline.DataMiner.FlowEngineering.Protocol.Enums;
	using Skyline.DataMiner.Scripting;

	public abstract class Flows<T> : ConcurrentDictionary<string, T>
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

			TryAdd(flow.Instance, flow);
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

			return TryRemove(flow.Instance, out _);
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

		public IEnumerable<T> UnlinkFlowEngineeringFlows(Guid provisionedFlowId)
		{
			var provisionedFlowIdString = Convert.ToString(provisionedFlowId);
			var linkedFlows = Values.Where(x => String.Equals(x.LinkedFlow, provisionedFlowIdString)).ToList();

			foreach (var linkedFlow in linkedFlows)
			{
				if (linkedFlow.IsPresent)
				{
					linkedFlow.LinkedFlow = String.Empty;
					linkedFlow.ExpectedBitrate = -1;
				}
				else
				{
					// flow was not present on the device, so row can be removed
					Remove(linkedFlow);
				}

				yield return linkedFlow;
			}
		}

		public void RegisterNotPresentOnLocalSystem(T flow)
		{
			if (flow.FlowOwner == FlowOwner.FlowEngineering)
			{
				// change to not present, but keep the row
				flow.IsPresent = false;
			}
			else
			{
				Remove(flow);
			}
		}

		public abstract void LoadTable(SLProtocol protocol);

		public abstract void UpdateTable(SLProtocol protocol, bool includeStatistics = true);

		public abstract void UpdateStatistics(SLProtocol protocol);
	}
}