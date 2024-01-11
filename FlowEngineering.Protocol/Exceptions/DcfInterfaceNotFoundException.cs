namespace Skyline.DataMiner.FlowEngineering.Protocol.Exceptions
{
	using System;

	public class DcfInterfaceNotFoundException : Exception
	{
		public DcfInterfaceNotFoundException()
		{
		}

		public DcfInterfaceNotFoundException(string message) : base(message)
		{
		}
	}
}