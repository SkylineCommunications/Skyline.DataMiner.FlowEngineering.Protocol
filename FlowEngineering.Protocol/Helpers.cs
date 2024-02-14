namespace Skyline.DataMiner.FlowEngineering.Protocol
{
	using Skyline.DataMiner.FlowEngineering.Protocol.Enums;

	public static class Helpers
	{
		public static ExpectedStatus CalculateExpectedStatus(int value, int expected)
		{
			if (expected.Equals(-1))
			{
				return ExpectedStatus.NA;
			}

			if (value < expected)
			{
				return ExpectedStatus.UnexpectedLow;
			}
			else if (value > expected)
			{
				return ExpectedStatus.UnexpectedHigh;
			}
			else
			{
				return ExpectedStatus.Normal;
			}
		}

		public static ExpectedStatus CalculateExpectedBitrateStatus(double bitrate, double expectedBitrate)
		{
			if (expectedBitrate.Equals(-1))
			{
				return ExpectedStatus.NA;
			}

			if (bitrate < expectedBitrate * 0.9)
			{
				return ExpectedStatus.UnexpectedLow;
			}
			else if (bitrate > expectedBitrate * 1.1)
			{
				return ExpectedStatus.UnexpectedHigh;
			}
			else
			{
				return ExpectedStatus.Normal;
			}
		}
	}
}
