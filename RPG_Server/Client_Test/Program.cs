﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            // DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            while (true)
            {
                // 휴대폰 설정
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // 문지기에게 입장 문의
                    socket.Connect(ipAddr, 7777); // 서버 대기
                    Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()}");

                    // 보낸다
                    byte[] sendBuff = Encoding.UTF8.GetBytes("Hello, World!");
                    int sendBytes = socket.Send(sendBuff); // 서버 대기

                    // 받는다
                    byte[] recvBuffer = new byte[1024];
                    int recvBytes = socket.Receive(recvBuffer); // 서버 대기
                    string recvData = Encoding.UTF8.GetString(recvBuffer, 0, recvBytes);
                    Console.WriteLine($"[From Server] {recvData}");

                    // 나간다
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(100);
            }
        }
    }
}
