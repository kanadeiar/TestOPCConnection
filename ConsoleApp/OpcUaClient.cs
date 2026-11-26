using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp;

public class OpcUaClient
{
    private ISession _session = null!;

    private static readonly string _clientApplicationName = "OpcUaPoCClient";
    private static readonly string _clientApplicationUri = "urn:OpcUaPoCClient";
    private static readonly int _operationTimeoutInMilliseconds = 10000;
    private static readonly int _defaultSessionTimeoutInMilliseconds = 60000;

    private static readonly NodeId _nodeIdObjectCount = new("ns=2;s=Channel1.Device1.Tag3");

    private static readonly string _serverUrl = "opc.tcp://127.0.0.1:49320";
    private static readonly string _opcUaClientSessionName = "OpcUaPoCSession";

    private static readonly string _certificateStoreBasePath = "CertificateStores/UA_MachineDefault";
    private static readonly string _publisherCertificatesPath = Path.Combine(_certificateStoreBasePath, "PublisherCertificates");
    private static readonly string _trustedPeerCertificatesPath = Path.Combine(_certificateStoreBasePath, "TrustedPeerCertificates");
    private static readonly string _trustedIssuerCertificatesPath = Path.Combine(_certificateStoreBasePath, "TrustedIssuerCertificates");
    private static readonly string _rejectedCertificatesPath = Path.Combine(_certificateStoreBasePath, "RejectedCertificates");

    /// <summary>
    /// Runs the OPC UA client asynchronously, connecting to the server by the specified URL,
    /// reading the current value of the specific node, writing a new value to it,
    /// and then reading the updated value.
    /// It handles exceptions related to OPC UA operations and argument validation.
    /// It also ensures proper cleanup of the session after operations are completed.
    /// </summary>
    /// <returns>A Task that represents the asynchronous operation.</returns>
    public static async Task RunAsync()
    {
        OpcUaClient client = new();
        try
        {
            Console.WriteLine("Starting OPC UA Client...");
            ApplicationConfiguration config = await BuildConfigurationAsync();

            Console.WriteLine("Trying to connect to: " + _serverUrl);
            await client.ConnectToOpcServerAsync(config);

            Console.WriteLine("Reading current value of ObjectCount...");
            DataValue initial = await client.ReadNodeValueAsync(_nodeIdObjectCount);
            Console.WriteLine($"Current value: {initial.Value}");

            // TODO - Specify the value to write
            string newValue = "55";
            Console.WriteLine($"Trying to write new value ({newValue})");
            await client.WriteNodeValueAsync(_nodeIdObjectCount, newValue);

            Console.WriteLine("Reading updated value of ObjectCount...");
            DataValue updated = await client.ReadNodeValueAsync(_nodeIdObjectCount);
            Console.WriteLine($"Updated value: {updated.Value}");
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
    }

    /// <summary>
    /// Builds the application configuration for the OPC UA client.
    /// </summary>
    /// <returns>A Task that represents the asynchronous operation, containing the ApplicationConfiguration.</returns>
    private static async Task<ApplicationConfiguration> BuildConfigurationAsync()
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
        await EnsureApplicationCertificateAsync(config);

        await config.ValidateAsync(ApplicationType.Client);
        return config;
    }

    /// <summary>
    /// Configures the security settings for the OPC UA client and sets up the certificate stores.
    /// </summary>
    /// <param name="config">The application configuration to modify.</param>
    /// <remarks>
    /// This method sets up the security configuration for the OPC UA client, including paths for certificate stores
    /// (publisher, trusted peer, trusted issuer, and rejected certificates),
    /// enabling auto-accept for untrusted certificates, and adding the application certificate to the trusted store.
    /// </remarks>
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

