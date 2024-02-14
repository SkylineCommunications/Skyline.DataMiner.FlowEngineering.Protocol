namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.FlowEngineering.Protocol;
	using Skyline.DataMiner.FlowEngineering.Protocol.Enums;
	using Skyline.DataMiner.Scripting;

	public abstract class Flow : IEquatable<Flow>
	{
		private string _linkedFlow;

		protected Flow(string instance)
		{
			if (String.IsNullOrWhiteSpace(instance))
			{
				throw new ArgumentException($"'{nameof(instance)}' cannot be null or whitespace.", nameof(instance));
			}

			Instance = instance;
		}

		public string Instance { get; }

		public string DestinationIP { get; set; }

		public int DestinationPort { get; set; }

		public string SourceIP { get; set; }

		public string Interface { get; set; }

		public FlowTransportType TransportType { get; set; }

		public double Bitrate { get; set; } = -1;

		public double ExpectedBitrate { get; set; } = -1;

		public ExpectedStatus ExpectedBitrateStatus => Helpers.CalculateExpectedBitrateStatus(Bitrate, ExpectedBitrate);

		public string Label { get; set; }

		public string LinkedFlow
		{
			get => _linkedFlow;
			set
			{
				if (!String.IsNullOrEmpty(value) && !Guid.TryParse(value, out _))
				{
					throw new ArgumentException($"{nameof(value)} should be either a valid GUID or an empty string.");
				}

				_linkedFlow = value;
			}
		}

		public FlowOwner FlowOwner => !String.IsNullOrEmpty(LinkedFlow) ? FlowOwner.FlowEngineering : FlowOwner.LocalSystem;

		public bool IsPresent { get; set; }

		/// <summary>
		/// Gets or sets the additional data associated with this object.
		/// </summary>
		public object Tag { get; set; }

		public abstract void SetOrAddRow(SLProtocol protocol, bool includeStatistics = true);

		public abstract void RemoveRow(SLProtocol protocol);

		public abstract QActionTableRow BuildRow(bool includeStatistics = true);

		#region IEquatable

		public override bool Equals(object obj)
		{
			return Equals(obj as Flow);
		}

		public bool Equals(Flow other)
		{
			return other != null &&
				   Instance == other.Instance;
		}

		public override int GetHashCode()
		{
			return Instance.GetHashCode();
		}

		public static bool operator ==(Flow left, Flow right)
		{
			return EqualityComparer<Flow>.Default.Equals(left, right);
		}

		public static bool operator !=(Flow left, Flow right)
		{
			return !(left == right);
		}

		#endregion
	}
}