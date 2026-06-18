using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetworkScript : MonoBehaviour
{
    string IP = "localhost";
    int port = 54000;
    public static TcpClient client = new TcpClient();

    public static Queue<String> messageQueue = new Queue<string>();
    

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            client.Connect(IP, port);
        }

        catch (Exception ex)
        {

        }

        Thread reader = new Thread(new ThreadStart(MessageReader));

        reader.Start();
    }

    public static void MessageReader()
    {
        while (true)
        {
            print("Reading Stream");
            byte[] bytes = new byte[client.ReceiveBufferSize];
            NetworkStream stream = client.GetStream();
            stream.Read(bytes, 0, client.ReceiveBufferSize);

            string message = Encoding.UTF8.GetString(bytes);
            print("MESSAGE LENGTH: " + bytes.Length);
            print(message);

            messageQueue.Enqueue(message);

            if (messageQueue.Count > 100)
            {
                return;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
        
    }

    public void SendMessageToServer(string message)
    {
        print("Sending " + message);
        if (client.Connected)
        {
            NetworkStream stream = client.GetStream();
            stream.Write(Encoding.ASCII.GetBytes(message), 0, 4);
        }
    }
}
