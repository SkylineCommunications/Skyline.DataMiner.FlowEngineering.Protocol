namespace Skyline.DataMiner.FlowEngineering.Protocol
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.ConnectorAPI.FlowEngineering.Enums;
	using Skyline.DataMiner.ConnectorAPI.FlowEngineering.Info;
	using Skyline.DataMiner.FlowEngineering.Protocol.Model;
	using Skyline.DataMiner.Scripting;

	public class FlowEngineeringManager
	{
		public FlowEngineeringManager()
		{
			Interfaces = new Interfaces(this);
			IncomingFlows = new RxFlows(this);
			OutgoingFlows = new TxFlows(this);
		}

		public Interfaces Interfaces { get; }

		public RxFlows IncomingFlows { get; }

		public TxFlows OutgoingFlows { get; }

		public static FlowEngineeringManager GetInstance(SLProtocol protocol) => FlowEngineeringManagerInstances.GetInstance(protocol);

		public void LoadTables(SLProtocol protocol)
		{
			Interfaces.LoadTable(protocol);
			Interfaces.LoadDcfDynamicLinks(protocol);

			IncomingFlows.LoadTable(protocol);
			OutgoingFlows.LoadTable(protocol);
		}

		public void UpdateTables(SLProtocol protocol, bool includeStatistics = true)
		{
			Interfaces.UpdateTable(protocol, includeStatistics);
			IncomingFlows.UpdateTable(protocol, includeStatistics);
			OutgoingFlows.UpdateTable(protocol, includeStatistics);
		}

		public void UpdateInterfaceAndIncomingFlowsTables(SLProtocol protocol, bool includeStatistics = true)
		{
			Interfaces.UpdateTable(protocol, includeStatistics);
			IncomingFlows.UpdateTable(protocol, includeStatistics);
		}

		public void UpdateInterfaceAndOutgoingFlowsTables(SLProtocol protocol, bool includeStatistics = true)
		{
			Interfaces.UpdateTable(protocol, includeStatistics);
			OutgoingFlows.UpdateTable(protocol, includeStatistics);
		}

		public (ICollection<Flow> addedFlows, ICollection<Flow> removedFlows) HandleInterAppMessage(SLProtocol protocol, FlowInfoMessage message, string flowInstance, bool ignoreDestinationPort = false)
		{
			if (protocol == null)
			{
				throw new ArgumentNullException(nameof(protocol));
			}

			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			var addedFlows = new List<Flow>();
			var removedFlows = new List<Flow>();

			switch (message.ActionType)
			{
				case ActionType.Create:
					if (message.IsIncoming)
					{
						var flow = IncomingFlows.RegisterFlowEngineeringFlow(message, flowInstance, ignoreDestinationPort);

						if (flow != null)
							addedFlows.Add(flow);
					}
					else
					{
						var flow = OutgoingFlows.RegisterFlowEngineeringFlow(message, flowInstance, ignoreDestinationPort);

						if (flow != null)
							addedFlows.Add(flow);
					}

					break;

				case ActionType.Delete:
					if (message.IsIncoming)
					{
						var flow = IncomingFlows.UnregisterFlowEngineeringFlow(message);

						if (flow != null)
							removedFlows.Add(flow);
					}
					else
					{
						var flow = OutgoingFlows.UnregisterFlowEngineeringFlow(message);

						if (flow != null)
							removedFlows.Add(flow);
					}

					break;

				default:
					throw new InvalidOperationException($"Unknown action: {message.ActionType}");
			}

			UpdateTables(protocol);

			return (addedFlows, removedFlows);
		}
	}
}