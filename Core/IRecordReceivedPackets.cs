using PRNet.NetworkEntities;
using PRNet.Packets;

namespace PRNet.Core {
	public interface IRecordReceivedPackets {

		void RecordReceivedPacket(Packet packet);
		void RecordReceivedPacket(Packet packet, NetworkConnection conn);
		void ClearExpiredReceivedPacketRecords();
		bool HasReceivedPacket(Packet packet);
		bool HasReceivedPacket(Packet packet, NetworkConnection conn);
	}
}