using Opc.Ua.Server;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace OPCUA_Server
{
    public class MyNodeManagerFactory : INodeManagerFactory
    {
        public StringCollection NamespacesUris => new StringCollection()
        {
            "http://opcfoundation.org/UA/",
            "http://opcfoundation.org/UA/2"
        };

        public INodeManager Create(IServerInternal server, ApplicationConfiguration configuration)
        {
            return new MyNodeManager(server, configuration, NamespacesUris);
        }
    }
}
