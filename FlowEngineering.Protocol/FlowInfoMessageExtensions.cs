namespace Skyline.DataMiner.FlowEngineering.Protocol
{
	using System;
	using System.Globalization;

	using Skyline.DataMiner.ConnectorAPI.FlowEngineering.Info;

	public static class FlowInfoMessageExtensions
	{
		public static bool TryGetBitrate(this FlowInfoMessage message, out double bitrate)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			if (message.Metadata != null &&
				message.Metadata.TryGetValue("Bitrate", out string strBitrate) &&
				Double.TryParse(strBitrate, NumberStyles.Any, CultureInfo.InvariantCulture, out bitrate))
			{
				return true;
			}

			bitrate = default;
			return false;
		}
	}
}
