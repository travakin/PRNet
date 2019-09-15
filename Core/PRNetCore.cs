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

			if (packet.packetId == 0) {

				packet.packetId = nextPacketId;
				nextPacketId++;
			}
		}

		protected void ParseAck(Packet packet)
			=> sentPacketRecorder.Acknowledge(packet.packetId);

		protected void ParseAck(Packet packet, NetworkConnection conn)
			=> sentPacketRecorder.Acknowledge(packet.packetId, conn);

		protected Packet GetPacketFromSocket(UdpClient socket, ref IPEndPoint clientEndPoint, IAsyncResult result) {
			
			Packet readPacket = null;

			try {

				Byte[] receivedBytes = socket.EndReceive(result, ref clientEndPoint);
				monitor.AddPacketsInbound(1);
				monitor.AddBytesInbound(receivedBytes.Length);

				if (receivedBytes.Length < 1)
					throw new EmptyDatagramException();

				readPacket = Converters.DeserializePacket(receivedBytes);

				if (readPacket == null)
					throw new NullPacketException();

				if (readPacket.packetId == 0)
					throw new UnidentifiedPacketException($"PRNet Core Error: Received packet of type {readPacket.type} with an id of 0.");

				return readPacket;
			}
			catch (SocketException se) {

				throw new SocketException();
			}
		}
	}
}