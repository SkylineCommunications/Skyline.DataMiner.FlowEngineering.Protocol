namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.FlowEngineering.Protocol;
	using Skyline.DataMiner.FlowEngineering.Protocol.Enums;
	using Skyline.DataMiner.Scripting;

	public class Interface : IEquatable<Interface>
	{
		public Interface(string index)
		{
			if (String.IsNullOrWhiteSpace(index))
			{
				throw new ArgumentException($"'{nameof(index)}' cannot be null or whitespace.", nameof(index));
			}

			Index = index;
		}

		public string Index { get; }

		/// <summary>
		/// Gets the instance of this interface. This is an alias for <see cref="Index" />.
		/// </summary>
		public string Instance => Index;

		public string Description { get; set; }

		public string DisplayKey { get; set; }

		public InterfaceType Type { get; set; }

		public InterfaceAdminStatus AdminStatus { get; set; }

		public InterfaceOperationalStatus OperationalStatus { get; set; }

		public int DcfInterfaceId { get; set; }

		public string DcfDynamicLink { get; internal set; }

		public double RxBitrate { get; set; }

		public double RxUtilization { get; set; }

		public double RxExpectedBitrate { get; internal set; }

		public ExpectedStatus RxExpectedBitrateStatus => Helpers.CalculateExpectedBitrateStatus(RxBitrate, RxExpectedBitrate);

		public int RxFlows { get; set; }

		public int RxExpectedFlows { get; internal set; }

		public ExpectedStatus RxExpectedFlowsStatus => Helpers.CalculateExpectedStatus(RxFlows, RxExpectedFlows);

		public double TxBitrate { get; set; }

		public double TxUtilization { get; set; }

		public double TxExpectedBitrate { get; internal set; }

		public ExpectedStatus TxExpectedBitrateStatus => Helpers.CalculateExpectedBitrateStatus(TxBitrate, TxExpectedBitrate);

		public int TxFlows { get; set; }

		public int TxExpectedFlows { get; internal set; }

		public ExpectedStatus TxExpectedFlowsStatus => Helpers.CalculateExpectedStatus(TxFlows, TxExpectedFlows);

		/// <summary>
		/// Gets or sets the additional data associated with this object.
		/// </summary>
		public object Tag { get; set; }

		public void SetStatus(SLProtocol protocol, InterfaceAdminStatus adminStatus, InterfaceOperationalStatus operStatus)
		{
			AdminStatus = adminStatus;
			OperationalStatus = operStatus;

			var row = new object[5];
			row[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableindex] = Index;
			row[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableadminstatus] = (int)AdminStatus;
			row[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableoperstatus] = (int)OperationalStatus;

			protocol.SetRow(FleParameters.Fleinterfacesoverviewtable.tablePid, Index, row);
		}

		public void SetOrAddRow(SLProtocol protocol, bool includeStatistics = true)
		{
			var row = BuildRow(includeStatistics);
			protocol.SetOrAddRow(FleParameters.Fleinterfacesoverviewtable.tablePid, row);
		}

		public void RemoveRow(SLProtocol protocol)
		{
			var key = Convert.ToString(Instance);
			protocol.DeleteRow(FleParameters.Fleinterfacesoverviewtable.tablePid, key);
		}

		public QActionTableRow BuildRow(bool includeStatistics = true)
		{
			var row = new QActionTableRow(0, 21);

			row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableindex] = Instance;
			row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtabledescription] = Description;
			row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtabletype] = (int)Type;
			row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableadminstatus] = (int)AdminStatus;
			row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableoperstatus] = (int)OperationalStatus;
			row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtabledisplaykey] = DisplayKey;
			row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtabledcfinterfaceid] = DcfInterfaceId;

			if (includeStatistics)
			{
				row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtablerxbitrate] = RxBitrate;
				row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtablerxflows] = RxFlows;
				row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtabletxbitrate] = TxBitrate;
				row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtabletxflows] = TxFlows;
				row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtablerxutilization] = RxUtilization;
				row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtabletxutilization] = TxUtilization;
				row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableexpectedrxbitrate] = RxExpectedBitrate;
				row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableexpectedrxbitratestatus] = (int)RxExpectedBitrateStatus;
				row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableexpectedrxflows] = RxExpectedFlows;
				row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableexpectedrxflowsstatus] = (int)RxExpectedFlowsStatus;
				row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableexpectedtxbitrate] = TxExpectedBitrate;
				row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableexpectedtxbitratestatus] = (int)TxExpectedBitrateStatus;
				row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableexpectedtxflows] = TxExpectedFlows;
				row.Columns[FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableexpectedtxflowsstatus] = (int)TxExpectedFlowsStatus;
			}

			return row;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Interface);
		}

		public bool Equals(Interface other)
		{
			return other != null &&
				   Index == other.Index;
		}

		public override int GetHashCode()
		{
			return Index.GetHashCode();
		}

		public static bool operator ==(Interface left, Interface right)
		{
			return EqualityComparer<Interface>.Default.Equals(left, right);
		}

		public static bool operator !=(Interface left, Interface right)
		{
			return !(left == right);
		}
	}
}