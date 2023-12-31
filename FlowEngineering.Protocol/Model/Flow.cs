﻿namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.FlowEngineering.Protocol;

	public class Flow : IEquatable<Flow>
    {
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

        public ExpectedStatus ExpectedBitrateStatus => Tools.CalculateExpectedBitrateStatus(Bitrate, ExpectedBitrate);

        public string Label { get; set; }

        public string LinkedFlow { get; set; }

        public FlowOwner FlowOwner { get; set; }

        public bool IsPresent { get; set; }

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
    }
}