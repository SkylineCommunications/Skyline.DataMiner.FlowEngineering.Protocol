namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Core.DataMinerSystem.Protocol;
	using Skyline.DataMiner.Scripting;

	public class ProvisionedFlows : Dictionary<Guid, ProvisionedFlow>
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

			Add(provisionedFlow.ID, provisionedFlow);
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

			return Remove(provisionedFlow.ID);
		}

		public void LoadTable(SLProtocol protocol)
		{
			var table = protocol.GetLocalElement()
				.GetTable(Parameter.Fleprovisionedflowstable.tablePid)
				.GetColumns(
					new uint[]
					{
						Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstableid,
						Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstablesourceid,
						Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstabledestinationid,
						Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstableincomingdcfinterfaceid,
						Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstableincomingdcfinterfacedynamiclink,
						Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstableoutgoingdcfinterfaceid,
						Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstableoutgoingdcfinterfacedynamiclink,
						Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstableoptionalsourceidentifier,
						Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstableoptionaldestinationidentifier,
						Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstabledestinationip,
						Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstabledestinationport,
						Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstablesourceip,
						Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstablemetadata,
					},
					(string id, string sourceId, string destId, int inDcfId, string inDcfLink, int outDcfId, string outDcfLink, string sourceIdf, string destIdf, string destIp, int destPort, string sourceIp, string metadata) =>
					{
						return new
						{
							Id = id,
							SourceId = sourceId,
							DestId = destId,
							InDcfId = inDcfId,
							InDcfLink = inDcfLink,
							OutDcfId = outDcfId,
							OutDcfLink = outDcfLink,
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
				provisionedFlow.IncomingDcfInterfaceID = row.InDcfId;
				provisionedFlow.IncomingDcfDynamicLink = row.InDcfLink;
				provisionedFlow.OutgoingDcfInterfaceID = row.OutDcfId;
				provisionedFlow.OutgoingDcfDynamicLink = row.OutDcfLink;
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
				(Parameter.Fleprovisionedflowstable.Pid.fleprovisionedflowstablesourceid, Values.Select(x => Convert.ToString(x.SourceID))),
				(Parameter.Fleprovisionedflowstable.Pid.fleprovisionedflowstabledestinationid, Values.Select(x => Convert.ToString(x.DestinationID))),
				(Parameter.Fleprovisionedflowstable.Pid.fleprovisionedflowstableincomingdcfinterfaceid, Values.Select(x => (object)x.IncomingDcfInterfaceID)),
				(Parameter.Fleprovisionedflowstable.Pid.fleprovisionedflowstableincomingdcfinterfacedynamiclink, Values.Select(x => x.IncomingDcfDynamicLink)),
				(Parameter.Fleprovisionedflowstable.Pid.fleprovisionedflowstableoutgoingdcfinterfaceid, Values.Select(x => (object)x.OutgoingDcfInterfaceID)),
				(Parameter.Fleprovisionedflowstable.Pid.fleprovisionedflowstableoutgoingdcfinterfacedynamiclink, Values.Select(x => x.OutgoingDcfDynamicLink)),
				(Parameter.Fleprovisionedflowstable.Pid.fleprovisionedflowstableoptionalsourceidentifier, Values.Select(x => x.OptionalSourceIdentifier)),
				(Parameter.Fleprovisionedflowstable.Pid.fleprovisionedflowstableoptionaldestinationidentifier, Values.Select(x => x.OptionalDestinationIdentifier)),
				(Parameter.Fleprovisionedflowstable.Pid.fleprovisionedflowstabledestinationip, Values.Select(x => x.DestinationIP)),
				(Parameter.Fleprovisionedflowstable.Pid.fleprovisionedflowstabledestinationport, Values.Select(x => (object)x.DestinationPort)),
				(Parameter.Fleprovisionedflowstable.Pid.fleprovisionedflowstablesourceip, Values.Select(x => x.SourceIP)),
				(Parameter.Fleprovisionedflowstable.Pid.fleprovisionedflowstablemetadata, Values.Select(x => x.Metadata)),
			};

			protocol.SetColumns(
				Parameter.Fleprovisionedflowstable.tablePid,
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

			provisionedFlow.AddToTable(protocol);
		}

		public void RemoveFromTable(SLProtocol protocol, ProvisionedFlow provisionedFlow)
		{
			if (provisionedFlow == null)
			{
				throw new ArgumentNullException(nameof(provisionedFlow));
			}

			provisionedFlow.RemoveFromTable(protocol);
		}

		public void RemoveFromTable(SLProtocol protocol, Guid provisionedFlowId)
		{
			var key = Convert.ToString(provisionedFlowId);
			protocol.DeleteRow(Parameter.Fleprovisionedflowstable.tablePid, key);
		}
	}
}
