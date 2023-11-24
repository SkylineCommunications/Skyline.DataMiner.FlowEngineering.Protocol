namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
    using System;

    public class TxFlow : Flow
    {
        public TxFlow(string instance) : base(instance)
        {
        }

        public string OutgoingInterface
        {
            get { return Interface; }
            set { Interface = value; }
        }

        public string ForeignKeyIncoming { get; set; } = String.Empty;
    }
}