using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using PRNet.Packets;
using OPS.Serialization.IO;

namespace PRNet.Utils {

    public static class Converters {

        public static byte[] SerializePacket(Packet packet) {

            //BinaryFormatter formatter = new BinaryFormatter();
            //MemoryStream memoryStream = new MemoryStream();

            //formatter.Serialize(memoryStream, packet);

            byte[] sendData = Serializer.Serialize(packet, true);

            //Debug.Log("Sending packet of size " + sendData.Length + " bytes");

            return sendData;
        }

        public static Packet DeserializePacket(byte[] bytes) {

            //Debug.Log("Receiving packet of size " + bytes.Length + " bytes");

            //BinaryFormatter binaryFormatter = new BinaryFormatter();
            //MemoryStream memoryStream = new MemoryStream();

            //memoryStream.Write(bytes, 0, bytes.Length);
            //memoryStream.Seek(0, SeekOrigin.Begin);

            //Packet packet = (Packet)binaryFormatter.Deserialize(memoryStream);

            Packet packet = Serializer.DeSerialize<Packet>(bytes, true);

            //Debug.Log(packet.type);

            return packet;
        }
    }
}