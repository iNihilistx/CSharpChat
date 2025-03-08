using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

class Server
{
    private HashSet<Socket> clients = new HashSet<Socket>();
    private string ip;
    private int port;
    private Socket listener;
    private bool isRunning = true; 

    public string IpAddress
    {
        get { return ip; }
        set { ip = value; }
    }
    public int Port
    {
        get { return port; }
        set { port = value; }
    }

    public Server(string ipaddress, int portnumber)
    {
        IpAddress = ipaddress;
        Port = portnumber;
    }

    public async Task Start()
    {
        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Parse(IpAddress), Port));
        listener.Listen(10);
        Console.WriteLine($"Server started on: {IpAddress} : {Port}");

        while(isRunning)
        {
            var ClientSocket = await listener.AcceptAsync();
            clients.Add(ClientSocket);
            Console.WriteLine("Client connected");

            // Handle the client in another thread, to allow for other clients to be accepted
            _ = Task.Run(() => HandleClient(ClientSocket));
        }
    }

    private async Task HandleClient(Socket client)
    {
        var buffer = new byte[1024];
        try
        {
            while(isRunning)
            {
                var received = await client.ReceiveAsync(buffer, SocketFlags.None);
                if (received == 0) break;

                var response = Encoding.UTF8.GetString(buffer, 0, received);
                Console.WriteLine(response);
                Broadcast(response, client);
            }
        }
        catch(Exception e)
        {
           // Console.WriteLine(e.Message);
        }
        finally
        {
            client.Close();
            clients.Remove(client);
            //Console.WriteLine("Client Disconnected");
        }
    }

    private async void Broadcast(string message, Socket sender )
    {
        foreach(var client in clients)
        {
            if(client != sender)
            {
                var messageBytes = Encoding.UTF8.GetBytes(message);
                await client.SendAsync(messageBytes, SocketFlags.None);
            }
        }
    }
}

class Program
{
    public static async Task Main(string[] args)
    {
        Server server = new Server("0.0.0.0", 8080);
        await server.Start();
    }
}