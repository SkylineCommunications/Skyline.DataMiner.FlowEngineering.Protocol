namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
    using System;

    public class RxFlow : Flow
    {
        public RxFlow(string instance) : base(instance)
        {
        }

        public string IncomingInterface
        {
            get { return Interface; }
            set { Interface = value; }
        }

        public string ForeignKeyOutgoing { get; set; } = String.Empty;
    }
}