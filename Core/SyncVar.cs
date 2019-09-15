using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PRNet.NetworkEntities;

namespace PRNet.Core {

    public class SyncVar {

        public NetworkSyncItem currentItem;
        public NetworkSyncItem prevItem;
        public string VariableName { get; set; }

        private NetworkEntity networkEntity;

        public SyncVar(string name, NetworkSyncItem item) {

            VariableName = name;

            this.Assign(item);
        }

        public void Assign(NetworkSyncItem val) {

            if (currentItem != null)
                prevItem = currentItem;
            else
                prevItem = val;

            currentItem = val;
        }

        public object GetObject() {

            return currentItem;
        }

        public bool Equals(SyncVar other) {

            return currentItem.Equals(other.currentItem);
        }

        public static bool operator ==(SyncVar var1, SyncVar var2) {

            return var1.currentItem.Equals(var2.currentItem);
        }

        public static bool operator !=(SyncVar var1, SyncVar var2) {

            return !var1.currentItem.Equals(var2.currentItem);
        }

        public override string ToString() {

            return currentItem.ToString();
        }

        public void Equalize() {

            prevItem = currentItem;
        }

        public bool IsChanged() {

            return !currentItem.Equals(prevItem);
        }
    }
}