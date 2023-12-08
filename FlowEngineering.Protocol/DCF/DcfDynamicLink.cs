namespace Skyline.DataMiner.FlowEngineering.Protocol.DCF
{
	using System;

	public class DcfDynamicLink
	{
		public DcfDynamicLink(int groupId, string pk)
		{
			GroupID = groupId;
			PK = pk;
		}

		public int GroupID { get; }

		public string PK { get; }

		public static bool TryParse(string input, out DcfDynamicLink dynamicLink)
		{
			var parts = input.Split(new[] { ';' }, 2, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length >= 2 &&
				Int32.TryParse(parts[0], out var groupId))
			{
				dynamicLink = new DcfDynamicLink(groupId, parts[1]);
				return true;
			}

			dynamicLink = null;
			return false;
		}
	}
}
