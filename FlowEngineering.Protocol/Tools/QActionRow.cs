namespace Skyline.DataMiner.FlowEngineering.Protocol.Tools
{
	using System;
	using System.Collections.Generic;

	internal class QActionRow
	{
		private readonly List<object> _cells = new List<object>();

		public QActionRow(string key)
		{
			if (String.IsNullOrWhiteSpace(key))
			{
				throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));
			}

			Key = key;
		}

		public string Key { get; private set; }

		public object this[int column]
		{
			get
			{
				return _cells.Count > column ? _cells[column] : default;
			}

			set
			{
				while (column >= _cells.Count)
				{
					_cells.Add(default);
				}

				_cells[column] = value;
			}
		}

		public object[] ToObjectArray()
		{
			return _cells.ToArray();
		}

		public static implicit operator object[](QActionRow source)
		{
			return source.ToObjectArray();
		}
	}
}
