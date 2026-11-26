using Opc.UaFx;
using Opc.UaFx.Client;
using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Text;
using System.Windows;

namespace WpfApp;

public class ConnectionToOpcServer
{
    public static (int, int) GetParameters()
    {
        using var client = new OpcClient("opc.tcp://127.0.0.1:49320");

        try
        {
            client.Connect();

            var values = GetFromServer(client);
            return values;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка при получении данных: " + ex.Message, "Ошибка");
            return (0, 0);           
        }
        finally
        {
            client?.Disconnect();
        }
    }

    private static (int, int) GetFromServer(OpcClient client)
    {
        int v1 = 0, v2 = 0;
        OpcReadNode[] rCommands = new OpcReadNode[] {
                new OpcReadNode("ns=2;s=ChannelMitsubishiFX.DeviceFX3U.MetrazhSutki"),
                new OpcReadNode("ns=2;s=ChannelMitsubishiFX.DeviceFX3U.SpeedShushStol"),
            };
        var job = client.ReadNodes(rCommands);
        v1 = Convert.ToInt32(job.First().Value);
        v2 = Convert.ToInt32(job.Skip(1).First().Value);
        return (v1, v2);
    }
}
