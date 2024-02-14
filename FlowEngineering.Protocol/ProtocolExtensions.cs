namespace Skyline.DataMiner.FlowEngineering.Protocol
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Scripting;

	public static class ProtocolExtensions
	{
		public static void SetColumns(this SLProtocol protocol, int tablePid, bool deleteOldRows, ICollection<string> keys, params (int Pid, IEnumerable<object> Data)[] columns)
		{
			if (deleteOldRows)
			{
				var keysToDelete = protocol.GetKeys(tablePid).Except(keys).ToArray();
				if (keysToDelete.Any())
				{
					protocol.DeleteRow(tablePid, keysToDelete);
				}
			}

			if (keys.Any())
			{
				object[] pids = new object[columns.Length + 1];
				object[] cols = new object[columns.Length + 1];

				pids[0] = tablePid;
				cols[0] = keys.Cast<object>().ToArray();

				for (int i = 0; i < columns.Length; i++)
				{
					var col = columns[i];
					var colData = col.Data.ToArray();

					pids[i + 1] = col.Pid;
					cols[i + 1] = colData;

					if (colData.Length != keys.Count)
					{
						throw new ArgumentException($"Column {col.Pid} has a wrong size.");
					}
				}

				protocol.NotifyProtocol(220, pids, cols);
			}
		}

		public static void SetOrAddRow(this SLProtocol protocol, int tablePid, QActionTableRow row)
		{
			if (protocol == null)
			{
				throw new ArgumentNullException(nameof(protocol));
			}

			if (row == null)
			{
				throw new ArgumentNullException(nameof(row));
			}

			if (protocol.Exists(tablePid, row.Key))
			{
				protocol.SetRow(tablePid, row.Key, row);
			}
			else
			{
				protocol.AddRow(tablePid, row);
			}
		}

	}
}
