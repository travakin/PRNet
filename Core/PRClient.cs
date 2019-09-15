using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using PRNet.Meta;
using PRNet.Packets;
using PRNet.Requests;
using PRNet.Utils;
using UnityEngine;

namespace PRNet.Core {

	public class PRClient : PRNetCore {

		private const int MAX_NETWORK_MESSAGES_PER_PACKET = 120;
		private const int SIO_UDP_CONNRESET = -1744830452;
		private const int HIGH_PRIORITY_RECORD_LIFESPAN = 3;
		private const int HIGH_PRIORITY_WAIT_MS = 200;
		private const int TICKRATE_MS = 25;

		public enum States { Offline, Pending, ChallengeResponse, Connected };

		public int state = (int)States.Offline;
		private int myPort;
		private long clientId;

		private DateTime lastPacketCountTime = DateTime.Now;

		private NetworkObjectsManager objectsManager;

		private Dictionary<int, Action<Packet>> parsers = new Dictionary<int, Action<Packet>>();

		public PRClient(NetworkObjectsManager manager, IRecordSentPackets sentPacketRecorder, IRecordReceivedPackets receivedPacketRecorder, INetworkMonitor monitor) {

			this.objectsManager = manager;
			this.sentPacketRecorder = sentPacketRecorder;
			this.receivedPacketRecorder = receivedPacketRecorder;
			this.monitor = monitor;

			IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
			IPEndPoint[] portInfo = properties.GetActiveUdpListeners();
			List<int> ports = portInfo.Select(info => info.Port).ToList();

			myPort = Identification.GetUniqueIdentifierFromList(ports, 5000, 50000);

			udpClient = new UdpClient(myPort);

			udpClient.Client.IOControl(
				(IOControlCode)SIO_UDP_CONNRESET,
				new byte[] { 0, 0, 0, 0 },
				null
			);

			udpClient.BeginReceive(new AsyncCallback(UdpClientData), udpClient);

			Debug.Log("Welcome to my client!");
			Debug.Log("You have been assigned port # " + myPort);

			SetupParsers();
			InitializeThreads();
		}

		private void SetupParsers() {

			parsers.Add(Packet.PACKET_ACK, ParseAck);
			parsers.Add(Packet.PACKET_CHALLENGEREQUEST, ParseChallengeRequestPacket);
			parsers.Add(Packet.PACKET_CONNECTIONCONFIRM, ParseConnectionConfirmPacket);
			parsers.Add(Packet.PACKET_SPAWNCOMMAND, ClientStage.ParseSpawnCommand);
			parsers.Add(Packet.PACKET_DESTROYCOMMAND, ClientStage.ParseDestroyCommand);
			parsers.Add(Packet.PACKET_NETWORKMESSAGE, ClientStage.ParseNetworkMessages);
		}

		private void InitializeThreads() {

			Thread changeDetectionThread = new Thread(new ThreadStart(ChangeDetectionThread));
			changeDetectionThread.Start();

			Thread clientTickThread = new Thread(new ThreadStart(ClientTick));
			clientTickThread.Start();
		}

		private void ClientTick() {

			while (!stopThreads) {

				SendNetworkMessages();

				System.Threading.Thread.Sleep(TICKRATE_MS);
			}
		}

		public void DisconnectClient() {

			Debug.Log("Disconnecting client");

			NetworkEventPayload.ClientDisonnectPayload payload = new NetworkEventPayload.ClientDisonnectPayload();

			ClientStage.EnqueueNetworkEvent(ClientStage.EVENT_CLIENTDISCONNECTED, payload);

			PacketClientDisconnect disconnectMessage = new PacketClientDisconnect();

			stopThreads = true;
			state = (int)States.Offline;
			udpClient.Close();
		}


		private void UdpClientData(IAsyncResult result) {

			UdpClient socket = result.AsyncState as UdpClient;

			socket.BeginReceive(new AsyncCallback(UdpClientData), socket);

			IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);

			Packet readPacket = GetPacketFromSocket(socket, ref clientEndPoint, result);

			if (!receivedPacketRecorder.HasReceivedPacket(readPacket))
				ProcessPacket(readPacket);

			if (readPacket.priority == Packet.PRIORITY_HIGH) {

				RecordHighPriorityPacket(readPacket);
				AcknowledgePacket(readPacket);
			}
		}

		public void ProcessPacket(Packet readPacket)
			=> parsers[readPacket.type](readPacket);

