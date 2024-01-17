namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
    using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Core.DataMinerSystem.Protocol;
    using Skyline.DataMiner.FlowEngineering.Protocol;
    using Skyline.DataMiner.FlowEngineering.Protocol.DCF;
    using Skyline.DataMiner.FlowEngineering.Protocol.Enums;
    using Skyline.DataMiner.Scripting;

    public class Interfaces : ConcurrentDictionary<string, Interface>
	{
		private readonly FlowEngineeringManager _manager;

		public Interfaces(FlowEngineeringManager manager)
		{
			_manager = manager;
		}

		public void Add(Interface intf)
		{
			if (intf == null)
			{
				throw new ArgumentNullException(nameof(intf));
			}

			TryAdd(intf.Index, intf);
		}

		public Interface GetOrAdd(string index)
		{
			if (String.IsNullOrWhiteSpace(index))
			{
				throw new ArgumentException($"'{nameof(index)}' cannot be null or whitespace.", nameof(index));
			}

			if (!TryGetValue(index, out var intf))
			{
				intf = new Interface(index);
				Add(intf);
			}

			return intf;
		}

		public void AddRange(IEnumerable<Interface> interfaces)
		{
			if (interfaces == null)
			{
				throw new ArgumentNullException(nameof(interfaces));
			}

			foreach (var itf in interfaces)
			{
				Add(itf);
			}
		}

		public bool Remove(Interface intf)
		{
			if (intf == null)
			{
				throw new ArgumentNullException(nameof(intf));
			}

			return TryRemove(intf.Index, out _);
		}

		public void ReplaceInterfaces(IEnumerable<Interface> newInterfaces)
		{
			if (newInterfaces == null)
			{
				throw new ArgumentNullException(nameof(newInterfaces));
			}

			Clear();
			AddRange(newInterfaces);
		}

		public bool TryGetByDescription(string description, out Interface intf)
		{
			if (description == null)
			{
				throw new ArgumentNullException(nameof(description));
			}

			intf = Values.FirstOrDefault(x => String.Equals(x.Description, description));
			return intf != null;
		}

		public bool TryGetByDcfInterfaceID(int dcfInterfaceId, out Interface intf)
		{
			intf = Values.FirstOrDefault(x => Equals(x.DcfInterfaceId, dcfInterfaceId));
			return intf != null;
		}

		public bool TryGetByDcfDynamicLink(string dcfDynamicLink, out Interface intf)
		{
			intf = Values.FirstOrDefault(x => String.Equals(x.DcfDynamicLink, dcfDynamicLink));
			return intf != null;
		}

		public void LoadTable(SLProtocol protocol)
		{
			var table = protocol.GetLocalElement()
				.GetTable(FleParameters.Fleinterfacesoverviewtable.tablePid)
				.GetColumns(
					new uint[]
					{
						FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableindex,
						FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtabledescription,
						FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtabletype,
						FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableadminstatus,
						FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtableoperstatus,
						FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtabledisplaykey,
						FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtablerxbitrate,
						FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtablerxflows,
						FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtabletxbitrate,
						FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtabletxflows,
						FleParameters.Fleinterfacesoverviewtable.Idx.fleinterfacesoverviewtabledcfinterfaceid,
					},
					(string idx, string desc, int type, int adminStatus, int operStatus, string displayKey, double rxBitrate, int rxFlows, double txBitrate, int txFlows, int dcfIntfId) =>
					{
						return new
						{
							Idx = idx,
							Description = desc,
							Type = (InterfaceType)type,
							AdminStatus = (InterfaceAdminStatus)adminStatus,
							OperationalStatus = (InterfaceOperationalStatus)operStatus,
							DisplayKey = displayKey,
							RxBitrate = rxBitrate,
							RxFlows = rxFlows,
							TxBitrate = txBitrate,
							TxFlows = txFlows,
							DcfInterfaceId = dcfIntfId,
						};
					});

			foreach (var row in table)
			{
				if (!TryGetValue(row.Idx, out var intf))
				{
					intf = new Interface(row.Idx);
					Add(intf);
				}

				intf.Description = row.Description;
				intf.Type = row.Type;
				intf.AdminStatus = row.AdminStatus;
				intf.OperationalStatus = row.OperationalStatus;
				intf.DisplayKey = row.DisplayKey;
				intf.RxBitrate = row.RxBitrate;
				intf.RxFlows = row.RxFlows;
				intf.TxBitrate = row.TxBitrate;
				intf.TxFlows = row.TxFlows;
				intf.DcfInterfaceId = row.DcfInterfaceId;
			}
		}

		public void LoadDcfDynamicLinks(SLProtocol protocol)
		{
			var dcfHelper = DcfInterfaceHelper.Create(protocol);

			foreach (var intf in this.Values)
			{
				if (dcfHelper.Interfaces.TryGetValue(intf.DcfInterfaceId, out var dcfIntf))
				{
					intf.DcfDynamicLink = dcfIntf.DynamicLink;
				}
			}
		}

		public void UpdateTable(SLProtocol protocol, bool includeStatistics = true)
		{
			var columns = new List<(int Pid, IEnumerable<object> Data)>
			{
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtabledescription, Values.Select(x => x.Description)),
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtabledisplaykey, Values.Select(x => x.DisplayKey)),
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtabletype, Values.Select(x => (object)x.Type)),
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtableadminstatus, Values.Select(x => (object)x.AdminStatus)),
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtableoperstatus, Values.Select(x => (object)x.OperationalStatus)),
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtabledcfinterfaceid, Values.Select(x => (object)x.DcfInterfaceId)),
			};

			if (includeStatistics)
			{
				columns.AddRange(GetStatisticsColumns());
			}

			protocol.SetColumns(
				FleParameters.Fleinterfacesoverviewtable.tablePid,
				deleteOldRows: true,
				Values.Select(x => x.Index).ToArray(),
				columns.ToArray());
		}

		public void UpdateStatistics(SLProtocol protocol)
		{
			protocol.SetColumns(
				FleParameters.Fleinterfacesoverviewtable.tablePid,
				deleteOldRows: true,
				Values.Select(x => x.Index).ToArray(),
				GetStatisticsColumns());
		}

		private void CalculateExpectedFlowsAndBitrates()
		{
			var lookupIncomingFlows = _manager.IncomingFlows.Values.ToLookup(x => x.Interface);
			var lookupOutgoingFlows = _manager.OutgoingFlows.Values.ToLookup(x => x.Interface);

			foreach (var intf in Values)
			{
				var incomingFlows = lookupIncomingFlows[intf.Index];
				var outgoingFlows = lookupOutgoingFlows[intf.Index];

				intf.RxFlows = incomingFlows.Count(x => x.IsPresent);
				intf.RxExpectedFlows = incomingFlows.Count();
				intf.RxExpectedBitrate = incomingFlows.Where(x => x.ExpectedBitrate >= 0).Sum(x => x.ExpectedBitrate);

				intf.TxFlows = outgoingFlows.Count(x => x.IsPresent);
				intf.TxExpectedFlows = outgoingFlows.Count();
				intf.TxExpectedBitrate = outgoingFlows.Where(x => x.ExpectedBitrate >= 0).Sum(x => x.ExpectedBitrate);
			}
		}

		private (int Pid, IEnumerable<object> Data)[] GetStatisticsColumns()
		{
			CalculateExpectedFlowsAndBitrates();

			return new[]
			{
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtablerxbitrate, Values.Select(x => (object)x.RxBitrate)),
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtablerxflows, Values.Select(x => (object)x.RxFlows)),
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtablerxutilization, Values.Select(x => (object)x.RxUtilization)),
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtableexpectedrxbitrate, Values.Select(x => (object)x.RxExpectedBitrate)),
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtableexpectedrxbitratestatus, Values.Select(x => (object)x.RxExpectedBitrateStatus)),
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtableexpectedrxflows, Values.Select(x => (object)x.RxExpectedFlows)),
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtableexpectedrxflowsstatus, Values.Select(x => (object)x.RxExpectedFlowsStatus)),
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtabletxbitrate, Values.Select(x => (object)x.TxBitrate)),
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtabletxflows, Values.Select(x => (object)x.TxFlows)),
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtabletxutilization, Values.Select(x => (object)x.TxUtilization)),
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtableexpectedtxbitrate, Values.Select(x => (object)x.TxExpectedBitrate)),
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtableexpectedtxbitratestatus, Values.Select(x => (object)x.TxExpectedBitrateStatus)),
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtableexpectedtxflows, Values.Select(x => (object)x.TxExpectedFlows)),
				(FleParameters.Fleinterfacesoverviewtable.Pid.fleinterfacesoverviewtableexpectedtxflowsstatus, Values.Select(x => (object)x.TxExpectedFlowsStatus)),
			};
		}
	}
}