﻿namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.FlowEngineering.Protocol;
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

		public string Description { get; set; }

		public string DisplayKey { get; set; }

		public InterfaceType Type { get; set; }

		public InterfaceAdminStatus AdminStatus { get; set; }

		public InterfaceOperationalStatus OperationalStatus { get; set; }

		public int DcfInterfaceId { get; set; }

		public double RxBitrate { get; set; }

		public double RxUtilization { get; set; }

		public double RxExpectedBitrate { get; internal set; }

		public ExpectedStatus RxExpectedBitrateStatus => Tools.CalculateExpectedBitrateStatus(RxBitrate, RxExpectedBitrate);

		public int RxFlows { get; set; }

		public int RxExpectedFlows { get; internal set; }

		public ExpectedStatus RxExpectedFlowsStatus => Tools.CalculateExpectedStatus(RxFlows, RxExpectedFlows);

		public double TxBitrate { get; set; }

		public double TxUtilization { get; set; }

		public double TxExpectedBitrate { get; internal set; }

		public ExpectedStatus TxExpectedBitrateStatus => Tools.CalculateExpectedBitrateStatus(TxBitrate, TxExpectedBitrate);

		public int TxFlows { get; set; }

		public int TxExpectedFlows { get; internal set; }

		public ExpectedStatus TxExpectedFlowsStatus => Tools.CalculateExpectedStatus(TxFlows, TxExpectedFlows);

		public void SetStatus(SLProtocol protocol, InterfaceAdminStatus adminStatus, InterfaceOperationalStatus operStatus)
		{
			AdminStatus = adminStatus;
			OperationalStatus = operStatus;

			var row = new object[5];
			row[Parameter.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableindex] = Index;
			row[Parameter.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableadminstatus] = (int)AdminStatus;
			row[Parameter.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableoperstatus] = (int)OperationalStatus;

			protocol.SetRow(Parameter.Fleinterfacesoverviewtable.tablePid, Index, row);
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