		private void AcknowledgePacket(Packet readPacket) {

			PacketPriorityAck ackPacket = new PacketPriorityAck();
			ackPacket.responseId = readPacket.packetId;

			SendPacket(ackPacket);
		}

		private void RecordHighPriorityPacket(Packet readPacket)
			=> receivedPacketRecorder.RecordReceivedPacket(readPacket);

		private void ChangeDetectionThread() {

			while (!stopThreads) {

				List<PacketConnection> expiredSentPackets = sentPacketRecorder.RetrieveExpiredSentPackets();

				int cnt = 0;
				foreach (PacketConnection entry in expiredSentPackets) {

					cnt++;

					SendPacket(entry.packet);
				}

				receivedPacketRecorder.ClearExpiredReceivedPacketRecords();

				if ((DateTime.Now - lastPacketCountTime).TotalSeconds > 1) {

					//Debug.Log("Current inbound packets: " + monitor.GetPacketsInbound() + " packets for a total of " + monitor.GetBytesInbound() + " bytes");
					//Debug.Log("Current outbound packets: " + monitor.GetPacketsOutbound() + " packets for a total of " + monitor.GetBytesOutbound() + " bytes");

					monitor.ClearData();
					lastPacketCountTime = DateTime.Now;
				}
			}
		}

		public void RequestConnection(string serverIp, int serverPort) {

			state = (int)States.Pending;

			udpClient.Connect(serverIp, serverPort);

			System.Random random = new System.Random();

			clientId = (random.Next() << 32) | (random.Next());

			PacketConnectionRequest request = new PacketConnectionRequest();
			request.clientId = clientId;

			SendPacket(request);
		}

		private void ParseChallengeRequestPacket(Packet packet) {

			PacketChallengeRequest parsePacket = (PacketChallengeRequest)packet;

			PacketChallengeResponse response = new PacketChallengeResponse();
			response.clientId = clientId;
			response.username = ClientData.Username;
			response.playerName = ClientData.PlayerName;
			response.playerAccountBalance = ClientData.AccountBalance;

			state = (int)States.ChallengeResponse;

			SendPacket(response);
		}

		private void ParseConnectionConfirmPacket(Packet packet) {

			PacketConnectionConfirm confirmation = (PacketConnectionConfirm)packet;

			Debug.Log("Successfully connected to server!");

			state = (int)States.Connected;

			NetworkEventPayload.ClientConnectPayload payload = new NetworkEventPayload.ClientConnectPayload();
			payload.level = confirmation.levelName;

			ClientStage.EnqueueNetworkEvent(ClientStage.EVENT_CLIENTCONNECTED, payload);

			ClientStage.clientId = confirmation.clientId;
		}

		public void RequestUpdate(Packet packet) {

			if (packet.type == Packet.PACKET_STATEREQUEST)
				SendPacket(packet);
		}

		public void GetStateFromServer() {

			PacketClientReady clientReady = new PacketClientReady();
			SendPacket(clientReady);
		}

		private void SendNetworkMessages() {

			List<NetworkMessage> messages = objectsManager.pendingMessagesOutbound.GetRange(0, objectsManager.pendingMessagesOutbound.Count);
			objectsManager.pendingMessagesOutbound.RemoveRange(0, objectsManager.pendingMessagesOutbound.Count);

			List<NetworkMessage> messagesHP = objectsManager.pendingMessagesOutboundHP.GetRange(0, objectsManager.pendingMessagesOutboundHP.Count);
			objectsManager.pendingMessagesOutboundHP.RemoveRange(0, objectsManager.pendingMessagesOutboundHP.Count);

			SendMessages(messages, false);
			SendMessages(messagesHP, true);
		}

		private void SendMessages(List<NetworkMessage> msgList, bool useHighPriority) {

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

		private async void SendPacket(Packet packet) {

			StampPacket(packet);
			CachePacket(packet);

			byte[] sendData = Converters.SerializePacket(packet);

			try {

				await udpClient.SendAsync(sendData, sendData.Length);
				monitor.AddPacketsOutbound(1);
				monitor.AddBytesOutbound(sendData.Length);
			}
			catch (SocketException se) {
			}
		}

		private void CachePacket(Packet packet) {

			if (packet.priority == Packet.PRIORITY_HIGH) {

				sentPacketRecorder.RecordSentPacket(packet);
			}
		}
	}
}