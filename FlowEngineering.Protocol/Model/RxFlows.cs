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

	public class RxFlows : Flows<RxFlow>
	{
		public RxFlows(FlowEngineeringManager manager) : base(manager)
		{
		}

		public RxFlow GetOrAdd(string instance)
		{
			if (String.IsNullOrWhiteSpace(instance))
			{
				throw new ArgumentException($"'{nameof(instance)}' cannot be null or whitespace.", nameof(instance));
			}

			if (!TryGetValue(instance, out var flow))
			{
				flow = new RxFlow(instance);
				Add(flow);
			}

			return flow;
		}

		public override RxFlow RegisterFlowEngineeringFlow(FlowInfoMessage flowInfo, string instance, bool ignoreDestinationPort = false)
		{
			if (flowInfo == null)
			{
				throw new ArgumentNullException(nameof(flowInfo));
			}

			if (String.IsNullOrWhiteSpace(instance))
			{
				throw new ArgumentException($"'{nameof(instance)}' cannot be null or whitespace.", nameof(instance));
			}

			if (!_manager.Interfaces.TryGetByDcfInterfaceID(flowInfo.IncomingDcfInterfaceID, out var incomingIntf) &&
				!_manager.Interfaces.TryGetByDcfDynamicLink(flowInfo.IncomingDcfDynamicLink, out incomingIntf))
			{
				throw new DcfInterfaceNotFoundException($"Couldn't find incoming DCF interface with ID '{flowInfo.IncomingDcfInterfaceID}' and link '{flowInfo.IncomingDcfDynamicLink}'");
			}

			if (!TryGetValue(instance, out var flow))
			{
				flow = new RxFlow(instance);
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
			flow.IncomingInterface = incomingIntf.Index;
			flow.ExpectedBitrate = flowInfo.TryGetBitrate(out var bitrate) ? bitrate : -1;

			return flow;
		}

		public override RxFlow UnregisterFlowEngineeringFlow(FlowInfoMessage flowInfo)
		{
			if (flowInfo == null)
			{
				throw new ArgumentNullException(nameof(flowInfo));
			}

			var provisionedFlowId = Convert.ToString(flowInfo.ProvisionedFlowId);
			var linkedFlow = Values.FirstOrDefault(x => String.Equals(x.LinkedFlow, provisionedFlowId));

			if (linkedFlow == null)
			{
				throw new ArgumentException($"Couldn't find incoming flow with provisioned flow ID '{provisionedFlowId}'");
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
				.GetTable(Parameter.Fleincomingflowstable.tablePid)
				.GetColumns(
					new uint[]
					{
						Parameter.Fleincomingflowstable.Idx.fleincomingflowstableinstance,
						Parameter.Fleincomingflowstable.Idx.fleincomingflowstabledestinationip,
						Parameter.Fleincomingflowstable.Idx.fleincomingflowstabledestinationport,
						Parameter.Fleincomingflowstable.Idx.fleincomingflowstablesourceip,
						Parameter.Fleincomingflowstable.Idx.fleincomingflowstableincominginterface,
						Parameter.Fleincomingflowstable.Idx.fleincomingflowstabletransporttype,
						Parameter.Fleincomingflowstable.Idx.fleincomingflowstablerxbitrate,
						Parameter.Fleincomingflowstable.Idx.fleincomingflowstableexpectedrxbitrate,
						Parameter.Fleincomingflowstable.Idx.fleincomingflowstablelabel,
						Parameter.Fleincomingflowstable.Idx.fleincomingflowstablefkoutgoing,
						Parameter.Fleincomingflowstable.Idx.fleincomingflowstablelinkedflow,
						Parameter.Fleincomingflowstable.Idx.fleincomingflowstableflowowner,
						Parameter.Fleincomingflowstable.Idx.fleincomingflowstablepresent,
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
							FkOutgoing = fk,
							LinkedFlow = linked,
							FlowOwner = (FlowOwner)owner,
							IsPresent = Convert.ToBoolean(present),
						};
					});

			foreach (var row in table)
			{
				if (!TryGetValue(row.Idx, out var flow))
				{
					flow = new RxFlow(row.Idx);
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
				flow.ForeignKeyOutgoing = row.FkOutgoing;
				flow.LinkedFlow = row.LinkedFlow;
				flow.FlowOwner = row.FlowOwner;
				flow.IsPresent = row.IsPresent;
			}
		}

		public override void UpdateTable(SLProtocol protocol, bool includeStatistics = true)
		{
			var columns = new List<(int Pid, IEnumerable<object> Data)>
			{
				(Parameter.Fleincomingflowstable.Pid.fleincomingflowstabledestinationip, Values.Select(x => x.DestinationIP)),
				(Parameter.Fleincomingflowstable.Pid.fleincomingflowstabledestinationport, Values.Select(x => (object)x.DestinationPort)),
				(Parameter.Fleincomingflowstable.Pid.fleincomingflowstablesourceip, Values.Select(x => x.SourceIP)),
				(Parameter.Fleincomingflowstable.Pid.fleincomingflowstableincominginterface, Values.Select(x => x.Interface)),
				(Parameter.Fleincomingflowstable.Pid.fleincomingflowstablefkoutgoing, Values.Select(x => x.ForeignKeyOutgoing)),
				(Parameter.Fleincomingflowstable.Pid.fleincomingflowstabletransporttype, Values.Select(x => (object)x.TransportType)),
				(Parameter.Fleincomingflowstable.Pid.fleincomingflowstablelabel, Values.Select(x => x.Label ?? String.Empty)),
				(Parameter.Fleincomingflowstable.Pid.fleincomingflowstablelinkedflow, Values.Select(x => x.LinkedFlow ?? String.Empty)),
				(Parameter.Fleincomingflowstable.Pid.fleincomingflowstableflowowner, Values.Select(x => (object)x.FlowOwner)),
				(Parameter.Fleincomingflowstable.Pid.fleincomingflowstablepresent, Values.Select(x => (object)(x.IsPresent ? 1 : 0))),
			};

			if (includeStatistics)
			{
				columns.AddRange(GetStatisticsColumns());
			}

			protocol.SetColumns(
				Parameter.Fleincomingflowstable.tablePid,
				deleteOldRows: true,
				Values.Select(x => x.Instance).ToArray(),
				columns.ToArray());
		}

		public override void UpdateStatistics(SLProtocol protocol)
		{
			protocol.SetColumns(
				Parameter.Fleincomingflowstable.tablePid,
				deleteOldRows: true,
				Values.Select(x => x.Instance).ToArray(),
				GetStatisticsColumns());
		}

		private (int Pid, IEnumerable<object> Data)[] GetStatisticsColumns()
		{
			return new[]
			{
				(Parameter.Fleincomingflowstable.Pid.fleincomingflowstablerxbitrate, Values.Select(x => (object)x.Bitrate)),
				(Parameter.Fleincomingflowstable.Pid.fleincomingflowstableexpectedrxbitrate, Values.Select(x => (object)x.ExpectedBitrate)),
				(Parameter.Fleincomingflowstable.Pid.fleincomingflowstableexpectedrxbitratestatus, Values.Select(x => (object)x.ExpectedBitrateStatus)),
			};
		}
	}
}