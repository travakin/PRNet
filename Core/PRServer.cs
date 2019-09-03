using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PRNet.Exceptions;
using PRNet.Packets;
using PRNet.Meta;
using PRNet.NetworkEntities;
using PRNet.Requests;
using PRNet.Utils;
using UnityEngine;

namespace PRNet.Core {

    public class PRServer : PRNetCore {

        private const int MAX_NETWORK_MESSAGES_PER_PACKET = 16;
        private const int SIO_UDP_CONNRESET = -1744830452;
        private const int MAX_CONNECTIONS = 16;
        private const int TIMEOUT_SEC = 20;
        private const int HIGH_PRIORITY_RECORD_LIFESPAN = 3;
        private const int HIGH_PRIORITY_WAIT_MS = 200;

        private NetworkObjectsManager objectsManager;

		private NetworkConnection[] clientConnections = new NetworkConnection[MAX_CONNECTIONS];
        private List<NetworkConnection> pendingConnections = new List<NetworkConnection>();
        private Action<Packet, NetworkConnection> invokePacketParsers;

        private DateTime lastPacketCountTime = DateTime.Now;

        private Dictionary<int, Action<Packet, NetworkConnection>> parsers = new Dictionary<int, Action<Packet, NetworkConnection>>();

        public PRServer(int port, NetworkObjectsManager manager, IRecordSentPackets sentPacketRecorder, IRecordReceivedPackets receivedPacketRecorder, INetworkMonitor monitor) {

            this.objectsManager = manager;
			this.sentPacketRecorder = sentPacketRecorder;
			this.receivedPacketRecorder = receivedPacketRecorder;
            this.monitor = monitor;

            udpClient = new UdpClient(47777);

            udpClient.Client.IOControl(
                (IOControlCode)SIO_UDP_CONNRESET,
                new byte[] { 0, 0, 0, 0 },
                null
            );

            udpClient.BeginReceive(new AsyncCallback(UdpServerData), udpClient);

            parsers.Add(Packet.PACKET_CONNECTIONREQUEST, ParseConnectionRequestPacket);
            parsers.Add(Packet.PACKET_CHALLENGERESPONSE, ParseChallengeResponsePacket);
            parsers.Add(Packet.PACKET_CLIENTREADY, ParseClientReadyPacket);
            parsers.Add(Packet.PACKET_ACK, ParseAck);
            parsers.Add(Packet.PACKET_STATEREQUEST, ParseStateRequestPacket);
            parsers.Add(Packet.PACKET_NETWORKMESSAGE, ServerStage.ParseNetworkMessages);
            parsers.Add(Packet.PACKET_CLIENTDISCONNECTED, ParseClientDisconnectPacket);

            invokePacketParsers = new Action<Packet, NetworkConnection>((readPacket, newConnection) => parsers[readPacket.type](readPacket, newConnection));

            Thread timeoutDetectionThread = new Thread(new ThreadStart(ChangeDetectionThread));
            timeoutDetectionThread.Start();
        }

        public void Stop() {

            stopThreads = true;

            if (stopThreads)
                udpClient.Close();
        }

        private void UdpServerData(IAsyncResult result) {

            UdpClient socket = result.AsyncState as UdpClient;

            socket.BeginReceive(new AsyncCallback(UdpServerData), socket);

            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);

            Packet readPacket = GetPacketFromSocket(socket, ref clientEndPoint, result);

            NetworkConnection newConnection = new NetworkConnection(clientEndPoint, DateTime.Now);

            int idx = Array.IndexOf(clientConnections, newConnection);
            if (idx > -1)
                newConnection = clientConnections[idx];

            RecordHighPriorityPacket(readPacket, newConnection);

            invokePacketParsers(readPacket, newConnection);
        }

        private void RecordHighPriorityPacket(Packet readPacket, NetworkConnection newConnection) {

            PacketPriorityAck ackPacket = new PacketPriorityAck();
            ackPacket.responseId = readPacket.packetId;

            SendPacket(ackPacket, newConnection);

			if (receivedPacketRecorder.HasReceivedPacket(readPacket, newConnection))
				return;

			receivedPacketRecorder.RecordReceivedPacket(readPacket, newConnection);
        }

