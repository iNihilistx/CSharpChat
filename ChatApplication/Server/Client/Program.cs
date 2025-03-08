using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class ClientSocket
{
    private string ip;
    private int port;
    private Socket clientSocket;

    public string Username;
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

    public ClientSocket(string username, string ipaddress, int portnumber)
    {
        Username = username;
        IpAddress = ipaddress;
        Port = portnumber;
    }

    public async Task Start()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await clientSocket.ConnectAsync(new IPEndPoint(IPAddress.Parse(IpAddress), Port));
        Console.WriteLine($"Connected to server: {IpAddress} : {Port}");

        _ = Task.Run(() => ReceiveMessages());
        
        while (true)
        {
            Console.Write("\nStart Chatting: ");
            string message = Console.ReadLine();
            string UsernameMessage = $"{Username}: {message}";
            if(string.IsNullOrEmpty(message))
            {
                Console.WriteLine("Message cannot be empty");
                continue;
            }
            else if(message.ToLower() == "exit")
            {
                await SendMessage($"{Username} has left the chat");
                break;
            }
            else
            {
                await SendMessage(UsernameMessage);
            }
        }
    }

    private async Task SendMessage(string message)
    {
        try
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            await clientSocket.SendAsync(buffer, SocketFlags.None);

            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.WriteLine($"{message}");
        }
        catch(Exception ex)
        {
            Environment.Exit(0);
        }
    }

    private async Task ReceiveMessages()
    {
        while(true)
        {
            var buffer = new byte[1024];
            var received = clientSocket.Receive(buffer);
            var message = Encoding.UTF8.GetString(buffer, 0, received);
            Console.WriteLine($"{message}");
        }
    }
}

class Program
{
    public static async Task Main(string[] args)
    {
        ClientSocket client = new ClientSocket("user", "0.0.0.0", 8080);
        await client.Start();
    }
}