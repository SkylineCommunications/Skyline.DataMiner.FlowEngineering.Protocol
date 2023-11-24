namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
    public enum ExpectedStatus
    {
        NA = -1,
        Normal = 1,
        UnexpectedLow = 2,
        UnexpectedHigh = 3,
    }

    public enum InterfaceAdminStatus
    {
        Up = 1,
        Down = 2,
        Testing = 3,
    }

    public enum InterfaceOperationalStatus
    {
        Up = 1,
        Down = 2,
        Testing = 3,
        Unknown = 4,
        Dormant = 5,
        NotPresent = 6,
        LowerLayerDown = 7,
        AdminDown = 10,
    }
}