        private void ChangeDetectionThread() {

            while (!stopThreads) {

                CheckTimeoutConnections();
                ResendHighPriorityPackets();
                DeleteExpiredHighPriorityRecords();
                DisplayData();
            }
        }

        private void CheckTimeoutConnections() {

            for (int i = 0; i < clientConnections.Length; i++) {

                if (clientConnections[i] == null)
                    continue;

                if ((DateTime.Now - clientConnections[i].timestamp).Seconds > TIMEOUT_SEC) {

                    NetworkEventPayload.ServerClientDisconnectPayload payload = new NetworkEventPayload.ServerClientDisconnectPayload();

                    ServerStage.EnqueueNetworkEvent(ServerStage.EVENT_CLIENTDISCONNECTED, clientConnections[i], payload);

                    objectsManager.pendingMessagesOutboundTargeted.Remove(clientConnections[i]);
                    objectsManager.pendingMessagesOutboundTargetedHP.Remove(clientConnections[i]);
                    clientConnections[i] = null;

					sentPacketRecorder.ClearPacketsForConnection(clientConnections[i]);
                }
            }
        }

        private void ResendHighPriorityPackets() {

			List <PacketConnection> expiredPackets = sentPacketRecorder.RetrieveExpiredSentPackets();

            foreach (PacketConnection record in expiredPackets) {

				SendPacket(record.packet, record.conn);
            }
        }

        private void DeleteExpiredHighPriorityRecords() 
			=> receivedPacketRecorder.ClearExpiredReceivedPacketRecords();

        private void DisplayData() {

            if ((DateTime.Now - lastPacketCountTime).TotalSeconds > 1) {

                //Debug.Log("Current inbound packets: " + monitor.GetPacketsInbound() + " packets for a total of " + monitor.GetBytesInbound() + " bytes");
                //Debug.Log("Current outbound packets: " + monitor.GetPacketsOutbound() + " packets for a total of " + monitor.GetBytesOutbound() + " bytes");

                monitor.ClearData();
                lastPacketCountTime = DateTime.Now;
            }
        }

        private void ParseConnectionRequestPacket(Packet packet, NetworkConnection clientConnection) {

            if (Array.IndexOf(clientConnections, clientConnection) > -1 || pendingConnections.Contains(clientConnection))
                return;

            pendingConnections.Add(clientConnection);

            PacketChallengeRequest packetChallengeRequest = new PacketChallengeRequest();

            SendPacket(packetChallengeRequest, clientConnection);
        }

        private void ParseChallengeResponsePacket(Packet packet, NetworkConnection clientConnection) {

            PacketChallengeResponse response = (PacketChallengeResponse)packet;

            if (Array.IndexOf(clientConnections, clientConnection) > -1)
                return;

            if (!pendingConnections.Contains(clientConnection)) {

                return;
            }

            if (!(Array.IndexOf(clientConnections, clientConnection) > -1)) {


                int slotIndex = GetEmptyServerSlotIndex();

                if (slotIndex != -1) {

                    clientConnection.clientId = slotIndex;
                    clientConnections[slotIndex] = clientConnection;

                    objectsManager.pendingMessagesOutboundTargeted.Add(clientConnection, new List<NetworkMessage>());
                    objectsManager.pendingMessagesOutboundTargetedHP.Add(clientConnection, new List<NetworkMessage>());
                }
                else {
                }
            }

            PacketConnectionConfirm confimation = new PacketConnectionConfirm(clientConnection.clientId);
            confimation.levelName = ServerData.CurrentLevelName;

            SendPacket(confimation, clientConnection);

            NetworkEventPayload.ServerClientConnectPayload payload = new NetworkEventPayload.ServerClientConnectPayload();
            payload.username = response.username;
            payload.playerName = response.playerName;
            payload.playerAccountBalance = response.playerAccountBalance;

            ServerStage.EnqueueNetworkEvent(ServerStage.EVENT_CLIENTCONNECTED, clientConnection, payload);
        }

