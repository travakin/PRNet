using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using PRNet.Core;
using PRNet.NetworkEntities;
using PRNet.Packets;
using PRNet.Utils;
using OPS.Serialization.Attributes;
using static PRNet.Core.NetworkSyncItem;

namespace PRNet.Requests {

    [Serializable]
    [SerializeAbleClass]
    [ClassInheritance(typeof(ChangeColorMessage), 0)]
    [ClassInheritance(typeof(SpawnPlayerMessage), 1)]
    [ClassInheritance(typeof(RpcInvokeMessage), 2)]
    [ClassInheritance(typeof(UpdateTransformMessage), 3)]
    [ClassInheritance(typeof(SyncVarUpdateMessage), 4)]
    [ClassInheritance(typeof(SpawnMessage), 5)]
    [ClassInheritance(typeof(UserPlaceBetMessage), 6)]
    [ClassInheritance(typeof(SpawnUpdateMessage), 7)]
    [ClassInheritance(typeof(LoadLevelMessage), 8)]
    [ClassInheritance(typeof(TextCommunicationMessage), 9)]
    [ClassInheritance(typeof(KillfeedEntryMessage), 10)]
    [ClassInheritance(typeof(DamageMessage), 11)]
    [ClassInheritance(typeof(ScoreboardEntryMessage), 12)]
    [ClassInheritance(typeof(SquadEndgameMessage), 13)]
    [ClassInheritance(typeof(DroneSpawnMessage), 14)]
    [ClassInheritance(typeof(TimerSyncMessage), 15)]
    public class NetworkMessage {

        public static short ChangeColor = 0;
        public static short SpawnPlayer = 1;
        public static short RpcInvoke = 2;
        public static short UpdateTransform = 3;
        public static short SyncVarUpdate = 4;
        public static short Spawn = 5;
        public static short PlaceBet = 6;
        public static short SpawnUpdate = 7;
        public static short LoadLevel = 8;
        public static short TextCommunication = 9;
        public static short KillfeedEntry = 10;
        public static short Damage = 11;
        public static short ScoreboardEntry = 12;
        public static short SquadEndgame = 13;
        public static short DroneSpawn = 14;
        public static short TimerSync = 15;
        public static short Max = 15;

        [SerializeAbleField(0)]
        public short type;
        public NetworkConnection senderConnection;

        [Serializable]
        [SerializeAbleClass]
        public class ChangeColorMessage : NetworkMessage {

            public ChangeColorMessage() {

                this.type = ChangeColor;
            }
        }

        [Serializable]
        [SerializeAbleClass]
        public class SpawnPlayerMessage : NetworkMessage {

            public SpawnPlayerMessage() {

                this.type = SpawnPlayer;
            }
        }

        [Serializable]
        [SerializeAbleClass]
        public class RpcInvokeMessage : NetworkMessage {

            [SerializeAbleField(0)]
            public NetworkInstanceId objectId;
            [SerializeAbleField(1)]
            public string functionName;
            [SerializeAbleField(2)]
            public RpcArgs arguments;

            public RpcInvokeMessage() {

                this.type = RpcInvoke;
            }

            public RpcInvokeMessage(NetworkInstanceId id, int priority, string funcName, RpcArgs args) {

                this.type = RpcInvoke;

                this.objectId = id;
                this.functionName = funcName;
                this.arguments = args;
            }
        }

        [Serializable]
        [SerializeAbleClass]
        public class UpdateTransformMessage : NetworkMessage {

            [SerializeAbleField(0)]
            public NetworkInstanceId objectId;
            [SerializeAbleField(1)]
            public NetworkSyncVector3 relayPosition;
            [SerializeAbleField(2)]
            public NetworkSyncQuaternion relayRotation;
            [SerializeAbleField(3)]
            public NetworkSyncVector3 childRelayPosition;
            [SerializeAbleField(4)]
            public NetworkSyncQuaternion childRelayRotation;
            [SerializeAbleField(5)]
            public NetworkSyncVector3 relayVelocity;

            public UpdateTransformMessage() {

                this.type = UpdateTransform;
            }

            public UpdateTransformMessage(NetworkInstanceId id, Vector3 position, Quaternion rotation, Vector3 childPosition, Quaternion childRotation, Vector3 velocity) {

                this.type = UpdateTransform;

                objectId = id;
                relayPosition = position.GetSerializableVector();
                relayRotation = rotation.GetSerializableVector();
                childRelayPosition = childPosition.GetSerializableVector();
                childRelayRotation = childRotation.GetSerializableVector();
                relayVelocity = velocity.GetSerializableVector();
            }
        }

        [Serializable]
        [SerializeAbleClass]
        public class SyncVarUpdateMessage : NetworkMessage {

            [SerializeAbleField(0)]
            public NetworkInstanceId objectId;
            [SerializeAbleField(1)]
            public string name;
            [SerializeAbleField(2)]
            public NetworkSyncItem value;

            public SyncVarUpdateMessage() {

                this.type = SyncVarUpdate;
            }

