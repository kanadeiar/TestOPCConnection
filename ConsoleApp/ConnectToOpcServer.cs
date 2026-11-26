using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ConsoleApp;

public class ConnectionToOpcServer
{
    private ISession _session = null!;

    private static readonly string _clientApplicationName = "OpcUaPoCClient";
    private static readonly string _clientApplicationUri = "urn:OpcUaPoCClient";
    private static readonly string _opcUaClientSessionName = "OpcUaPoCSession";
    private static readonly int _operationTimeoutInMilliseconds = 10000;
    private static readonly int _defaultSessionTimeoutInMilliseconds = 60000;

    private static readonly NodeId _nodeIdTag3 = new("ns=2;s=Channel1.Device1.Tag2");
    private static readonly NodeId _nodeIdTag4 = new("ns=2;s=Channel1.Device1.Tag3");

    private static readonly string _serverUrl = "opc.tcp://127.0.0.1:49320";

    private static readonly string _certificateStoreBasePath = "CertificateStores/UA_MachineDefault";
    private static readonly string _publisherCertificatesPath = Path.Combine(_certificateStoreBasePath, "PublisherCertificates");
    private static readonly string _trustedPeerCertificatesPath = Path.Combine(_certificateStoreBasePath, "TrustedPeerCertificates");
    private static readonly string _trustedIssuerCertificatesPath = Path.Combine(_certificateStoreBasePath, "TrustedIssuerCertificates");
    private static readonly string _rejectedCertificatesPath = Path.Combine(_certificateStoreBasePath, "RejectedCertificates");

    public static async Task<(int, int)> GetParametersAsync()
    {
        ConnectionToOpcServer client = new();
        try
        {
            Console.WriteLine("Starting OPC UA Client...");
            ApplicationConfiguration config = await buildConfigurationAsync();

            Console.WriteLine("Trying to connect to: " + _serverUrl);
            await client.ConnectToOpcServerAsync(config);

            Console.WriteLine("Reading current value of ObjectCount...");
            DataValue value1 = await client.ReadNodeValueAsync(_nodeIdTag3);
            DataValue value2 = await client.ReadNodeValueAsync(_nodeIdTag4); 
            Console.WriteLine($"Current values: {value1.Value} {value2.Value}");

            var v1 = Convert.ToInt32(value1.Value);
            var v2 = Convert.ToInt32(value2.Value);
            return (v1, v2);
        }
        catch (ServiceResultException ex)
        {
            Console.Error.WriteLine($"OPC UA error: {ex.Message} ({ex.StatusCode})");
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"Invalid argument: {ex.Message}");
        }
        finally
        {
            client.Disconnect();
        }

        return (0, 0);
    }

    private static async Task<ApplicationConfiguration> buildConfigurationAsync()
    {
        ApplicationConfiguration config = new()
        {
            ApplicationName = _clientApplicationName,
            ApplicationUri = _clientApplicationUri,
            ApplicationType = ApplicationType.Client,
            TransportQuotas = new TransportQuotas { OperationTimeout = _operationTimeoutInMilliseconds },
            ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = _defaultSessionTimeoutInMilliseconds }
        };

        ConfigureSecurity(config);
        //await EnsureApplicationCertificateAsync(config);

        await config.ValidateAsync(ApplicationType.Client);
        return config;
    }

    private static void ConfigureSecurity(ApplicationConfiguration config)
    {
        config.SecurityConfiguration = new SecurityConfiguration
        {
            ApplicationCertificate = new CertificateIdentifier { StoreType = "Directory", StorePath = _publisherCertificatesPath, SubjectName = "CN=OpcUaPoCClient" },
            TrustedPeerCertificates = new CertificateTrustList { StoreType = "Directory", StorePath = _trustedPeerCertificatesPath },
            TrustedIssuerCertificates = new CertificateTrustList { StoreType = "Directory", StorePath = _trustedIssuerCertificatesPath },
            RejectedCertificateStore = new CertificateTrustList { StoreType = "Directory", StorePath = _rejectedCertificatesPath },
            AutoAcceptUntrustedCertificates = true,
            AddAppCertToTrustedStore = true
        };

        Directory.CreateDirectory(_publisherCertificatesPath);
        Directory.CreateDirectory(_trustedPeerCertificatesPath);
        Directory.CreateDirectory(_trustedIssuerCertificatesPath);
        Directory.CreateDirectory(_rejectedCertificatesPath);
    }

    private async Task ConnectToOpcServerAsync(ApplicationConfiguration appConfig)
    {
        EndpointDescription selectedEndpoint = await CoreClientUtils.SelectEndpointAsync(
            application: appConfig,
            discoveryUrl: _serverUrl,
            useSecurity: false);

        ConfiguredEndpoint endpoint = new(
            collection: null,
            description: selectedEndpoint,
            configuration: EndpointConfiguration.Create(applicationConfiguration: appConfig));

        _session = await DefaultSessionFactory.Instance.CreateAsync(
            configuration: appConfig,
            endpoint: endpoint,
            updateBeforeConnect: false,
            sessionName: _opcUaClientSessionName,
            sessionTimeout: (uint)_defaultSessionTimeoutInMilliseconds,
            identity: null,
            preferredLocales: null);

        Console.WriteLine("Successfully connected to OPC UA server.");
    }

    public void Disconnect()
    {
        if (_session != null)
        {
            Console.WriteLine("Ending OPC UA session...");
            _session.CloseAsync();
            _session.Dispose();
            _session = null!;
            Console.WriteLine("Session ended.");
        }
    }

    private async Task<DataValue> ReadNodeValueAsync(NodeId nodeId)
    {
        ReadValueIdCollection readIds = new() { new ReadValueId { NodeId = nodeId, AttributeId = Attributes.Value } };
        ReadResponse readResponse = await _session.ReadAsync(
            requestHeader: null,
            maxAge: 0,
            timestampsToReturn: TimestampsToReturn.Neither,
            nodesToRead: readIds,
            ct: CancellationToken.None);
        DataValue responseValue = readResponse.Results[0];
        StatusCode status = responseValue.StatusCode;

        EnsureGoodStatus(nodeId, status);
        return responseValue;
    }

    private static void EnsureGoodStatus(NodeId nodeId, StatusCode status)
    {
        if (!StatusCode.IsGood(status))
        {
            Console.Error.WriteLine($"Check failed for NodeId {nodeId}: {status}");
            throw new ServiceResultException((uint)status, $"Error reading/writing node {nodeId}: {status}");
        }
        Console.WriteLine($"Read/write operation successful for NodeId {nodeId}: {status}");
    }
}
