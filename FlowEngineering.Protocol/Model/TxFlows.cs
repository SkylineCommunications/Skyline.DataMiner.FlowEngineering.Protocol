namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.ConnectorAPI.FlowEngineering.Info;
	using Skyline.DataMiner.Core.DataMinerSystem.Protocol;
	using Skyline.DataMiner.FlowEngineering.Protocol;
	using Skyline.DataMiner.FlowEngineering.Protocol.Exceptions;
	using Skyline.DataMiner.Scripting;

	public class TxFlows : Flows<TxFlow>
	{
		public TxFlows(FlowEngineeringManager manager) : base(manager)
		{
		}

		public TxFlow GetOrAdd(string instance)
		{
			if (String.IsNullOrWhiteSpace(instance))
			{
				throw new ArgumentException($"'{nameof(instance)}' cannot be null or whitespace.", nameof(instance));
			}

			if (!TryGetValue(instance, out var flow))
			{
				flow = new TxFlow(instance);
				Add(flow);
			}

			return flow;
		}

		public override TxFlow RegisterFlowEngineeringFlow(FlowInfoMessage flowInfo, string instance, bool ignoreDestinationPort = false)
		{
			if (flowInfo == null)
			{
				throw new ArgumentNullException(nameof(flowInfo));
			}

			if (String.IsNullOrWhiteSpace(instance))
			{
				throw new ArgumentException($"'{nameof(instance)}' cannot be null or whitespace.", nameof(instance));
			}

			if (!_manager.Interfaces.TryGetByDcfInterfaceID(flowInfo.OutgoingDcfInterfaceID, out var outgoingIntf) &&
				!_manager.Interfaces.TryGetByDcfDynamicLink(flowInfo.OutgoingDcfDynamicLink, out outgoingIntf))
			{
				throw new DcfInterfaceNotFoundException($"Couldn't find outgoing DCF interface with ID '{flowInfo.OutgoingDcfInterfaceID}' and link '{flowInfo.OutgoingDcfDynamicLink}'");
			}

			if (!TryGetValue(instance, out var flow))
			{
				flow = new TxFlow(instance);
				Add(flow);
			}

			var ip = flowInfo.IpConfiguration;
			if (ip != null)
			{
				flow.SourceIP = ip.SourceIP;
				flow.DestinationIP = ip.DestinationIP;
				flow.DestinationPort = !ignoreDestinationPort ? Convert.ToInt32(ip.DestinationPort) : -1;
				flow.TransportType = FlowTransportType.IP;
			}
			else
			{
				flow.SourceIP = String.Empty;
				flow.DestinationIP = String.Empty;
				flow.DestinationPort = -1;
			}

			flow.FlowOwner = FlowOwner.FlowEngineering;
			flow.LinkedFlow = Convert.ToString(flowInfo.ProvisionedFlowId);
			flow.OutgoingInterface = outgoingIntf.Index;
			flow.ExpectedBitrate = flowInfo.TryGetBitrate(out var bitrate) ? bitrate : -1;

			return flow;
		}

		public override TxFlow UnregisterFlowEngineeringFlow(FlowInfoMessage flowInfo)
		{
			if (flowInfo == null)
			{
				throw new ArgumentNullException(nameof(flowInfo));
			}

			var provisionedFlowId = Convert.ToString(flowInfo.ProvisionedFlowId);
			var linkedFlow = Values.FirstOrDefault(x => String.Equals(x.LinkedFlow, provisionedFlowId));

			if (linkedFlow == null)
			{
				throw new ArgumentException($"Couldn't find outgoing flow with provisioned flow ID '{provisionedFlowId}'");
			}

			if (linkedFlow.IsPresent)
			{
				linkedFlow.FlowOwner = FlowOwner.LocalSystem;
				linkedFlow.LinkedFlow = String.Empty;
				linkedFlow.ExpectedBitrate = -1;
			}
			else
			{
				Remove(linkedFlow);
			}

			return linkedFlow;
		}

		public override void LoadTable(SLProtocol protocol)
		{
			var table = protocol.GetLocalElement()
				.GetTable(Parameter.Fleoutgoingflowstable.tablePid)
				.GetColumns(
					new uint[]
					{
						Parameter.Fleoutgoingflowstable.Idx.fleoutgoingflowstableinstance,
						Parameter.Fleoutgoingflowstable.Idx.fleoutgoingflowstabledestinationip,
						Parameter.Fleoutgoingflowstable.Idx.fleoutgoingflowstabledestinationport,
						Parameter.Fleoutgoingflowstable.Idx.fleoutgoingflowstablesourceip,
						Parameter.Fleoutgoingflowstable.Idx.fleoutgoingflowstableoutgoinginterface,
						Parameter.Fleoutgoingflowstable.Idx.fleoutgoingflowstabletransporttype,
						Parameter.Fleoutgoingflowstable.Idx.fleoutgoingflowstabletxbitrate,
						Parameter.Fleoutgoingflowstable.Idx.fleoutgoingflowstableexpectedtxbitrate,
						Parameter.Fleoutgoingflowstable.Idx.fleoutgoingflowstablelabel,
						Parameter.Fleoutgoingflowstable.Idx.fleoutgoingflowstablefkincoming,
						Parameter.Fleoutgoingflowstable.Idx.fleoutgoingflowstablelinkedflow,
						Parameter.Fleoutgoingflowstable.Idx.fleoutgoingflowstableflowowner,
						Parameter.Fleoutgoingflowstable.Idx.fleoutgoingflowstablepresent,
					},
					(string idx, string dest, int destPort, string source, string intf, int type, double bitrate, double expectedBitrate, string label, string fk, string linked, int owner, int present) =>
					{
						return new
						{
							Idx = idx,
							DestinationIP = dest,
							DestinationPort = destPort,
							SourceIP = source,
							Interface = intf,
							TransportType = (FlowTransportType)type,
							Bitrate = bitrate,
							ExpectedBitrate = expectedBitrate,
							Label = label,
							FkIncoming = fk,
							LinkedFlow = linked,
							FlowOwner = (FlowOwner)owner,
							IsPresent = Convert.ToBoolean(present),
						};
					});

			foreach (var row in table)
			{
				if (!TryGetValue(row.Idx, out var flow))
				{
					flow = new TxFlow(row.Idx);
					Add(flow);
				}

				flow.DestinationIP = row.DestinationIP;
				flow.DestinationPort = row.DestinationPort;
				flow.SourceIP = row.SourceIP;
				flow.Interface = row.Interface;
				flow.TransportType = row.TransportType;
				flow.Bitrate = row.Bitrate;
				flow.ExpectedBitrate = row.ExpectedBitrate;
				flow.Label = row.Label;
				flow.ForeignKeyIncoming = row.FkIncoming;
				flow.LinkedFlow = row.LinkedFlow;
				flow.FlowOwner = row.FlowOwner;
				flow.IsPresent = row.IsPresent;
			}
		}

		public override void UpdateTable(SLProtocol protocol, bool includeStatistics = true)
		{
			var columns = new List<(int Pid, IEnumerable<object> Data)>
			{
				(Parameter.Fleoutgoingflowstable.Pid.fleoutgoingflowstabledestinationip, Values.Select(x => x.DestinationIP)),
				(Parameter.Fleoutgoingflowstable.Pid.fleoutgoingflowstabledestinationport, Values.Select(x => (object)x.DestinationPort)),
				(Parameter.Fleoutgoingflowstable.Pid.fleoutgoingflowstablesourceip, Values.Select(x => x.SourceIP)),
				(Parameter.Fleoutgoingflowstable.Pid.fleoutgoingflowstableoutgoinginterface, Values.Select(x => x.Interface)),
				(Parameter.Fleoutgoingflowstable.Pid.fleoutgoingflowstablefkincoming, Values.Select(x => x.ForeignKeyIncoming)),
				(Parameter.Fleoutgoingflowstable.Pid.fleoutgoingflowstabletransporttype, Values.Select(x => (object)x.TransportType)),
				(Parameter.Fleoutgoingflowstable.Pid.fleoutgoingflowstablelabel, Values.Select(x => x.Label ?? String.Empty)),
				(Parameter.Fleoutgoingflowstable.Pid.fleoutgoingflowstablelinkedflow, Values.Select(x => x.LinkedFlow ?? String.Empty)),
				(Parameter.Fleoutgoingflowstable.Pid.fleoutgoingflowstableflowowner, Values.Select(x => (object)x.FlowOwner)),
				(Parameter.Fleoutgoingflowstable.Pid.fleoutgoingflowstablepresent, Values.Select(x => (object)(x.IsPresent ? 1 : 0))),
			};

			if (includeStatistics)
			{
				columns.AddRange(GetStatisticsColumns());
			}

			protocol.SetColumns(
				Parameter.Fleoutgoingflowstable.tablePid,
				deleteOldRows: true,
				Values.Select(x => x.Instance).ToArray(),
				columns.ToArray());
		}

		public override void UpdateStatistics(SLProtocol protocol)
		{
			protocol.SetColumns(
				Parameter.Fleoutgoingflowstable.tablePid,
				deleteOldRows: true,
				Values.Select(x => x.Instance).ToArray(),
				GetStatisticsColumns());
		}

		private (int Pid, IEnumerable<object> Data)[] GetStatisticsColumns()
		{
			return new[]
			{
				(Parameter.Fleoutgoingflowstable.Pid.fleoutgoingflowstabletxbitrate, Values.Select(x => (object)x.Bitrate)),
				(Parameter.Fleoutgoingflowstable.Pid.fleoutgoingflowstableexpectedtxbitrate, Values.Select(x => (object)x.ExpectedBitrate)),
				(Parameter.Fleoutgoingflowstable.Pid.fleoutgoingflowstableexpectedtxbitratestatus, Values.Select(x => (object)x.ExpectedBitrateStatus)),
			};
		}
	}
}