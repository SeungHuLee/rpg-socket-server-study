using System;
using System.Net;
using System.Net.Sockets;

namespace RPG_Server_Core
{
    class Listener
    {
        private Socket _listenSocket;
        private Action<Socket> _onAcceptHandler;

        public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {
            // 문지기 핸드폰
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _onAcceptHandler += onAcceptHandler;

            // 문지기 교육
            _listenSocket.Bind(endPoint);

            // 영업 시작
            // Backlog : 최대 대기수
            _listenSocket.Listen(10);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }

        private void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            bool isPending = _listenSocket.AcceptAsync(args);
            if (isPending == false) { OnAcceptCompleted(null, args); }
        }

        private void OnAcceptCompleted(Object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                // TODO
                _onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString()); ;
            }

            RegisterAccept(args);
        }

        public Socket Accept()
        {
            // 게임 제작 시 Blocking 계열 함수의 사용은 최대한 줄여야함


            return _listenSocket.Accept();
        }
    }
}
