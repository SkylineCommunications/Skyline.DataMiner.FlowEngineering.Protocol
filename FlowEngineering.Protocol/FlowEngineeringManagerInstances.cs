namespace Skyline.DataMiner.FlowEngineering.Protocol
{
	using System;
	using System.Collections.Concurrent;

	using Skyline.DataMiner.Scripting;

	public class FlowEngineeringManagerInstances
    {
        private static readonly ConcurrentDictionary<string, FlowEngineeringManager> Instances = new ConcurrentDictionary<string, FlowEngineeringManager>();

        public static FlowEngineeringManager GetInstance(SLProtocol protocol)
        {
            string key = GetKey(protocol);

            return Instances.GetOrAdd(key, k =>
            {
                var manager = new FlowEngineeringManager();
                manager.LoadTables(protocol);
                return manager;
            });
        }

        /// <summary>
        /// Removes existing data, and returns a new instance.
        /// </summary>
        public static FlowEngineeringManager CreateNewInstance(SLProtocol protocol)
        {
            string key = GetKey(protocol);
            Instances.TryRemove(key, out _);
            return GetInstance(protocol);
        }

        private static string GetKey(SLProtocol protocol)
        {
            if (protocol == null)
                throw new ArgumentNullException(nameof(protocol));

            if (protocol.DataMinerID < 0 || protocol.ElementID < 0)
                throw new ArgumentException(nameof(protocol));

            string key = String.Join("/", protocol.DataMinerID, protocol.ElementID);

            return key;
        }
    }
}
