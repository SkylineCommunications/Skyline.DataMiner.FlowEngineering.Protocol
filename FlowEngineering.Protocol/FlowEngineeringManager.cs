namespace Skyline.DataMiner.FlowEngineering.Protocol
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.ConnectorAPI.FlowEngineering.Enums;
	using Skyline.DataMiner.ConnectorAPI.FlowEngineering.Info;
	using Skyline.DataMiner.FlowEngineering.Protocol.DCF;
	using Skyline.DataMiner.FlowEngineering.Protocol.Model;
	using Skyline.DataMiner.Scripting;

	public class FlowEngineeringManager : IDisposable
	{
		private readonly Lazy<DcfInterfaceHelper> _lazy_dcfIntfHelper;

		internal FlowEngineeringManager(string key, SLProtocol protocol)
		{
			Key = key;
			Interfaces = new Interfaces(this);
			IncomingFlows = new RxFlows(this);
			OutgoingFlows = new TxFlows(this);
			ProvisionedFlows = new ProvisionedFlows(this);
			FlowUpdateTrackers = new FlowUpdateTrackers(this);

			_lazy_dcfIntfHelper = new Lazy<DcfInterfaceHelper>(() => DcfInterfaceHelper.Create(protocol));

			LoadTables(protocol);
		}

		internal string Key { get; }

		public Interfaces Interfaces { get; }

		public RxFlows IncomingFlows { get; }

		public TxFlows OutgoingFlows { get; }

		public ProvisionedFlows ProvisionedFlows { get; }

		public FlowUpdateTrackers FlowUpdateTrackers { get; }

		public DcfInterfaceHelper DcfInterfaceHelper => _lazy_dcfIntfHelper.Value;

		public static FlowEngineeringManager GetInstance(SLProtocol protocol) => FlowEngineeringManagerInstances.GetInstance(protocol);

		public void LoadTables(SLProtocol protocol)
		{
			Interfaces.LoadTable(protocol);
			Interfaces.LoadDcfDynamicLinks(protocol);

			IncomingFlows.LoadTable(protocol);
			OutgoingFlows.LoadTable(protocol);
			ProvisionedFlows.LoadTable(protocol);
		}

		public void UpdateTables(SLProtocol protocol, bool includeStatistics = true)
		{
			Interfaces.UpdateTable(protocol, includeStatistics);
			ProvisionedFlows.UpdateTable(protocol);
			UpdateIncomingAndOutgoingFlowsTables(protocol, includeStatistics);
		}

		public void UpdateIncomingAndOutgoingFlowsTables(SLProtocol protocol, bool includeStatistics = true)
		{
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

		public (ProvisionedFlow, ICollection<Flow> unlinkedFlows) HandleInterAppMessage(SLProtocol protocol, FlowInfoMessage message)
		{
			if (protocol == null)
			{
				throw new ArgumentNullException(nameof(protocol));
			}

			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			ProvisionedFlow provisionedFlow;
			ICollection<Flow> unlinkedFlows;

			switch (message.ActionType)
			{
				case ActionType.Create:
					provisionedFlow = RegisterProvisionedFlowFromInterAppMessage(protocol, message);
					unlinkedFlows = Array.Empty<Flow>();
					break;

				case ActionType.Delete:
					(provisionedFlow, unlinkedFlows) = UnregisterProvisionedFlowFromInterAppMessage(protocol, message);
					break;

				default:
					throw new InvalidOperationException($"Unknown action: {message.ActionType}");
			}

			UpdateTables(protocol);

			return (provisionedFlow, unlinkedFlows);
		}

		public ProvisionedFlow RegisterProvisionedFlowFromInterAppMessage(SLProtocol protocol, FlowInfoMessage message)
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

			var provisionedFlow = ProvisionedFlow.CreateFromFlowInfoMessage(message);
			ProvisionedFlows[provisionedFlow.ID] = provisionedFlow;

			provisionedFlow.AddToTable(protocol);

			return provisionedFlow;
		}

		public (ProvisionedFlow, ICollection<Flow>) UnregisterProvisionedFlowFromInterAppMessage(SLProtocol protocol, FlowInfoMessage message)
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

			var provisionedFlowId = message.ProvisionedFlowId;

			return UnregisterProvisionedFlow(protocol, provisionedFlowId);
		}

		public (ProvisionedFlow, ICollection<Flow>) UnregisterProvisionedFlow(SLProtocol protocol, Guid provisionedFlowId)
		{
			if (protocol == null)
			{
				throw new ArgumentNullException(nameof(protocol));
			}

			var unlinkedFlows = new List<Flow>();

			if (ProvisionedFlows.TryGetValue(provisionedFlowId, out var provisionedFlow))
			{
				unlinkedFlows.AddRange(IncomingFlows.UnlinkFlowEngineeringFlows(provisionedFlowId));
				unlinkedFlows.AddRange(OutgoingFlows.UnlinkFlowEngineeringFlows(provisionedFlowId));

				if (unlinkedFlows.Count > 0)
				{
					UpdateTables(protocol);
				}
				else
				{
					provisionedFlow.RemoveFromTable(protocol);
				}
			}

			return (provisionedFlow, unlinkedFlows);
		}

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			FlowEngineeringManagerInstances.RemoveInstance(this);
		}

		~FlowEngineeringManager()
		{
			Dispose(false);
		}

		#endregion
	}
}