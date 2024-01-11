namespace Skyline.DataMiner.FlowEngineering.Protocol.DCF
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Core.DataMinerSystem.Protocol;
	using Skyline.DataMiner.FlowEngineering.Protocol.Model;
	using Skyline.DataMiner.Scripting;

	public class DcfInterfaceHelper
	{
		private DcfInterfaceHelper()
		{
		}

		internal Dictionary<int, DcfInterface> Interfaces { get; } = new Dictionary<int, DcfInterface>();

		public static DcfInterfaceHelper Create(SLProtocol protocol)
		{
			var intfs = protocol.GetLocalElement()
				.GetTable(65049)
				.GetColumns(
					new uint[] { 0, 1, 5, },
					(string idx, string name, string dynamicLink) =>
					{
						var id = Convert.ToInt32(idx);

						var intf = new DcfInterface(id)
						{
							Name = name,
							DynamicLink = dynamicLink,
						};

						return intf;
					});

			var helper = new DcfInterfaceHelper();

			foreach (var intf in intfs)
			{
				helper.Interfaces.Add(intf.ID, intf);
			}

			return helper;
		}

		public bool TryFindInterface(int groupId, out DcfInterface dcfInterface)
		{
			return Interfaces.TryGetValue(groupId, out dcfInterface);
		}

		public bool TryFindInterface(int groupId, string dynamicPk, out DcfInterface dcfInterface)
		{
			foreach (var intf in Interfaces.Values)
			{
				if (DcfDynamicLink.TryParse(intf.DynamicLink, out var dynamicLink)
					&& dynamicLink.GroupID == groupId
					&& String.Equals(dynamicLink.PK, dynamicPk))
				{
					dcfInterface = intf;
					return true;
				}
			}

			dcfInterface = null;
			return false;
		}
	}
}
