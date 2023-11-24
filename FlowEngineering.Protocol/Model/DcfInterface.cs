namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
    public class DcfInterface
    {
        public DcfInterface(int id)
        {
            ID = id;
        }

        public int ID { get; }

        public string Name { get; internal set; }

        public string DynamicLink { get; internal set; }
    }
}