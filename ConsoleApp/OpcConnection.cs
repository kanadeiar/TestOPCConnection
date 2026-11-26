//using Opc.UaFx;
//using Opc.UaFx.Client;


using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Net.Sockets;

namespace ConsoleApp;

// Sumulator
// Channel1.Device1
//   Tag1 bool
//   Tag2 float
//   Tag3 word
//   Tag4 string

public class OpcConnection
{
    public static async void Run()
    {
        ApplicationConfiguration applicationConfiguration = new ApplicationConfiguration();

        //using var client = new OpcClient("opc.tcp://127.0.0.1:49320");
        //client.Security.UserIdentity = new OpcClientIdentity("Tim", "1qaz2wsx3edc4rfv");
        //client.Security.EndpointPolicy = new OpcSecurityPolicy(OpcSecurityMode.SignAndEncrypt, OpcSecurityAlgorithm.Basic256);

            //try
            //{           
            //    client.Connect();

            //    AdvancedCommuticateWithServer(client);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
            //finally
            //{
            //    client?.Disconnect();
            //}
    }

    //private static void AdvancedCommuticateWithServer(OpcClient client)
    //{
    //    OpcSubscription subscription = client.SubscribeDataChange("ns=2;s=Channel1.Device1.Tag2", HandleDataChanged);
    //    subscription.PublishingInterval = 3000;
    //    subscription.ApplyChanges();

    //    Console.WriteLine("Нажать любую кнопку для записи данных ...");
    //    Console.ReadKey();

    //    OpcWriteNode[] wCommands = new OpcWriteNode[] {
    //            new OpcWriteNode("ns=2;s=Channel1.Device1.Tag1", false),    
    //            new OpcWriteNode("ns=2;s=Channel1.Device1.Tag2", 0.1),   
    //            new OpcWriteNode("ns=2;s=Channel1.Device1.Tag3", (ushort)1),     
    //            new OpcWriteNode("ns=2;s=Channel1.Device1.Tag4", "test1"),
    //        };
    //    var results = client.WriteNodes(wCommands);
    //    Console.WriteLine("Результат: {0}", results.First().Description);

    //    Console.WriteLine("Данные:");

    //    OpcReadNode[] rCommands = new OpcReadNode[] {
    //            new OpcReadNode("ns=2;s=Channel1.Device1.Tag1"),
    //            new OpcReadNode("ns=2;s=Channel1.Device1.Tag2"),
    //            new OpcReadNode("ns=2;s=Channel1.Device1.Tag3"),
    //            new OpcReadNode("ns=2;s=Channel1.Device1.Tag4"),
    //        };
    //    var job = client.ReadNodes(rCommands);
    //    int i = 0;
    //    foreach (OpcValue value in job)
    //    {
    //        Console.WriteLine("ReadNode: {0}, = {1}", rCommands[i].NodeId, value);
    //        i++;
    //    }

    //    Console.WriteLine("Нажать любую кнопку для завершения ...");
    //    Console.ReadKey();
    //}

    //private static void Example(OpcClient client)
    //{
    //    OpcWriteNode[] wCommands = new OpcWriteNode[] {
    //            new OpcWriteNode("ns=2;s=Channel2.Device1.Tag1", false),    // 寫 boolean
    //            new OpcWriteNode("ns=2;s=Channel2.Device1.Tag2", "Test"),   // 寫 sting
    //            new OpcWriteNode("ns=2;s=Channel2.Device1.Tag3", 8.7),      // 寫 float
    //            new OpcWriteNode("ns=2;s=Channel2.Device1.Tag3", (ushort)88)// 寫 word
    //        };
    //    OpcStatusCollection results = client.WriteNodes(wCommands);

    //    OpcReadNode[] rCommands = new OpcReadNode[] {
    //            new OpcReadNode("ns=2;s=Channel2.Device1.Tag1"),
    //            new OpcReadNode("ns=2;s=Channel2.Device1.Tag2"),
    //            new OpcReadNode("ns=2;s=Channel2.Device1.Tag3"),
    //            new OpcReadNode("ns=2;s=Channel2.Device1.Tag4")
    //        };
    //    IEnumerable<OpcValue> job = client.ReadNodes(rCommands);
    //    int i = 0;
    //    foreach (OpcValue value in job)
    //    {
    //        Console.WriteLine("ReadNode: {0},\t = {1}", rCommands[i].NodeId, value);
    //        i++;
    //    }

    //    OpcSubscription subscription = client.SubscribeDataChange("ns=2;s=Channel2.Device1.Tag5", HandleDataChanged);
    //    subscription.PublishingInterval = 1000;
    //    subscription.ApplyChanges();
    //}

    //private static void SimpleCommunicateWithServer(OpcClient client)
    //{
    //    Console.WriteLine("Нажать любую кнопку для получения данных ...");

    //    //Console.WriteLine("Нажать любую кнопку для чтения ...");
    //    //Console.ReadKey();

    //    //Console.WriteLine("ReadNode: {0}", client.ReadNode("ns=2;s=Channel1.Device1.Tag1"));

    //    //Console.WriteLine("Нажать любую кнопку для записи 1");
    //    //Console.ReadKey();

    //    //var result = client.WriteNode("ns=2;s=Channel1.Device1.Tag1", (ushort)1);
    //    //Console.WriteLine("ReadNode: {0}", client.ReadNode("ns=2;s=Channel1.Device1.Tag1"));
    //}

    //private static void HandleDataChanged(object sender, OpcDataChangeReceivedEventArgs e)
    //{
    //    OpcMonitoredItem item = (OpcMonitoredItem)sender;
    //    Console.WriteLine("Изменение данных: {0} = {1}", item.NodeId, e.Item.Value);
    //}
}
