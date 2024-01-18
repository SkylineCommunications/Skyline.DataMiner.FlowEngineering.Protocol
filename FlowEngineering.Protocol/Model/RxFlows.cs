namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Core.DataMinerSystem.Protocol;
	using Skyline.DataMiner.FlowEngineering.Protocol;
	using Skyline.DataMiner.FlowEngineering.Protocol.Enums;
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

		public override void LoadTable(SLProtocol protocol)
		{
			var table = protocol.GetLocalElement()
				.GetTable(FleParameters.Fleincomingflowstable.tablePid)
				.GetColumns(
					new uint[]
					{
						FleParameters.Fleincomingflowstable.Idx.fleincomingflowstableinstance,
						FleParameters.Fleincomingflowstable.Idx.fleincomingflowstabledestinationip,
						FleParameters.Fleincomingflowstable.Idx.fleincomingflowstabledestinationport,
						FleParameters.Fleincomingflowstable.Idx.fleincomingflowstablesourceip,
						FleParameters.Fleincomingflowstable.Idx.fleincomingflowstableincominginterface,
						FleParameters.Fleincomingflowstable.Idx.fleincomingflowstabletransporttype,
						FleParameters.Fleincomingflowstable.Idx.fleincomingflowstablerxbitrate,
						FleParameters.Fleincomingflowstable.Idx.fleincomingflowstableexpectedrxbitrate,
						FleParameters.Fleincomingflowstable.Idx.fleincomingflowstablelabel,
						FleParameters.Fleincomingflowstable.Idx.fleincomingflowstablefkoutgoing,
						FleParameters.Fleincomingflowstable.Idx.fleincomingflowstablelinkedflow,
						FleParameters.Fleincomingflowstable.Idx.fleincomingflowstablepresent,
					},
					(string idx, string dest, int destPort, string source, string intf, int type, double bitrate, double expectedBitrate, string label, string fk, string linked, int present) =>
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
				flow.IsPresent = row.IsPresent;
			}
		}

		public override void UpdateTable(SLProtocol protocol, bool includeStatistics = true)
		{
			var columns = new List<(int Pid, IEnumerable<object> Data)>
			{
				(FleParameters.Fleincomingflowstable.Pid.fleincomingflowstabledestinationip, Values.Select(x => x.DestinationIP)),
				(FleParameters.Fleincomingflowstable.Pid.fleincomingflowstabledestinationport, Values.Select(x => (object)x.DestinationPort)),
				(FleParameters.Fleincomingflowstable.Pid.fleincomingflowstablesourceip, Values.Select(x => x.SourceIP)),
				(FleParameters.Fleincomingflowstable.Pid.fleincomingflowstableincominginterface, Values.Select(x => x.Interface)),
				(FleParameters.Fleincomingflowstable.Pid.fleincomingflowstablefkoutgoing, Values.Select(x => x.ForeignKeyOutgoing)),
				(FleParameters.Fleincomingflowstable.Pid.fleincomingflowstabletransporttype, Values.Select(x => (object)x.TransportType)),
				(FleParameters.Fleincomingflowstable.Pid.fleincomingflowstablelabel, Values.Select(x => x.Label ?? String.Empty)),
				(FleParameters.Fleincomingflowstable.Pid.fleincomingflowstablelinkedflow, Values.Select(x => x.LinkedFlow ?? String.Empty)),
				(FleParameters.Fleincomingflowstable.Pid.fleincomingflowstableflowowner, Values.Select(x => (object)x.FlowOwner)),
				(FleParameters.Fleincomingflowstable.Pid.fleincomingflowstablepresent, Values.Select(x => (object)(x.IsPresent ? 1 : 0))),
			};

			if (includeStatistics)
			{
				columns.AddRange(GetStatisticsColumns());
			}

			protocol.SetColumns(
				FleParameters.Fleincomingflowstable.tablePid,
				deleteOldRows: true,
				Values.Select(x => x.Instance).ToArray(),
				columns.ToArray());
		}

		public override void UpdateStatistics(SLProtocol protocol)
		{
			protocol.SetColumns(
				FleParameters.Fleincomingflowstable.tablePid,
				deleteOldRows: true,
				Values.Select(x => x.Instance).ToArray(),
				GetStatisticsColumns());
		}

		private (int Pid, IEnumerable<object> Data)[] GetStatisticsColumns()
		{
			return new[]
			{
				(FleParameters.Fleincomingflowstable.Pid.fleincomingflowstablerxbitrate, Values.Select(x => (object)x.Bitrate)),
				(FleParameters.Fleincomingflowstable.Pid.fleincomingflowstableexpectedrxbitrate, Values.Select(x => (object)x.ExpectedBitrate)),
				(FleParameters.Fleincomingflowstable.Pid.fleincomingflowstableexpectedrxbitratestatus, Values.Select(x => (object)x.ExpectedBitrateStatus)),
			};
		}
	}
}