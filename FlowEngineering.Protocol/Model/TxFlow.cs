namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;

	using Skyline.DataMiner.Scripting;

	public class TxFlow : Flow
	{
		public TxFlow(string instance) : base(instance)
		{
		}

		public string OutgoingInterface
		{
			get { return Interface; }
			set { Interface = value; }
		}

		public string ForeignKeyIncoming { get; set; } = String.Empty;

		public override void SetOrAddRow(SLProtocol protocol, bool includeStatistics = true)
		{
			var row = BuildRow(includeStatistics);
			protocol.SetOrAddRow(FleParameters.Fleoutgoingflowstable.tablePid, row);
		}

		public override void RemoveRow(SLProtocol protocol)
		{
			var key = Convert.ToString(Instance);
			protocol.DeleteRow(FleParameters.Fleoutgoingflowstable.tablePid, key);
		}

		public override QActionTableRow BuildRow(bool includeStatistics = true)
		{
			var row = new QActionTableRow(0, 15);

			row.Columns[FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstableinstance] = Instance;
			row.Columns[FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstabledestinationip] = DestinationIP;
			row.Columns[FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstabledestinationport] = DestinationPort;
			row.Columns[FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstablesourceip] = SourceIP;
			row.Columns[FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstableoutgoinginterface] = OutgoingInterface;
			row.Columns[FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstabletransporttype] = (int)TransportType;
			row.Columns[FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstablelabel] = Label ?? String.Empty;
			row.Columns[FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstablefkincoming] = ForeignKeyIncoming;
			row.Columns[FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstablelinkedflow] = LinkedFlow ?? String.Empty;
			row.Columns[FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstableflowowner] = (int)FlowOwner;
			row.Columns[FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstablepresent] = IsPresent ? 1 : 0;

			if (includeStatistics)
			{
				row.Columns[FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstabletxbitrate] = Bitrate;
				row.Columns[FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstableexpectedtxbitrate] = ExpectedBitrate;
				row.Columns[FleParameters.Fleoutgoingflowstable.Idx.fleoutgoingflowstableexpectedtxbitratestatus] = (int)ExpectedBitrateStatus;
			}

			return row;
		}
	}
}