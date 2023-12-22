using Microsoft.AspNetCore.Hosting.Server;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Server;
using Org.BouncyCastle.Asn1.Anssi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace OPCUA_Server
{
    internal class MyNodeManager : CustomNodeManager2
    {
        private ushort[] nameSpaceIndexes = new ushort[2];
        private Hardware _hardware;
        private BaseDataVariableState _isBusyVariable;
        public MyNodeManager(IServerInternal server, ApplicationConfiguration configuration, StringCollection namespaceIndex)
       : base(server, configuration)
        {
            // Initialize your namespace, it's important for node identification
            SystemContext.NodeIdFactory = this;

            nameSpaceIndexes[0] = GetNamespaceIndex(server, namespaceIndex[0]);
            nameSpaceIndexes[1] = GetNamespaceIndex(server, namespaceIndex[1]);
            base.SetNamespaces(namespaceIndex.ToArray());
            base.SetNamespaceIndexes(nameSpaceIndexes);
        }

        public ushort GetNamespaceIndex(IServerInternal server, string namespaceUri)
        {
            // Check if the namespace URI is already in the server's namespace array
            var index = server.NamespaceUris.GetIndexOrAppend(namespaceUri);

            // If not found, add it to the array
            if (index == -1)
            {
                index = (ushort)server.NamespaceUris.Append(namespaceUri);
            }

            // Return the index as ushort
            return (ushort)index;
        }

        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            base.CreateAddressSpace(externalReferences);

            var root = FindNodeInAddressSpace(84);


            // Create a folder
            FolderState folder = new FolderState(root)
            {
                SymbolicName = "VIPS",
                NodeId = new NodeId("VIPS", NamespaceIndexes[1]),
                BrowseName = new QualifiedName("VIPS", NamespaceIndexes[1]),
                DisplayName = new LocalizedText("VIPS"),
                TypeDefinitionId = Opc.Ua.ObjectTypeIds.FolderType,
                WriteMask = AttributeWriteMask.None,
                UserWriteMask = AttributeWriteMask.None,
                EventNotifier = EventNotifiers.None
            };

            folder.AccessRestrictions = AccessRestrictionType.None;
            var test = folder.GetDisplayPath();
            root.AddChild(folder);

            // Add the folder to the address space
            AddPredefinedNode(SystemContext, folder);
            _hardware = new Hardware();
            _hardware.MyEvent += _hardware_MyEvent;
            // Create a variable
            _isBusyVariable = new BaseDataVariableState(folder)
            {
                SymbolicName = "IsBusy",
                NodeId = new NodeId("IsBusy", NamespaceIndexes[1]),
                BrowseName = new QualifiedName("IsBusy", NamespaceIndexes[1]),
                DisplayName = new LocalizedText("IsBusy"),
                Description = "IsBusy",
                TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
                DataType = DataTypeIds.Boolean,
                ValueRank = ValueRanks.Scalar,
                AccessLevel = AccessLevels.CurrentReadOrWrite,
                UserAccessLevel = AccessLevels.CurrentReadOrWrite,
                Historizing = false,
                StatusCode = StatusCodes.Good,
                Timestamp = DateTime.UtcNow,
                Value = _hardware.IsBusy,
            };

            _isBusyVariable.SetAreEventsMonitored(SystemContext, true, false);

            // Add the variable to the folder
            folder.AddChild(_isBusyVariable);

            // Add the variable to the address space
            AddPredefinedNode(SystemContext, _isBusyVariable);
            var test3 = Server.EndpointAddresses;
            var test4 = Server.NamespaceUris;
            var test2 = FindNodeInAddressSpace(_isBusyVariable.NodeId);
            bool test1 = IsNodeIdInNamespace(_isBusyVariable.NodeId);
        }

        private void _hardware_MyEvent(object sender, EventArgs e)
        {
            // Update the value of the OPC UA variable
            // The actual update logic will depend on your application requirements
            lock (_isBusyVariable)
            {
                _isBusyVariable.Value = _hardware.IsBusy; // Assuming GetNewValue() fetches or computes the new value
                _isBusyVariable.Timestamp = DateTime.UtcNow;
                _isBusyVariable.StatusCode = StatusCodes.Good;
                var monitoredItems = MonitoredNodes;
                foreach (KeyValuePair<NodeId, MonitoredNode2> kvp in monitoredItems)
                {
                    if (kvp.Key.Equals(_isBusyVariable.NodeId))
                    {
                        kvp.Value.OnMonitoredNodeChanged(SystemContext, _isBusyVariable, NodeStateChangeMasks.Value);
                    }
                }


            }
            // Notify any OPC UA clients subscribed to this variable
            // This typically involves calling a method to trigger the notification
        }

        public override void Write(
           OperationContext context,
           IList<WriteValue> nodesToWrite,
           IList<ServiceResult> errors)
        {

            foreach(var node in nodesToWrite)
            {
                if (node.NodeId.Equals(_isBusyVariable.NodeId))
                {
                    _hardware.IsBusy = (bool)node.Value.Value;
                }
            }

        }
    }
}
