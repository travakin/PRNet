using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OPS.Serialization.Attributes;

namespace PRNet.NetworkEntities {

    [Serializable]
    [SerializeAbleClass]
    [ClassInheritance(typeof(TestArgs), 0)]
    [ClassInheritance(typeof(PlayerSpawnArgs), 1)]
    public class NetworkSpawnArgs {

        [Serializable]
        [SerializeAbleClass]
        public class TestArgs : NetworkSpawnArgs {

            [SerializeAbleField(0)]
            public string value;
        }

        [Serializable]
        [SerializeAbleClass]
        public class PlayerSpawnArgs : NetworkSpawnArgs {

            [SerializeAbleField(0)]
            public string primaryWeapon;
            [SerializeAbleField(1)]
            public string secondaryWeapon;
            [SerializeAbleField(2)]
            public string primarySight;
            [SerializeAbleField(3)]
            public string primaryBarrel;
            [SerializeAbleField(4)]
            public string primaryMagazine;
            [SerializeAbleField(5)]
            public string selectedCharacter;
        }
    }
}