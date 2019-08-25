using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using OPS.Serialization.Attributes;

namespace PRNet.NetworkEntities {

    [Serializable]
    public class NetworkConnection : IEquatable<NetworkConnection>{

        public IPEndPoint endPoint;
        public int clientId;
        public DateTime timestamp;
        public int ping;

        public NetworkConnection(IPEndPoint ep, DateTime time) {

            endPoint = ep;
            timestamp = time;
        }

        public bool Equals(NetworkConnection other) {

            return this.endPoint.Address.ToString().Equals(other.endPoint.Address.ToString()) && this.endPoint.Port == other.endPoint.Port;
        }
    }
}
