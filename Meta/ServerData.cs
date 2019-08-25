using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PRNet.Meta {

    public static class ServerData {

        public static string PublicIP { get; set; }
        public static int Port { get; set; }
        public static string ServerName { get; set; }
        public static string GameMode { get; set; }
        public static string CurrentLevelName { get; set; }
        public static int PlayerCount { get; set; }
        public static int MaxPlayers { get; set; }
    }
}