    /// <summary>
    /// Ensures that the application certificate exists, generating it if necessary.
    /// </summary>
    /// <param name="config">The application configuration containing the security settings.</param>
    /// <remarks>
    /// This method checks if the application certificate exists in the specified store. 
    /// If it does not exist, it generates a self-signed certificate with the specified parameters.
    /// If it exists, it logs a message indicating that the certificate is already present.
    /// </remarks>
    private static async Task EnsureApplicationCertificateAsync(ApplicationConfiguration config)
    {
        CertificateIdentifier appCert = config.SecurityConfiguration.ApplicationCertificate;
        var foundCertificate = await appCert.FindAsync(true);

        if (foundCertificate == null)
        {
            Console.WriteLine("Generating self-signed application certificate...");
#pragma warning disable CS0618
            //foundCertificate = CertificateFactory.CreateCertificate(
            //    storeType: appCert.StoreType,
            //    storePath: appCert.StorePath,
            //    password: null,
            //    applicationUri: config.ApplicationUri,
            //    applicationName: config.ApplicationName,
            //    subjectName: appCert.SubjectName,
            //    domainNames: new List<string> { Utils.GetHostName() },
            //    keySize: 2048,
            //    startTime: DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)),
            //    lifetimeInMonths: 24,
            //    hashSizeInBits: 256,
            //    isCA: false,
            //    issuerCAKeyCert: null,
            //    publicKey: null,
            //    pathLengthConstraint: 0);
#pragma warning restore CS0618
            appCert.Certificate = foundCertificate;
        }
        else
        {
            // TODO - Use the existing certificate ?
            Console.WriteLine("There is an existing application certificate...");
        }
    }

    /// <summary>
    /// Connects to the OPC UA server using the provided configuration.
    /// </summary>
    /// <param name="appConfig">The application configuration.</param>
    /// <exception cref="ServiceResultException">Thrown if the connection fails.</exception>
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

    /// <summary>
    /// Reads the value of the specified NodeId asynchronously.
    /// </summary>
    /// <param name="nodeId">The NodeId to read.</param>
    /// <returns>A Task that represents the asynchronous operation, containing the DataValue read from the node.</returns>
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

    /// <summary>
    /// Writes a new integer value to the specified NodeId.
    /// </summary>
    /// <param name="nodeId">The NodeId to write to.</param>
    /// <param name="textValue">The value to write, as a string.</param>
    private async Task WriteNodeValueAsync(NodeId nodeId, string textValue)
    {
        int intValue = ValidateAndParseInt(textValue);
        WriteValue nodeValueToWrite = new() 
        { 
            NodeId = nodeId, 
            AttributeId = Attributes.Value, 
            Value = new DataValue(new Variant((ushort)intValue), StatusCodes.Good)
        };
        WriteValueCollection writeOperations = new() { nodeValueToWrite };
        WriteResponse writeResponse = await _session.WriteAsync(
            requestHeader: null,
            nodesToWrite: writeOperations,
            ct: CancellationToken.None);
        StatusCode status = writeResponse.Results[0];

        EnsureGoodStatus(nodeId, status);
    }

    /// <summary>
    /// Validates and parses the input string as an integer.
    /// </summary>
    /// <param name="textValue">The string value to validate and parse.</param>
    /// <returns>The parsed integer value.</returns>
    /// <exception cref="ArgumentException">Thrown if the input string is not a valid integer.</exception>
    private static int ValidateAndParseInt(string textValue)
    {
        if (!int.TryParse(textValue, out int intValue))
        {
            Console.Error.WriteLine($"Invalid value for ObjectCount: '{textValue}' is not a valid int.");
            throw new ArgumentException($"Provide a valid integer value. Received: '{textValue}'.", nameof(textValue));
        }
        return intValue;
    }

    /// <summary>
    /// Ensures that the status code of the read/write operation is good.
    /// If the status is not good, it throws a ServiceResultException with the status code.
    /// </summary>
    /// <param name="nodeId">The NodeId that was written to.</param>
    /// <param name="status">The status code returned from the write operation.</param>
    /// <exception cref="ServiceResultException">Thrown if the write operation was not successful.</exception>
    private static void EnsureGoodStatus(NodeId nodeId, StatusCode status)
    {
        if (!StatusCode.IsGood(status))
        {
            Console.Error.WriteLine($"Check failed for NodeId {nodeId}: {status}");
            throw new ServiceResultException((uint)status, $"Error reading/writing node {nodeId}: {status}");
        }
        Console.WriteLine($"Read/write operation successful for NodeId {nodeId}: {status}");
    }

    /// <summary>
    /// Disconnects from the OPC UA server and cleans up the session - by closing and disposing it.
    /// If the session is already null, it does nothing.
    /// </summary>
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
}