            public SyncVarUpdateMessage(NetworkInstanceId id, string name, NetworkSyncItem val) {

                this.type = SyncVarUpdate;

                this.objectId = id;
                this.name = name;
                this.value = val;
            }
        }

        [Serializable]
        [SerializeAbleClass]
        public class SpawnMessage : NetworkMessage {

            //Return Error Flag
            //0 is default error
            //1 is unsuccessful spawn
            //2 is invalid transaction
            //3 is all players dead
            //4 is game over

            public SpawnMessage() {

                type = Spawn;
            }

            [SerializeAbleField(0)]
            public short error_flag;
            [SerializeAbleField(1)]
            public NetworkInstanceId playerNetId;
            [SerializeAbleField(2)]
            public string primaryWeapon;
            [SerializeAbleField(3)]
            public string primarySight;
            [SerializeAbleField(4)]
            public string primaryBarrel;
            [SerializeAbleField(5)]
            public string primaryMag;
            [SerializeAbleField(6)]
            public string secondaryWeapon;
        }

        [Serializable]
        [SerializeAbleClass]
        public class UserPlaceBetMessage : NetworkMessage {

            public UserPlaceBetMessage() {

                type = PlaceBet;
            }

            [SerializeAbleField(0)]
            public uint bet;
            [SerializeAbleField(1)]
            public bool success;
        }

        [Serializable]
        [SerializeAbleClass]
        public class SpawnUpdateMessage : NetworkMessage {

            public SpawnUpdateMessage() {

                type = SpawnUpdate;
            }

            [SerializeAbleField(0)]
            public int teamIndex;
            [SerializeAbleField(1)]
            public uint score;
            [SerializeAbleField(2)]
            public bool onJoin;
        }

        [Serializable]
        [SerializeAbleClass]
        public class LoadLevelMessage : NetworkMessage {

            public LoadLevelMessage() {

                type = LoadLevel;
            }

            [SerializeAbleField(0)]
            public string levelName;
            [SerializeAbleField(1)]
            public bool isReload;
        }

        [Serializable]
        [SerializeAbleClass]
        public class TextCommunicationMessage : NetworkMessage {

            public TextCommunicationMessage() {

                type = TextCommunication;
            }

            [SerializeAbleField(0)]
            public string message;
        }

        [Serializable]
        [SerializeAbleClass]
        public class KillfeedEntryMessage : NetworkMessage {

            public KillfeedEntryMessage() {

                type = KillfeedEntry;
            }

            [SerializeAbleField(0)]
            public string bamboozler;
            [SerializeAbleField(1)]
            public string bamboozled;
            [SerializeAbleField(2)]
            public string weaponName;
            [SerializeAbleField(3)]
            public bool teamKill;
        }

        [Serializable]
        [SerializeAbleClass]
        public class DamageMessage : NetworkMessage {

            [SerializeAbleField(0)]
            public DamageInfo damageInfo;

            public DamageMessage() {

                type = Damage;
            }

            public DamageMessage(DamageInfo info) {

                type = Damage;

                this.damageInfo = info;
            }
        }

        [Serializable]
        [SerializeAbleClass]
        public class ScoreboardEntryMessage : NetworkMessage {

            [SerializeAbleField(0)]
            public short clientId;
            [SerializeAbleField(1)]
            public byte teamIndex;
            [SerializeAbleField(2)]
            public short ping;
            [SerializeAbleField(3)]
            public string playerName;
            [SerializeAbleField(4)]
            public short kills;
            [SerializeAbleField(5)]
            public short deaths;
            [SerializeAbleField(6)]
            public short points;

            public ScoreboardEntryMessage() {

                type = ScoreboardEntry;
            }

            public ScoreboardEntryMessage(short clientId, byte teamIdx, short ping, string pName, short kills, short deaths, short points) {

                type = ScoreboardEntry;

                this.clientId = clientId;
                this.teamIndex = teamIdx;
                this.ping = ping;
                this.playerName = pName;
                this.kills = kills;
                this.deaths = deaths;
                this.points = points;
            }
        }

        [Serializable]
        [SerializeAbleClass]
        public class SquadEndgameMessage : NetworkMessage {

            [SerializeAbleField(0)]
            public string message;
            [SerializeAbleField(1)]
            public byte place;
            [SerializeAbleField(2)]
            public uint reward;

            public SquadEndgameMessage() {

                type = SquadEndgame;
            }

            public SquadEndgameMessage(string message, byte place, uint reward) {

                type = SquadEndgame;


                this.message = message;
                this.place = place;
                this.reward = reward;
            }
        }

        [Serializable]
        [SerializeAbleClass]
        public class DroneSpawnMessage : NetworkMessage {

            public DroneSpawnMessage() {

                type = DroneSpawn;
            }
        }

        [Serializable]
        [SerializeAbleClass]
        public class TimerSyncMessage : NetworkMessage {

            [SerializeAbleField(0)]
            public int timeLeft;

            public TimerSyncMessage() {

                type = TimerSync;
            }
        }
    }
}
