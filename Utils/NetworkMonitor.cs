using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PRNet.Utils {

    public class NetworkMonitor : INetworkMonitor {

        private int bytesInbound = 0;
        private int bytesOutbound = 0;
        private int packetsInbound = 0;
        private int packetsOutbound = 0;

        public void AddBytesInbound(int bytes) {

            bytesInbound += bytes;
        }

        public void AddBytesOutbound(int bytes) {

            bytesOutbound += bytes;
        }

        public void AddPacketsInbound(int packets) {

            packetsInbound += packets;
        }

        public void AddPacketsOutbound(int packets) {

            packetsOutbound += packets;
        }

        public void ClearData() {

            bytesInbound = 0;
            bytesOutbound = 0;
            packetsInbound = 0;
            packetsOutbound = 0;
        }

        public int GetBytesInbound() {

            return bytesInbound;
        }

        public int GetBytesOutbound() {

            return bytesOutbound;
        }

        public int GetPacketsInbound() {

            return packetsInbound;
        }

        public int GetPacketsOutbound() {

            return packetsOutbound;
        }
    }
}