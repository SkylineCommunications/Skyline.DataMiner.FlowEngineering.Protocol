namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Core.DataMinerSystem.Protocol;
	using Skyline.DataMiner.FlowEngineering.Protocol;
	using Skyline.DataMiner.FlowEngineering.Protocol.DCF;
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

		public override RxFlow RegisterFlowEngineeringFlow(ConnectorAPI.FlowEngineering.Info.FlowInfoMessage flowInfo, bool ignoreDestinationPort = false)
		{
			if (flowInfo == null)
			{
				throw new ArgumentNullException(nameof(flowInfo));
			}

			var ip = flowInfo.IpConfiguration;
			if (ip == null)
			{
				throw new NotSupportedException("Only IP flows are supported");
			}

			if (!DcfDynamicLink.TryParse(flowInfo.IncomingDcfDynamicLink, out var dcfDynamicLink))
			{
				throw new ArgumentException("Couldn't parse DCF dynamic link");
			}

			var instance = !ignoreDestinationPort
				? String.Join("/", ip.SourceIP, $"{ip.DestinationIP}:{ip.DestinationPort}")
				: String.Join("/", ip.SourceIP, ip.DestinationIP);

			if (!TryGetValue(instance, out var flow))
			{
				flow = new RxFlow(instance)
				{
					SourceIP = ip.SourceIP,
					DestinationIP = ip.DestinationIP,
					DestinationPort = !ignoreDestinationPort ? Convert.ToInt32(ip.DestinationPort) : -1,
					TransportType = FlowTransportType.IP,
				};
				Add(flow);
			}

			flow.FlowOwner = FlowOwner.FlowEngineering;
			flow.LinkedFlow = Convert.ToString(flowInfo.ProvisionedFlowId);
			flow.IncomingInterface = dcfDynamicLink.PK;
			flow.ExpectedBitrate = flowInfo.GetBitrate();

			return flow;
		}

		public override RxFlow UnregisterFlowEngineeringFlow(ConnectorAPI.FlowEngineering.Info.FlowInfoMessage flowInfo, bool ignoreDestinationPort = false)
		{
			if (flowInfo == null)
			{
				throw new ArgumentNullException(nameof(flowInfo));
			}

			var ip = flowInfo.IpConfiguration;
			if (ip == null)
			{
				throw new NotSupportedException("Only IP flows are supported");
			}

			var instance = !ignoreDestinationPort
				? String.Join("/", ip.SourceIP, $"{ip.DestinationIP}:{ip.DestinationPort}")
				: String.Join("/", ip.SourceIP, ip.DestinationIP);

			if (!TryGetValue(instance, out var flow))
			{
				return null;
			}

			if (flow.IsPresent)
			{
				flow.FlowOwner = FlowOwner.LocalSystem;
				flow.LinkedFlow = String.Empty;
				flow.ExpectedBitrate = -1;
			}
			else
			{
				Remove(instance);
			}

			return flow;
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
					(string idx, string dest, int destPort, string source, string intf, int type, double bitrate, double expectedBitrate, string label, string fkOut, string linked, int owner, int present) =>
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
							FkOutgoing = fkOut,
							LinkedFlow = linked,
							FlowOwner = (FlowOwner)owner,
							IsPresent = Convert.ToBoolean(present),
						};
					}
				);

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