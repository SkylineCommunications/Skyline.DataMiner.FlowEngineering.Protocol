namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Core.DataMinerSystem.Protocol;
	using Skyline.DataMiner.FlowEngineering.Protocol;
	using Skyline.DataMiner.FlowEngineering.Protocol.Enums;
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

		public override void LoadTable(SLProtocol protocol)
		{
			var table = protocol.GetLocalElement()
				.GetTable(FleParameters.Fleoutgoingflowstable.tablePid)
				.GetColumns(
					new uint[]
					{
						FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstableinstance,
						FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstabledestinationip,
						FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstabledestinationport,
						FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstablesourceip,
						FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstableoutgoinginterface,
						FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstabletransporttype,
						FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstabletxbitrate,
						FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstableexpectedtxbitrate,
						FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstablelabel,
						FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstablefkincoming,
						FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstablelinkedflow,
						FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstablepresent,
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
							FkIncoming = fk,
							LinkedFlow = linked,
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
				flow.IsPresent = row.IsPresent;
			}
		}

		public override void UpdateTable(SLProtocol protocol, bool includeStatistics = true)
		{
			var columns = new List<(int Pid, IEnumerable<object> Data)>
			{
				(FleParameters.Fleoutgoingflowstable.Pid.fleoutgoingflowstabledestinationip, Values.Select(x => x.DestinationIP)),
				(FleParameters.Fleoutgoingflowstable.Pid.fleoutgoingflowstabledestinationport, Values.Select(x => (object)x.DestinationPort)),
				(FleParameters.Fleoutgoingflowstable.Pid.fleoutgoingflowstablesourceip, Values.Select(x => x.SourceIP)),
				(FleParameters.Fleoutgoingflowstable.Pid.fleoutgoingflowstableoutgoinginterface, Values.Select(x => x.Interface)),
				(FleParameters.Fleoutgoingflowstable.Pid.fleoutgoingflowstablefkincoming, Values.Select(x => x.ForeignKeyIncoming)),
				(FleParameters.Fleoutgoingflowstable.Pid.fleoutgoingflowstabletransporttype, Values.Select(x => (object)x.TransportType)),
				(FleParameters.Fleoutgoingflowstable.Pid.fleoutgoingflowstablelabel, Values.Select(x => x.Label ?? String.Empty)),
				(FleParameters.Fleoutgoingflowstable.Pid.fleoutgoingflowstablelinkedflow, Values.Select(x => x.LinkedFlow ?? String.Empty)),
				(FleParameters.Fleoutgoingflowstable.Pid.fleoutgoingflowstableflowowner, Values.Select(x => (object)x.FlowOwner)),
				(FleParameters.Fleoutgoingflowstable.Pid.fleoutgoingflowstablepresent, Values.Select(x => (object)(x.IsPresent ? 1 : 0))),
			};

			if (includeStatistics)
			{
				columns.AddRange(GetStatisticsColumns());
			}

			protocol.SetColumns(
				FleParameters.Fleoutgoingflowstable.tablePid,
				deleteOldRows: true,
				Values.Select(x => x.Instance).ToArray(),
				columns.ToArray());
		}

		public override void UpdateStatistics(SLProtocol protocol)
		{
			protocol.SetColumns(
				FleParameters.Fleoutgoingflowstable.tablePid,
				deleteOldRows: true,
				Values.Select(x => x.Instance).ToArray(),
				GetStatisticsColumns());
		}

		private (int Pid, IEnumerable<object> Data)[] GetStatisticsColumns()
		{
			return new[]
			{
				(FleParameters.Fleoutgoingflowstable.Pid.fleoutgoingflowstabletxbitrate, Values.Select(x => (object)x.Bitrate)),
				(FleParameters.Fleoutgoingflowstable.Pid.fleoutgoingflowstableexpectedtxbitrate, Values.Select(x => (object)x.ExpectedBitrate)),
				(FleParameters.Fleoutgoingflowstable.Pid.fleoutgoingflowstableexpectedtxbitratestatus, Values.Select(x => (object)x.ExpectedBitrateStatus)),
			};
		}
	}
}