        private int GetEmptyServerSlotIndex() {

            int ret = -1;

            for (int i = 0; i < clientConnections.Length; i++) {

                if (clientConnections[i] == null) {

                    ret = i;
                    break;
                }
            }

            return ret;
        }

        private void ParseClientReadyPacket(Packet packet, NetworkConnection clientConnection) {

            ServerStage.ClientInitializationUpdate(clientConnection);

            NetworkEventPayload.ServerClientReadyPayload payload = new NetworkEventPayload.ServerClientReadyPayload();
            ServerStage.EnqueueNetworkEvent(ServerStage.EVENT_CLIENTREADY, clientConnection, payload);
        }

        private void ParseStateRequestPacket(Packet packet, NetworkConnection clientConnection) {

            int idx = Array.IndexOf(clientConnections, clientConnection);

            if (!(idx > -1)) 
                return;
            else 
                clientConnection = clientConnections[idx];

            clientConnection.timestamp = DateTime.Now;
            clientConnection.ping = (int)(DateTime.Now - packet.timeStamp).TotalMilliseconds * 2;

            PacketStateRequest parsePacket = (PacketStateRequest)packet;
        }

        private void ParseClientDisconnectPacket(Packet packet, NetworkConnection clientConnection) {

            ServerStage.EnqueueNetworkEvent(ServerStage.EVENT_CLIENTDISCONNECTED, clientConnection, new NetworkEventPayload.ServerClientDisconnectPayload());

            for (int i = 0; i < clientConnections.Length; i++) {

                if (clientConnections[i] == clientConnection) {

                    objectsManager.pendingMessagesOutboundTargeted.Remove(clientConnections[i]);
                    objectsManager.pendingMessagesOutboundTargetedHP.Remove(clientConnections[i]);

                    clientConnections[i] = null;
                    break;
                }
            }
        }

        public void SendClientUpdates(Packet packet) {

            SendPacket(packet);
        }

        public void SendClientUpdates(Packet packet, NetworkConnection target) {

            SendPacket(packet, target);
        }

        public void SendNetworkMessages() {

            List<NetworkMessage> messages = objectsManager.pendingMessagesOutbound.GetRange(0, objectsManager.pendingMessagesOutbound.Count);
            objectsManager.pendingMessagesOutbound.RemoveRange(0, objectsManager.pendingMessagesOutbound.Count);

            List<NetworkMessage> messagesHP = objectsManager.pendingMessagesOutboundHP.GetRange(0, objectsManager.pendingMessagesOutboundHP.Count);
            objectsManager.pendingMessagesOutboundHP.RemoveRange(0, objectsManager.pendingMessagesOutboundHP.Count);

            SendUntargeted(messages, false);
            SendUntargeted(messagesHP, true);

            for (int i = 0; i < clientConnections.Length; i++) {

                if (clientConnections[i] == null)
                    continue;

                SendNetworkMessages(clientConnections[i]);
            }
        }

        private void SendNetworkMessages(NetworkConnection currentConnection) {

            List<NetworkMessage> targetMessages = objectsManager.pendingMessagesOutboundTargeted[currentConnection].GetRange(0, objectsManager.pendingMessagesOutboundTargeted[currentConnection].Count);
            objectsManager.pendingMessagesOutboundTargeted[currentConnection].RemoveRange(0, objectsManager.pendingMessagesOutboundTargeted[currentConnection].Count);

            List<NetworkMessage> targetMessagesHP = objectsManager.pendingMessagesOutboundTargetedHP[currentConnection].GetRange(0, objectsManager.pendingMessagesOutboundTargetedHP[currentConnection].Count);
            objectsManager.pendingMessagesOutboundTargetedHP[currentConnection].RemoveRange(0, objectsManager.pendingMessagesOutboundTargetedHP[currentConnection].Count);

            SendTargeted(targetMessages, currentConnection, false);
            SendTargeted(targetMessagesHP, currentConnection, true);
        }

