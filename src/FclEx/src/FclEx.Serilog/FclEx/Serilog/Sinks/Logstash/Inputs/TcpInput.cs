using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using FclEx.Extensions;

namespace FclEx.Serilog.Sinks.Logstash.Inputs
{
    internal class TcpInput : ILogstashInput
    {
        private readonly Uri _uri;
        public TcpInput(Uri uri)
        {
            _uri = uri;
        }

        private static readonly char _newLine = '\n';
        private static readonly byte[] _line = { (byte)_newLine };

        public async Task SendAsync(IReadOnlyList<string> list)
        {
            using var client = new TcpClient();
            await client.ConnectAsync(_uri.Host, _uri.Port).DonotCapture();
            using var writer = new StreamWriter(client.GetStream());
            foreach (var item in list)
            {
                writer.Write(item);
                writer.Write(_newLine);
            }
            //writer.Write(_newLine);
            await writer.FlushAsync().DonotCapture();
        }
    }
}
