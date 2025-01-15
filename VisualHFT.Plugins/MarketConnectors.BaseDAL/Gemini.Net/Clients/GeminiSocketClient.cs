using Gemini.Net.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Websocket.Client;

namespace Gemini.Net.Clients
{
    public class GeminiSocketClient
    {
        WebsocketClient socket;

        public event EventHandler<GemeniTradeResponse> ClientConnected;
        public event EventHandler<EventArgs> ClientDisconnected;
        public event EventHandler<EventArgs> OnDataReceived;

        public GeminiSocketClient()
        {
            socket = new WebsocketClient(new Uri("wss://api.gemini.com/v2/marketdata?heartbeat=true"));
            socket.ReconnectTimeout = TimeSpan.FromSeconds(30);

            
            //socket.NativeClient.Options.SetRequestHeader();
            socket.ReconnectionHappened.Subscribe(data =>
            {
                GemeniTradeResponse trade = new GemeniTradeResponse();
                ClientConnected.Invoke(socket, trade);
            });
            
            socket.DisconnectionHappened.Subscribe(data =>
            {
                 
            });
            
            socket.MessageReceived.Subscribe(data =>
            {

            });

            socket.Start();
        }





    }
}
