namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Collections.Generic;

	using Newtonsoft.Json;

	using Skyline.DataMiner.ConnectorAPI.FlowEngineering.Info;
	using Skyline.DataMiner.FlowEngineering.Protocol.Tools;
	using Skyline.DataMiner.Scripting;

	public class ProvisionedFlow : IEquatable<ProvisionedFlow>
	{
		public ProvisionedFlow(Guid id)
		{
			ID = id;
		}

		public ProvisionedFlow(string id)
		{
			ID = Guid.Parse(id);
		}

		/// <summary>
		/// Gets or sets the DOM instance ID of the provisioned flow.
		/// </summary>
		public Guid ID { get; }

		/// <summary>
		/// Gets or sets the DOM instance ID of the source.
		/// </summary>
		public Guid SourceID { get; set; }

		/// <summary>
		/// Gets or sets the DOM instance ID of the destination.
		/// </summary>
		public Guid DestinationID { get; set; }

		/// <summary>
		/// Gets or sets the ID of the incoming DCF interface.
		/// </summary>
		public int IncomingDcfInterfaceID { get; set; }

		/// <summary>
		/// Gets or sets "ParameterGroupID;PrimaryKey" of the incoming DCF interface.
		/// </summary>
		public string IncomingDcfDynamicLink { get; set; }

		/// <summary>
		/// Gets or sets the ID of the outgoing DCF interface.
		/// </summary>
		public int OutgoingDcfInterfaceID { get; set; }

		/// <summary>
		/// Gets or sets "ParameterGroupID;PrimaryKey" of the outgoing DCF interface.
		/// </summary>
		public string OutgoingDcfDynamicLink { get; set; }

		/// <summary>
		/// Gets or sets an optional identifier of the source that can be used in addition to the DOM instance ID.
		/// </summary>
		public string OptionalSourceIdentifier { get; set; }

		/// <summary>
		/// Gets or sets an optional identifier of the destination that can be used in addition to the DOM instance ID.
		/// </summary>
		public string OptionalDestinationIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the IP address of the media stream's source.
		/// </summary>
		public string SourceIP { get; set; }

		/// <summary>
		/// Gets or sets the IP address of the media stream's destination.
		/// </summary>
		public string DestinationIP { get; set; }

		/// <summary>
		/// Gets or sets the port of the media stream's destination.
		/// </summary>
		public int DestinationPort { get; set; }

		/// <summary>
		/// Gets or sets the metadata.
		/// </summary>
		public string Metadata { get; set; }

		/// <summary>
		/// Gets or sets the additional data associated with this object.
		/// </summary>
		public object Tag { get; set; }

		public void AddToTable(SLProtocol protocol)
		{
			var row = BuildRow();

			if (protocol.Exists(Parameter.Fleprovisionedflowstable.tablePid, row.Key))
			{
				protocol.SetRow(Parameter.Fleprovisionedflowstable.tablePid, row.Key, (object[])row);
			}
			else
			{
				protocol.AddRow(Parameter.Fleprovisionedflowstable.tablePid, (object[])row);
			}
		}

		public void RemoveFromTable(SLProtocol protocol)
		{
			var key = Convert.ToString(ID);
			protocol.DeleteRow(Parameter.Fleprovisionedflowstable.tablePid, key);
		}

		public static ProvisionedFlow FromFlowInfoMessage(FlowInfoMessage message)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			return new ProvisionedFlow(message.ProvisionedFlowId)
			{
				SourceID = message.SourceId,
				DestinationID = message.DestinationId,
				IncomingDcfInterfaceID = message.IncomingDcfInterfaceID,
				IncomingDcfDynamicLink = message.IncomingDcfDynamicLink,
				OutgoingDcfInterfaceID = message.OutgoingDcfInterfaceID,
				OutgoingDcfDynamicLink = message.OutgoingDcfDynamicLink,
				OptionalSourceIdentifier = message.OptionalSourceIdentifier,
				OptionalDestinationIdentifier = message.OptionalDestinationIdentifier,
				DestinationIP = message.IpConfiguration?.DestinationIP ?? String.Empty,
				DestinationPort = message.IpConfiguration?.DestinationPort ?? -1,
				SourceIP = message.IpConfiguration?.SourceIP ?? String.Empty,
				Metadata = JsonConvert.SerializeObject(message.Metadata),
			};
		}

		internal QActionRow BuildRow()
		{
			var key = Convert.ToString(ID);

			var row = new QActionRow(key);

			row[Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstableid] = key;
			row[Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstablesourceid] = Convert.ToString(SourceID);
			row[Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstabledestinationid] = Convert.ToString(DestinationID);
			row[Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstableincomingdcfinterfaceid] = IncomingDcfInterfaceID;
			row[Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstableincomingdcfinterfacedynamiclink] = IncomingDcfDynamicLink;
			row[Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstableoutgoingdcfinterfaceid] = OutgoingDcfInterfaceID;
			row[Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstableoutgoingdcfinterfacedynamiclink] = OutgoingDcfDynamicLink;
			row[Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstableoptionalsourceidentifier] = OptionalSourceIdentifier;
			row[Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstableoptionaldestinationidentifier] = OptionalDestinationIdentifier;
			row[Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstabledestinationip] = DestinationIP;
			row[Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstabledestinationport] = DestinationPort;
			row[Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstablesourceip] = SourceIP;
			row[Parameter.Fleprovisionedflowstable.Idx.fleprovisionedflowstablemetadata] = Metadata;

			return row;
		}

		#region IEquatable

		public override bool Equals(object obj)
		{
			return Equals(obj as ProvisionedFlow);
		}

		public bool Equals(ProvisionedFlow other)
		{
			return other != null &&
				   ID == other.ID;
		}

		public override int GetHashCode()
		{
			return ID.GetHashCode();
		}

		public static bool operator ==(ProvisionedFlow left, ProvisionedFlow right)
		{
			return EqualityComparer<ProvisionedFlow>.Default.Equals(left, right);
		}

		public static bool operator !=(ProvisionedFlow left, ProvisionedFlow right)
		{
			return !(left == right);
		}

		#endregion
	}
}
