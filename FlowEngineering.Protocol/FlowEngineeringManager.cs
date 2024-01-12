﻿namespace Skyline.DataMiner.FlowEngineering.Protocol
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

		public ICollection<Flow> RegisterFlowEngineeringFlowsFromInterAppMessage(SLProtocol protocol, FlowInfoMessage message, string newFlowInstance, bool ignoreDestinationPort = false)
		{
			if (protocol == null)
			{
				throw new ArgumentNullException(nameof(protocol));
			}

			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			if (message.ActionType != ActionType.Delete)
			{
				throw new ArgumentException("Expected a FlowInfoMessage with ActionType 'Delete'");
			}

			if (String.IsNullOrWhiteSpace(newFlowInstance))
			{
				throw new ArgumentException($"'{nameof(newFlowInstance)}' cannot be null or whitespace.", nameof(newFlowInstance));
			}

			var addedFlows = new List<Flow>();

			if (message.IsIncoming)
			{
				var flow = IncomingFlows.RegisterFlowEngineeringFlow(message, newFlowInstance, ignoreDestinationPort);

				if (flow != null)
					addedFlows.Add(flow);
			}
			else
			{
				var flow = OutgoingFlows.RegisterFlowEngineeringFlow(message, newFlowInstance, ignoreDestinationPort);

				if (flow != null)
					addedFlows.Add(flow);
			}

			UpdateTables(protocol);

			return addedFlows;
		}

		public ICollection<Flow> UnregisterFlowEngineeringFlowsFromInterAppMessage(SLProtocol protocol, FlowInfoMessage message)
		{
			if (protocol == null)
			{
				throw new ArgumentNullException(nameof(protocol));
			}

			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			if (message.ActionType != ActionType.Create)
			{
				throw new ArgumentException("Expected a FlowInfoMessage with ActionType 'Create'");
			}

			var removedFlows = new List<Flow>();

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

			UpdateTables(protocol);

			return removedFlows;
		}
	}
}