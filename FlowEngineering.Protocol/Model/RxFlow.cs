namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;

	using Skyline.DataMiner.Scripting;

	public class RxFlow : Flow
	{
		public RxFlow(string instance) : base(instance)
		{
		}

		public string IncomingInterface
		{
			get { return Interface; }
			set { Interface = value; }
		}

		public string ForeignKeyOutgoing { get; set; } = String.Empty;

		public override void SetOrAddRow(SLProtocol protocol, bool includeStatistics = true)
		{
			var row = BuildRow(includeStatistics);
			protocol.SetOrAddRow(FleParameters.Fleincomingflowstable.tablePid, row);
		}

		public override void RemoveRow(SLProtocol protocol)
		{
			var key = Convert.ToString(Instance);
			protocol.DeleteRow(FleParameters.Fleincomingflowstable.tablePid, key);
		}

		public override QActionTableRow BuildRow(bool includeStatistics = true)
		{
			var row = new QActionTableRow(0, 15);

			row.Columns[FleParameters.Fleincomingflowstable.Idx.fleincomingflowstableinstance] = Instance;
			row.Columns[FleParameters.Fleincomingflowstable.Idx.fleincomingflowstabledestinationip] = DestinationIP;
			row.Columns[FleParameters.Fleincomingflowstable.Idx.fleincomingflowstabledestinationport] = DestinationPort;
			row.Columns[FleParameters.Fleincomingflowstable.Idx.fleincomingflowstablesourceip] = SourceIP;
			row.Columns[FleParameters.Fleincomingflowstable.Idx.fleincomingflowstableincominginterface] = IncomingInterface;
			row.Columns[FleParameters.Fleincomingflowstable.Idx.fleincomingflowstabletransporttype] = (int)TransportType;
			row.Columns[FleParameters.Fleincomingflowstable.Idx.fleincomingflowstablelabel] = Label ?? String.Empty;
			row.Columns[FleParameters.Fleincomingflowstable.Idx.fleincomingflowstablefkoutgoing] = ForeignKeyOutgoing;
			row.Columns[FleParameters.Fleincomingflowstable.Idx.fleincomingflowstablelinkedflow] = LinkedFlow ?? String.Empty;
			row.Columns[FleParameters.Fleincomingflowstable.Idx.fleincomingflowstableflowowner] = (int)FlowOwner;
			row.Columns[FleParameters.Fleincomingflowstable.Idx.fleincomingflowstablepresent] = IsPresent ? 1 : 0;

			if (includeStatistics)
			{
				row.Columns[FleParameters.Fleincomingflowstable.Idx.fleincomingflowstablerxbitrate] = Bitrate;
				row.Columns[FleParameters.Fleincomingflowstable.Idx.fleincomingflowstableexpectedrxbitrate] = ExpectedBitrate;
				row.Columns[FleParameters.Fleincomingflowstable.Idx.fleincomingflowstableexpectedrxbitratestatus] = (int)ExpectedBitrateStatus;
			}

			return row;
		}
	}
}