using Opc.Ua.Configuration;
using Opc.Ua;
using Opc.Ua.Server;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting.Server;

namespace OPCUA_Server
{
    internal class MyOPCUAServer : StandardServer
    {
        private ApplicationConfiguration _config;
     
        protected override void OnServerStarted(IServerInternal server)
        {
            base.OnServerStarted(server);
        }

 


        public void Start()
        {



            ApplicationInstance application = new ApplicationInstance
            {
                ApplicationType = ApplicationType.Server,
                ConfigSectionName = "MyOPCUAServer"
            };

            // Create the application configuration
            _config = new ApplicationConfiguration
            {
                ApplicationName = "My OPC UA Server",
                ApplicationUri = Utils.Format(@"urn:{0}:MyOPCUAServer", System.Net.Dns.GetHostName()),
                ApplicationType = ApplicationType.Server,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier
                    {
                        StoreType = "Directory",
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault",
                        SubjectName = "My OPC UA Server"
                    },
                    TrustedPeerCertificates = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Applications",
                    },
                    TrustedIssuerCertificates = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities",
                    },
                    RejectedCertificateStore = new CertificateStoreIdentifier
                    {
                        StoreType = "Directory",
                        StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates",
                    },
                    AutoAcceptUntrustedCertificates = true,
                    RejectSHA1SignedCertificates = false,
                    MinimumCertificateKeySize = 1024
                },
                ServerConfiguration = new ServerConfiguration
                {
                    BaseAddresses = { "opc.tcp://localhost:4840/" },
                    SecurityPolicies = new ServerSecurityPolicyCollection {
                new ServerSecurityPolicy {
                    SecurityMode = MessageSecurityMode.None,
                    SecurityPolicyUri = SecurityPolicies.None
                }
            },
                    UserTokenPolicies = new UserTokenPolicyCollection {
                new UserTokenPolicy(UserTokenType.Anonymous)
            },
                    MaxSessionCount = 10,
                    MaxBrowseContinuationPoints = 10,
                    MaxQueryContinuationPoints = 10,
                    MaxHistoryContinuationPoints = 10,
                    MaxSubscriptionCount = 10
                },
                TransportQuotas = new TransportQuotas
                {
                    OperationTimeout = 600000,
                    MaxStringLength = 65535,
                    MaxByteStringLength = 65535,
                    MaxArrayLength = 65535,
                    MaxMessageSize = 65535,
                    MaxBufferSize = 65535,
                    ChannelLifetime = 300000,
                    SecurityTokenLifetime = 3600000
                },
                TraceConfiguration = new TraceConfiguration()
            };

            _config.SecurityConfiguration.ApplicationCertificate = new CertificateIdentifier
            {
                StoreType = "Directory",
                StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault",
                SubjectName = _config.ApplicationName
            };

            // Assign the certificate to the application
            application.ApplicationConfiguration = _config;


            // Validate the application configuration
            _config.Validate(ApplicationType.Server).GetAwaiter().GetResult();


            var applicationInstance = new ApplicationInstance(_config);

            // This call will check that the application instance certificate exists, and will create it if not
            var result =
                applicationInstance.CheckApplicationInstanceCertificate(false, CertificateFactory.DefaultKeySize).GetAwaiter().GetResult();;


            // If auto-accepting untrusted certificates, add a dummy certificate to the trusted peers list
            if (_config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            {
                // Create a self-signed certificate (for example purposes only)
                var certificate = CertificateFactory.CreateCertificate(
                    _config.ApplicationUri,
                    "My OPC UA Server",
                    "OPC UA Test",
                    null).CreateForRSA();

                // Export the certificate to a byte array
                byte[] certData = certificate.RawData;
                application.ApplicationConfiguration = new ApplicationConfiguration();
                // Add the byte array directly to the trusted peer certificates
                application.ApplicationConfiguration.SecurityConfiguration.TrustedPeerCertificates.TrustedCertificates.Add(new CertificateIdentifier { RawData = certData });
            }

            var nodeManagerFactory = new MyNodeManagerFactory();

            base.AddNodeManager(nodeManagerFactory);
            
            applicationInstance.Start(this).Wait();

    
        }

        /// <summary>
        /// Called after the node managers have been started.
        /// </summary>
        /// <param name="server">The server.</param>
        protected override void OnNodeManagerStarted(IServerInternal server)
        {
            // may be overridden by the subclass.
        }
    }
}
