namespace Skyline.DataMiner.FlowEngineering.Protocol
{
	using System;
	using System.Globalization;

	using Skyline.DataMiner.ConnectorAPI.FlowEngineering.Info;
	using Skyline.DataMiner.Scripting;

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

		public static void ReplyResult(this FlowInfoMessage flowInfoMessage, SLProtocol protocol, bool success, string message)
		{
			if (flowInfoMessage == null)
			{
				throw new ArgumentNullException(nameof(flowInfoMessage));
			}

			if (protocol == null)
			{
				throw new ArgumentNullException(nameof(protocol));
			}

			if (String.IsNullOrWhiteSpace(message) && !success)
			{
				throw new ArgumentException($"'{nameof(message)}' cannot be null or whitespace when '{nameof(success)}' is false.", nameof(message));
			}

			var response = new FlowInfoResponseMessage()
			{
				ProvisionedFlowId = flowInfoMessage.ProvisionedFlowId,
				IsSuccess = success,
				Message = message,
			};

			flowInfoMessage.Reply(
				protocol.SLNet.RawConnection,
				response,
				FlowInfoResponseMessage.Serializer);
		}

		public static void ReplySuccess(this FlowInfoMessage flowInfoMessage, SLProtocol protocol)
		{
			ReplyResult(flowInfoMessage, protocol, true, String.Empty);
		}

		public static void ReplyFailed(this FlowInfoMessage flowInfoMessage, SLProtocol protocol, string message)
		{
			ReplyResult(flowInfoMessage, protocol, false, message);
		}
	}
}
