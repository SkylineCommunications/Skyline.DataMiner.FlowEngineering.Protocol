namespace Skyline.DataMiner.FlowEngineering.Protocol.Model
{
	using System;
	using System.Collections.Generic;

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
		/// Gets or sets the extra saved data.
		/// </summary>
		public string ExtraData { get; set; }

		/// <summary>
		/// Gets or sets the additional data associated with this object.
		/// </summary>
		public object Tag { get; set; }

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
