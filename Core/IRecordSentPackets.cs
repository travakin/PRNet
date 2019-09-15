using PRNet.NetworkEntities;
using PRNet.Packets;
using System.Collections.Generic;

namespace PRNet.Core {
	public interface IRecordSentPackets {

		void RecordSentPacket(Packet packet);
		void RecordSentPacket(Packet packet, NetworkConnection conn);
		void Acknowledge(int packetId);
		void Acknowledge(int packetId, NetworkConnection conn);
		void ClearPacketsForConnection(NetworkConnection conn);
		void ClearAllSentPacketRecords();
		int SentPacketRecordsCount();
		List<PacketConnection> RetrieveExpiredSentPackets();
	}
}