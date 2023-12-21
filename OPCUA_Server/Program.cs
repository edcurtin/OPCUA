using Opc.Ua;
using Opc.Ua.Configuration;
using Opc.Ua.Server;
using OPCUA_Server;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            MyOPCUAServer server = new MyOPCUAServer();
        
            server.Start();
        }
        catch(Exception ex)
        {

        }
        Console.ReadLine();
    }
}