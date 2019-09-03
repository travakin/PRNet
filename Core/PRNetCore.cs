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
using PRNet.Core;

namespace PRNet {

    public class PRNetCore{

		private int nextPacketId = 1;

        protected UdpClient udpClient;

		protected IRecordSentPackets sentPacketRecorder;
		protected IRecordReceivedPackets receivedPacketRecorder;
		protected INetworkMonitor monitor;

        protected bool stopThreads = false;

		protected void StampPacket(Packet packet) {

			packet.timeStamp = DateTime.Now;

			if (packet.packetId == 0) {

				packet.packetId = nextPacketId;
				nextPacketId++;
			}
		}

		protected void ParseAck(Packet packet)
			=> receivedPacketRecorder.RecordReceivedPacket(packet);

		protected void ParseAck(Packet packet, NetworkConnection conn) 
			=> receivedPacketRecorder.RecordReceivedPacket(packet, conn);

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