        private void SendUntargeted(List<NetworkMessage> msgList, bool useHighPriority) {

            if (msgList.Count < 1)
                return;

            int numberOfPackets = msgList.Count / MAX_NETWORK_MESSAGES_PER_PACKET + 1;

            for (int i = 0; i < numberOfPackets; i++) {

                PacketNetworkMessage msgPacket;

                if (useHighPriority)
                    msgPacket = new PacketNetworkMessage(Packet.PRIORITY_HIGH);
                else
                    msgPacket = new PacketNetworkMessage(Packet.PRIORITY_LOW);

                int messageCount = (msgList.Count < MAX_NETWORK_MESSAGES_PER_PACKET) ? msgList.Count : MAX_NETWORK_MESSAGES_PER_PACKET;

                List<NetworkMessage> toSend = msgList.GetRange(0, messageCount);
                msgList.RemoveRange(0, messageCount);//This may fuck things up

                foreach (NetworkMessage msg in toSend) {

                    msgPacket.networkMessages.Add(msg);
                }

                if (msgPacket.networkMessages.Count > 0)
                    SendPacket(msgPacket);
            }
        }

        private void SendTargeted(List<NetworkMessage> msgList, NetworkConnection conn, bool useHighPriority) {

            if (msgList.Count < 1)
                return;

            int numberOfPackets = msgList.Count / MAX_NETWORK_MESSAGES_PER_PACKET + 1;

            for (int i = 0; i < numberOfPackets; i++) {

                PacketNetworkMessage msgPacket;

                if (useHighPriority)
                    msgPacket = new PacketNetworkMessage(Packet.PRIORITY_HIGH);
                else
                    msgPacket = new PacketNetworkMessage(Packet.PRIORITY_LOW);

                int messageCount = (msgList.Count < MAX_NETWORK_MESSAGES_PER_PACKET) ? msgList.Count : MAX_NETWORK_MESSAGES_PER_PACKET;

                List<NetworkMessage> toSend = msgList.GetRange(0, messageCount);
                msgList.RemoveRange(0, messageCount);//This may fuck things up

                foreach (NetworkMessage msg in toSend) {

                    msgPacket.networkMessages.Add(msg);
                }

                if (msgPacket.networkMessages.Count > 0)
                    SendPacket(msgPacket, conn);
            }
        }

        public async void SendPacket(Packet packet) {

            CachePacket(packet, null);

            byte[] sendData = Converters.SerializePacket(packet);

            for (int i = 0; i < clientConnections.Length; i++) {

                if (clientConnections[i] == null)
                    continue;

                if (sendData.Length > 1500) {
                }

                try {

                    await udpClient.SendAsync(sendData, sendData.Length, clientConnections[i].endPoint);
                    monitor.AddPacketsOutbound(1);
                    monitor.AddBytesOutbound(sendData.Length);
                }
                catch (SocketException se) {
                }
            }
        }

        public async void SendPacket(Packet packet, NetworkConnection connection) {

			StampPacket(packet);
            CachePacket(packet, connection);

            byte[] sendData = Converters.SerializePacket(packet);

            if (sendData.Length > 1500) {
            }

            try {

                monitor.AddPacketsOutbound(1);
                monitor.AddBytesOutbound(sendData.Length);
                await udpClient.SendAsync(sendData, sendData.Length, connection.endPoint);
            }
            catch (SocketException se) {

                Debug.LogError("Unable to send packet: " + se.Message);
                Debug.LogError("Client info: " + connection.endPoint.ToString());
            }

        }

        private void CachePacket(Packet packet, NetworkConnection connection) {

            if (packet.priority == Packet.PRIORITY_HIGH) {

				sentPacketRecorder.RecordSentPacket(packet, connection);
            }
        }

        public int ClientCount() {

            int count = 0;

            for (int i = 0; i < clientConnections.Length; i++) {

                if (clientConnections[i] != null)
                    count++;
            }

            return count;
        }
    }
}
