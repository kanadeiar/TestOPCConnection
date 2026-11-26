using ConsoleApp;

Console.WriteLine("Опытное приложение связи с OPC-сервером");

//var client = new OpcUaClient();
//await OpcUaClient.RunAsync();
var ones = await ConnectionToOpcServer.GetParametersAsync();

Console.WriteLine(ones.Item1);
Console.WriteLine(ones.Item2);

Console.WriteLine("Нажмите любую кнопку ...");
Console.ReadKey();