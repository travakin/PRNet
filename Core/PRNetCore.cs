using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using PRNet.Exceptions;
using PRNet.Packets;
using PRNet.NetworkEntities;
using PRNet.Utils;

namespace PRNet {

    public class PRNetCore{

        protected UdpClient udpClient;
        protected INetworkMonitor monitor;

        protected bool stopThreads = false;

        protected void ServerParseAck(Packet packet, NetworkConnection conn) {

            PacketPriorityAck ackPacket = (PacketPriorityAck)packet;

            HighPriorityMessageLog.HighPriorityServerCache cache;

            if (HighPriorityMessageLog.highPriorityMessagesServer.ContainsKey(ackPacket.responseId))
                HighPriorityMessageLog.highPriorityMessagesServer.TryRemove(ackPacket.responseId, out cache);
        }

        protected void ClientParseAck(Packet packet) {

            PacketPriorityAck ackPacket = (PacketPriorityAck)packet;

            Packet ackedPacket;

            if (HighPriorityMessageLog.highPriorityMessagesClient.ContainsKey(ackPacket.responseId))
                HighPriorityMessageLog.highPriorityMessagesClient.TryRemove(ackPacket.responseId, out ackedPacket);
        }

        protected Packet GetPacketFromSocket(UdpClient socket, ref IPEndPoint clientEndPoint, IAsyncResult result) {

            Byte[] receivedBytes = new Byte[0];
            Packet readPacket = null;

            try {

                receivedBytes = socket.EndReceive(result, ref clientEndPoint);
                monitor.AddPacketsInbound(1);
                monitor.AddBytesInbound(receivedBytes.Length);
            }
            catch (SocketException se) {

                throw new SocketException();
            }
            finally {

                if (receivedBytes.Length < 1)
                    throw new EmptyDatagramException();

                readPacket = Converters.DeserializePacket(receivedBytes);
            }

            return readPacket;
        }
    }
}