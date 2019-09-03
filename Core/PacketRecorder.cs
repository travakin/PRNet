using PRNet.NetworkEntities;
using PRNet.Packets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PRNet.Core {

	public class PacketRecorder : IRecordReceivedPackets, IRecordSentPackets {

		private List<SentPacketRecord> sentPacketsRecord;
		private List<ReceivedPacketRecord> receivedPacketsRecord;
		private int sentPacketTimeoutMs;
		private int receivedPacketTimeoutMs;

		public PacketRecorder(int sentPacketTimeoutMs, int receivedPacketTimeoutMs) {

			sentPacketsRecord = new List<SentPacketRecord>();
			receivedPacketsRecord = new List<ReceivedPacketRecord>();

			this.sentPacketTimeoutMs = sentPacketTimeoutMs;
			this.receivedPacketTimeoutMs = receivedPacketTimeoutMs;
		}

		public void RecordSentPacket(Packet packet) {

			if (packet == null)
				throw new ArgumentNullException();

			sentPacketsRecord.Add(new SentPacketRecord() { packet = packet, timestamp = DateTime.UtcNow });
		}

		public void RecordSentPacket(Packet packet, NetworkConnection conn) {

			if (packet == null)
				throw new ArgumentNullException();

			sentPacketsRecord.Add(new SentPacketRecord() { packet = packet, timestamp = DateTime.UtcNow, conn = conn });
		}

		public void Acknowledge(int packetId)
			=> sentPacketsRecord.RemoveAll(record => record.packet.packetId == packetId);

		public void ClearPacketsForConnection(NetworkConnection conn)
			=> sentPacketsRecord.RemoveAll(record => record.conn == conn);

		public void RecordReceivedPacket(Packet packet) {

			if (packet == null)
				throw new ArgumentNullException();

			receivedPacketsRecord.Add(new ReceivedPacketRecord { packetId = packet.packetId, timestamp = DateTime.UtcNow });
		}

		public void RecordReceivedPacket(Packet packet, NetworkConnection conn) {

			if (packet == null)
				throw new ArgumentNullException();

			receivedPacketsRecord.Add(new ReceivedPacketRecord { packetId = packet.packetId, conn = conn, timestamp = DateTime.UtcNow });
		}

		public void ClearExpiredReceivedPacketRecords()
			=> receivedPacketsRecord.RemoveAll(record => (DateTime.UtcNow - record.timestamp).TotalMilliseconds > receivedPacketTimeoutMs);

		public bool HasReceivedPacket(Packet packet)
			=> receivedPacketsRecord.Select(record => record.packetId).Contains(packet.packetId);

		public bool HasReceivedPacket(Packet packet, NetworkConnection conn)
			=> receivedPacketsRecord.Where(record => record.packetId == packet.packetId && record.conn == conn).Count() > 0;

		public int SentPacketRecordsCount()
			=> sentPacketsRecord.Count;

		public List<PacketConnection> RetrieveExpiredSentPackets() {

			List<PacketConnection> expiredPackets = GetExpiredSentPackets();
			sentPacketsRecord.RemoveAll(record => IsSentPacketRecordExpired(record));

			return expiredPackets;
		}

		private List<PacketConnection> GetExpiredSentPackets()
			=> sentPacketsRecord.Where(record => IsSentPacketRecordExpired(record)).Select(record => new PacketConnection { conn = record.conn, packet = record.packet }).ToList();

		private bool IsSentPacketRecordExpired(SentPacketRecord record)
			=> (DateTime.UtcNow - record.timestamp).TotalMilliseconds > sentPacketTimeoutMs;
	}

	struct SentPacketRecord {

		public DateTime timestamp;
		public NetworkConnection conn;
		public Packet packet;
	}

	struct ReceivedPacketRecord {

		public DateTime timestamp;
		public NetworkConnection conn;
		public int packetId;
	}

	public struct PacketConnection {

		public NetworkConnection conn;
		public Packet packet;
	}
}
