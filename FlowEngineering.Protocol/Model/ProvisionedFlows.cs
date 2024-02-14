namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Core.DataMinerSystem.Protocol;
	using Skyline.DataMiner.Scripting;

	public class ProvisionedFlows : ConcurrentDictionary<Guid, ProvisionedFlow>
	{
		private readonly FlowEngineeringManager _manager;

		public ProvisionedFlows(FlowEngineeringManager manager)
		{
			_manager = manager;
		}

		public void Add(ProvisionedFlow provisionedFlow)
		{
			if (provisionedFlow == null)
			{
				throw new ArgumentNullException(nameof(provisionedFlow));
			}

			TryAdd(provisionedFlow.ID, provisionedFlow);
		}

		public ProvisionedFlow GetOrAdd(Guid id)
		{
			if (!TryGetValue(id, out var provisionedFlow))
			{
				provisionedFlow = new ProvisionedFlow(id);
				Add(provisionedFlow);
			}

			return provisionedFlow;
		}

		public void AddRange(IEnumerable<ProvisionedFlow> provisionedFlows)
		{
			if (provisionedFlows == null)
			{
				throw new ArgumentNullException(nameof(provisionedFlows));
			}

			foreach (var provisionedFlow in provisionedFlows)
			{
				Add(provisionedFlow);
			}
		}

		public bool Remove(ProvisionedFlow provisionedFlow)
		{
			if (provisionedFlow == null)
			{
				throw new ArgumentNullException(nameof(provisionedFlow));
			}

			return TryRemove(provisionedFlow.ID, out _);
		}

		public void LoadTable(SLProtocol protocol)
		{
			var table = protocol.GetLocalElement()
				.GetTable(FleParameters.Fleprovisionedflowstable.tablePid)
				.GetColumns(
					new uint[]
					{
						FleParameters.Fleprovisionedflowstable.Idx.fleprovisionedflowstableid,
						FleParameters.Fleprovisionedflowstable.Idx.fleprovisionedflowstablesourceid,
						FleParameters.Fleprovisionedflowstable.Idx.fleprovisionedflowstabledestinationid,
						FleParameters.Fleprovisionedflowstable.Idx.fleprovisionedflowstableincomingdcfinterfaceid,
						FleParameters.Fleprovisionedflowstable.Idx.fleprovisionedflowstableincomingdcfinterfacedynamiclink,
						FleParameters.Fleprovisionedflowstable.Idx.fleprovisionedflowstableoutgoingdcfinterfaceid,
						FleParameters.Fleprovisionedflowstable.Idx.fleprovisionedflowstableoutgoingdcfinterfacedynamiclink,
						FleParameters.Fleprovisionedflowstable.Idx.fleprovisionedflowstableoptionalsourceidentifier,
						FleParameters.Fleprovisionedflowstable.Idx.fleprovisionedflowstableoptionaldestinationidentifier,
						FleParameters.Fleprovisionedflowstable.Idx.fleprovisionedflowstabledestinationip,
						FleParameters.Fleprovisionedflowstable.Idx.fleprovisionedflowstabledestinationport,
						FleParameters.Fleprovisionedflowstable.Idx.fleprovisionedflowstablesourceip,
						FleParameters.Fleprovisionedflowstable.Idx.fleprovisionedflowstablemetadata,
					},
					(string id, string sourceId, string destId, int rxDcfId, string rxDcfLink, int txDcfId, string txDcfLink, string sourceIdf, string destIdf, string destIp, int destPort, string sourceIp, string metadata) =>
					{
						return new
						{
							Id = id,
							SourceId = sourceId,
							DestId = destId,
							RxDcfId = rxDcfId,
							RxDcfLink = rxDcfLink,
							TxDcfId = txDcfId,
							TxDcfLink = txDcfLink,
							SourceIdf = sourceIdf,
							DestIdf = destIdf,
							DestIp = destIp,
							DestPort = destPort,
							SourceIp = sourceIp,
							Metadata = metadata,
						};
					});

			foreach (var row in table)
			{
				var id = Guid.Parse(row.Id);

				if (!TryGetValue(id, out var provisionedFlow))
				{
					provisionedFlow = new ProvisionedFlow(id);
					Add(provisionedFlow);
				}

				provisionedFlow.SourceID = Guid.Parse(row.SourceId);
				provisionedFlow.DestinationID = Guid.Parse(row.DestId);
				provisionedFlow.IncomingDcfInterfaceID = row.RxDcfId;
				provisionedFlow.IncomingDcfDynamicLink = row.RxDcfLink;
				provisionedFlow.OutgoingDcfInterfaceID = row.TxDcfId;
				provisionedFlow.OutgoingDcfDynamicLink = row.TxDcfLink;
				provisionedFlow.OptionalSourceIdentifier = row.SourceIdf;
				provisionedFlow.OptionalDestinationIdentifier = row.DestIdf;
				provisionedFlow.DestinationIP = row.DestIp;
				provisionedFlow.DestinationPort = row.DestPort;
				provisionedFlow.SourceIP = row.SourceIp;
				provisionedFlow.Metadata = row.Metadata;
			}
		}

		public void UpdateTable(SLProtocol protocol)
		{
			var columns = new List<(int Pid, IEnumerable<object> Data)>
			{
				(FleParameters.Fleprovisionedflowstable.Pid.fleprovisionedflowstablesourceid, Values.Select(x => Convert.ToString(x.SourceID))),
				(FleParameters.Fleprovisionedflowstable.Pid.fleprovisionedflowstabledestinationid, Values.Select(x => Convert.ToString(x.DestinationID))),
				(FleParameters.Fleprovisionedflowstable.Pid.fleprovisionedflowstableincomingdcfinterfaceid, Values.Select(x => (object)x.IncomingDcfInterfaceID)),
				(FleParameters.Fleprovisionedflowstable.Pid.fleprovisionedflowstableincomingdcfinterfacedynamiclink, Values.Select(x => x.IncomingDcfDynamicLink)),
				(FleParameters.Fleprovisionedflowstable.Pid.fleprovisionedflowstableoutgoingdcfinterfaceid, Values.Select(x => (object)x.OutgoingDcfInterfaceID)),
				(FleParameters.Fleprovisionedflowstable.Pid.fleprovisionedflowstableoutgoingdcfinterfacedynamiclink, Values.Select(x => x.OutgoingDcfDynamicLink)),
				(FleParameters.Fleprovisionedflowstable.Pid.fleprovisionedflowstableoptionalsourceidentifier, Values.Select(x => x.OptionalSourceIdentifier)),
				(FleParameters.Fleprovisionedflowstable.Pid.fleprovisionedflowstableoptionaldestinationidentifier, Values.Select(x => x.OptionalDestinationIdentifier)),
				(FleParameters.Fleprovisionedflowstable.Pid.fleprovisionedflowstabledestinationip, Values.Select(x => x.DestinationIP)),
				(FleParameters.Fleprovisionedflowstable.Pid.fleprovisionedflowstabledestinationport, Values.Select(x => (object)x.DestinationPort)),
				(FleParameters.Fleprovisionedflowstable.Pid.fleprovisionedflowstablesourceip, Values.Select(x => x.SourceIP)),
				(FleParameters.Fleprovisionedflowstable.Pid.fleprovisionedflowstablemetadata, Values.Select(x => x.Metadata)),
			};

			protocol.SetColumns(
				FleParameters.Fleprovisionedflowstable.tablePid,
				deleteOldRows: true,
				Values.Select(x => Convert.ToString(x.ID)).ToArray(),
				columns.ToArray());
		}

		public void AddToTable(SLProtocol protocol, ProvisionedFlow provisionedFlow)
		{
			if (provisionedFlow == null)
			{
				throw new ArgumentNullException(nameof(provisionedFlow));
			}

			provisionedFlow.SetOrAddRow(protocol);
		}

		public void RemoveFromTable(SLProtocol protocol, ProvisionedFlow provisionedFlow)
		{
			if (provisionedFlow == null)
			{
				throw new ArgumentNullException(nameof(provisionedFlow));
			}

			provisionedFlow.RemoveRow(protocol);
		}

		public void RemoveFromTable(SLProtocol protocol, Guid provisionedFlowId)
		{
			var key = Convert.ToString(provisionedFlowId);
			protocol.DeleteRow(FleParameters.Fleprovisionedflowstable.tablePid, key);
		}
	}
}
