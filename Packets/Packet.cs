using PRNet.Requests;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using OPS.Serialization.Attributes;

namespace PRNet.Packets {

    [Serializable]
    [SerializeAbleClass]
    [ClassInheritance(typeof(PacketChallengeRequest), 0)]
    [ClassInheritance(typeof(PacketChallengeResponse), 1)]
    [ClassInheritance(typeof(PacketClientDisconnect), 2)]
    [ClassInheritance(typeof(PacketClientReady), 3)]
    [ClassInheritance(typeof(PacketConnectionConfirm), 4)]
    [ClassInheritance(typeof(PacketConnectionRequest), 5)]
    [ClassInheritance(typeof(PacketConnectionResponse), 6)]
    [ClassInheritance(typeof(PacketDestroyCommand), 7)]
    [ClassInheritance(typeof(PacketNetworkMessage), 8)]
    [ClassInheritance(typeof(PacketPriorityAck), 9)]
    [ClassInheritance(typeof(PacketSpawnCommand), 10)]
    [ClassInheritance(typeof(PacketStateRequest), 11)]
    public abstract class Packet {

        public static short PRIORITY_LOW = 0;
        public static short PRIORITY_HIGH = 1;

        public static short PACKET_CONNECTIONREQUEST = 0;
        public static short PACKET_CONNECTIONRESPONSE = 1;
        public static short PACKET_CHALLENGEREQUEST = 2;
        public static short PACKET_CHALLENGERESPONSE = 3;
        public static short PACKET_CONNECTIONCONFIRM = 4;
        public static short PACKET_STATEREQUEST = 5;
        public static short PACKET_SPAWNCOMMAND = 6;
        public static short PACKET_DESTROYCOMMAND = 7;
        public static short PACKET_CLIENTREADY = 8;
        public static short PACKET_NETWORKMESSAGE = 9;
        public static short PACKET_CLIENTDISCONNECTED = 10;
        public static short PACKET_ACK = 11;

        //Header

        [SerializeAbleField(0)]
        public int packetId;
        [SerializeAbleField(1)]
        public short type;
        [SerializeAbleField(2)]
        public short priority;
    }

    [Serializable]
    [SerializeAbleClass]
    class PacketChallengeRequest : Packet {

        public PacketChallengeRequest() {

            type = Packet.PACKET_CHALLENGEREQUEST;
			priority = PRIORITY_HIGH;
        }
    }

    [Serializable]
    [SerializeAbleClass]
    public class PacketChallengeResponse : Packet {

        [SerializeAbleField(0)]
        public long clientId;
        [SerializeAbleField(1)]
        public string username;
        [SerializeAbleField(2)]
        public string playerName;
        [SerializeAbleField(3)]
        public uint playerAccountBalance;

        public PacketChallengeResponse() {

            type = Packet.PACKET_CHALLENGERESPONSE;
        }
    }

    [Serializable]
    [SerializeAbleClass]
    public class PacketClientDisconnect : Packet {

        public PacketClientDisconnect() {

            type = Packet.PACKET_CLIENTDISCONNECTED;
        }
    }

    [Serializable]
    [SerializeAbleClass]
    public class PacketClientReady : Packet {

        public PacketClientReady() {

            type = Packet.PACKET_CLIENTREADY;
            priority = Packet.PRIORITY_HIGH;
        }
    }

    [Serializable]
    [SerializeAbleClass]
    public class PacketConnectionConfirm : Packet {

        [SerializeAbleField(0)]
        public int clientId;
        [SerializeAbleField(1)]
        public string levelName;

        public PacketConnectionConfirm() {

            type = Packet.PACKET_CONNECTIONCONFIRM;
        }

        public PacketConnectionConfirm(int clientId) {

            type = Packet.PACKET_CONNECTIONCONFIRM;

            this.clientId = clientId;
        }
    }

    [Serializable]
    [SerializeAbleClass]
    public class PacketConnectionRequest : Packet {

        [SerializeAbleField(0)]
        public long clientId;

        public PacketConnectionRequest() {

            type = Packet.PACKET_CONNECTIONREQUEST;
            priority = Packet.PRIORITY_HIGH;
        }
    }

    [Serializable]
    [SerializeAbleClass]
    public class PacketConnectionResponse : Packet {

        public PacketConnectionResponse() {

            type = Packet.PACKET_CONNECTIONRESPONSE;
        }
    }

    [Serializable]
    [SerializeAbleClass]
    public class PacketDestroyCommand : Packet {

        [SerializeAbleField(0)]
        public DestroyCommand destroyCommand;

        public PacketDestroyCommand() {

            priority = PRIORITY_HIGH;
            type = Packet.PACKET_DESTROYCOMMAND;
        }

        public PacketDestroyCommand(DestroyCommand command) {

            priority = PRIORITY_HIGH;
            type = Packet.PACKET_DESTROYCOMMAND;

            destroyCommand = command;
        }
    }

    [Serializable]
    [SerializeAbleClass]
    public class PacketNetworkMessage : Packet {

        [SerializeAbleField(0)]
        public List<NetworkMessage> networkMessages = new List<NetworkMessage>();

        public PacketNetworkMessage() {

            //type = Packet.PACKET_NETWORKMESSAGE;
        }

        public PacketNetworkMessage(short priorityType) {

            type = Packet.PACKET_NETWORKMESSAGE;

            if (priorityType != Packet.PRIORITY_LOW && priorityType != PRIORITY_HIGH) {

                Debug.Log("Priority type " + priorityType + " not valid, assigning low priority.");

                priority = PRIORITY_LOW;
                return;
            }

            priority = priorityType;
        }
    }

    [Serializable]
    [SerializeAbleClass]
    public class PacketPriorityAck : Packet {

        [SerializeAbleField(0)]
        public int responseId;

        public PacketPriorityAck() {

            type = Packet.PACKET_ACK;
        }
    }

    [Serializable]
    [SerializeAbleClass]
    public class PacketSpawnCommand : Packet {

        [SerializeAbleField(0)]
        public SpawnCommand spawnCommand;

        public PacketSpawnCommand() {

            priority = PRIORITY_HIGH;
            type = Packet.PACKET_SPAWNCOMMAND;
        }

        public PacketSpawnCommand(SpawnCommand command) {

            priority = PRIORITY_HIGH;
            type = Packet.PACKET_SPAWNCOMMAND;

            spawnCommand = command;
        }
    }

    [Serializable]
    [SerializeAbleClass]
    public class PacketStateRequest : Packet {

        public PacketStateRequest() {

            type = Packet.PACKET_STATEREQUEST;
        }
    }
}
