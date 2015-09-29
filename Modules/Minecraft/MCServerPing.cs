//
//  File: MCServerPing.cs
//  Created: 29.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Threading.Tasks;
using NLog;

namespace MCServerPing {
    public class ServerPing {
        private NetworkStream NetworkStream = null;
        private List<byte> WriteBuffer = new List<byte>();
        private int ReadOffset = 0;
        private readonly CancellationToken CancellationToken;
        private readonly String Host;
        private readonly short Port;

        public ServerPing(string host, short port, CancellationToken cancellationToken) {
            Host = host;
            Port = port;
            CancellationToken = cancellationToken;
        }

        public async Task<PingPayload> Ping(){

            NetworkStream = null;
            WriteBuffer.Clear();
            ReadOffset = 0;
            var client = new TcpClient();

            await client.ConnectAsync(Host, Port);
            if (!client.Connected)
                return null;
            
            NetworkStream = client.GetStream();


            /*
             * Send a "Handshake" packet
             * http://wiki.vg/Server_List_Ping#Ping_Process
             */
            WriteVarInt(47);
            WriteString(Host);
            WriteShort(Port);
            WriteVarInt(1);
            await Flush(0);

            /*
             * Send a "Status Request" packet
             * http://wiki.vg/Server_List_Ping#Ping_Process
             */
            await Flush(0);



            var message = new List<byte>();
            var buf = new byte[1024];
            var bytes = await NetworkStream.ReadAsync(buf, 0, buf.Length, CancellationToken);
            message.AddRange(new ArraySegment<byte>(buf, 0, bytes));
            var length = ReadVarInt(buf);
            var left = length - (message.Count - ReadOffset);
            while (left > 0) {
                buf = new byte[1024]; 
                bytes = await NetworkStream.ReadAsync(buf, 0, buf.Length, CancellationToken);
                message.AddRange(new ArraySegment<byte>(buf, 0, bytes));
                left -= bytes;
            }

            client.Close();

            ReadOffset = 0;
            var buffer = message.ToArray();
            length = ReadVarInt(buffer);
            ReadVarInt(buffer); // packetID
            var jsonLength = ReadVarInt(buffer);
            var json = ReadString(buffer, jsonLength);
            var ping = JsonConvert.DeserializeObject<PingPayload>(json);
            ping.Motd = ping.Motd != null ? CleanMotd(ping.Motd) : null;

            return ping;
        }

        private String CleanMotd(String motd){
            StringBuilder sb = new StringBuilder();
            var chars = motd.ToCharArray();
            for (var i = 0; i < chars.Length; i++) {
                if (chars[i] == '\u00A7') {
                    continue;
                }
                if (i > 1 && chars[i - 1] == '\u00A7') {
                    continue;
                }
                sb.Append(chars[i]);
            }
            return sb.ToString();
        }

        #region Read/Write methods

        internal byte ReadByte(byte[] buffer){
            var b = buffer[ReadOffset];
            ReadOffset += 1;
            return b;
        }

        internal byte[] Read(byte[] buffer, int length){
            var data = new byte[length];
            Array.Copy(buffer, ReadOffset, data, 0, length);
            ReadOffset += length;
            return data;
        }

        internal int ReadVarInt(byte[] buffer){
            var value = 0;
            var size = 0;
            int b;
            while (((b = ReadByte(buffer)) & 0x80) == 0x80) {
                value |= (b & 0x7F) << (size++ * 7);
                if (size > 5) {
                    throw new IOException("This VarInt is an imposter!");
                }
            }
            return value | ((b & 0x7F) << (size * 7));
        }

        internal string ReadString(byte[] buffer, int length){
            var data = Read(buffer, length);
            return Encoding.UTF8.GetString(data);
        }

        internal void WriteVarInt(int value){
            while ((value & 128) != 0) {
                WriteBuffer.Add((byte)(value & 127 | 128));
                value = (int)((uint)value) >> 7;
            }
            WriteBuffer.Add((byte)value);
        }

        internal void WriteShort(short value){
            WriteBuffer.AddRange(BitConverter.GetBytes(value));
        }

        internal void WriteString(string data){
            var buffer = Encoding.UTF8.GetBytes(data);
            WriteVarInt(buffer.Length);
            WriteBuffer.AddRange(buffer);
        }

        internal async Task Flush(int id = -1){
            var buffer = WriteBuffer.ToArray();
            WriteBuffer.Clear();

            var add = 0;
            var packetData = new[] { (byte)0x00 };
            if (id >= 0) {
                WriteVarInt(id);
                packetData = WriteBuffer.ToArray();
                add = packetData.Length;
                WriteBuffer.Clear();
            }

            WriteVarInt(buffer.Length + add);
            var bufferLength = WriteBuffer.ToArray();
            WriteBuffer.Clear();

            await NetworkStream.WriteAsync(bufferLength, 0, bufferLength.Length, CancellationToken);
            await NetworkStream.WriteAsync(packetData, 0, packetData.Length, CancellationToken);
            await NetworkStream.WriteAsync(buffer, 0, buffer.Length, CancellationToken);
            await NetworkStream.FlushAsync(CancellationToken);
        }

        #endregion
    }

    #region Server ping
    /// <summary>
    /// C# represenation of the following JSON file
    /// https://gist.github.com/thinkofdeath/6927216
    /// </summary>
    public class PingPayload {
        /// <summary>
        /// Protocol that the server is using and the given name
        /// </summary>
        [JsonProperty(PropertyName = "version")]
        public VersionPayload Version { get; set; }

        [JsonProperty(PropertyName = "players")]
        public PlayersPayload Players { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Motd { get; set; }

        /// <summary>
        /// Server icon, important to note that it's encoded in base 64
        /// </summary>
        [JsonProperty(PropertyName = "favicon")]
        public string Icon { get; set; }
    }

    public class VersionPayload {
        [JsonProperty(PropertyName = "protocol")]
        public int Protocol { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }

    public class PlayersPayload {
        [JsonProperty(PropertyName = "max")]
        public int Max { get; set; }

        [JsonProperty(PropertyName = "online")]
        public int Online { get; set; }

        [JsonProperty(PropertyName = "sample")]
        public List<Player> Sample { get; set; }
    }

    public class Player {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
    #endregion
}

