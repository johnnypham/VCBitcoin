using System;
using System.Net;

namespace VCBitcoinNetwork {

    public enum Net { Main, Test };

    public class NodeConnection {

        public string SocketAddress { get; }

        public string Username { get; }

        public string Password { get; }

        public Net NetType { get; }

        public NodeConnection(Net netType,
            string ipAddress, string username, string password) {

            if (!IPAddress.TryParse(ipAddress, out IPAddress ip)) {
                throw new ArgumentException($"{ipAddress} is not a valid IP Address.");
            }

            Username = username;
            Password = password;
            NetType = netType;

            int port = (netType == Net.Main) ? 8332 : 18332;

            SocketAddress = $"http://{ipAddress}:{port}";
        }

        public NodeConnection(Net netType, string ipAddress,
            int port, string username, string password) {

            if (!IPAddress.TryParse(ipAddress, out IPAddress ip)) {
                throw new ArgumentException($"{ipAddress} is not a valid IP Address.");
            }

            if (port < 1 || port > 65535) {
                throw new ArgumentException($"{port} is not a valid port.");
            }

            Username = username;
            Password = password;
            NetType = netType;

            SocketAddress = $"http://{ipAddress}:{port}";
        }

    }

}