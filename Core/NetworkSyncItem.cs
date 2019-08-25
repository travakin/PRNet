using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PRNet.Core;
using PRNet.Utils;
using OPS.Serialization.Attributes;

namespace PRNet.Core {

    [SerializeAbleClass]
    [ClassInheritance(typeof(NetworkSyncPass), 0)]
    [ClassInheritance(typeof(NetworkSyncInt), 1)]
    [ClassInheritance(typeof(NetworkSyncFloat), 2)]
    [ClassInheritance(typeof(NetworkSyncBool), 3)]
    [ClassInheritance(typeof(NetworkSyncVector3), 4)]
    [ClassInheritance(typeof(NetworkSyncQuaternion), 5)]
    [ClassInheritance(typeof(NetworkSyncUInt), 6)]
    [ClassInheritance(typeof(NetworkSyncString), 7)]
    [ClassInheritance(typeof(NetworkSyncStringList), 8)]
    [ClassInheritance(typeof(NetworkSyncVarValue), 9)]
    public abstract class NetworkSyncItem {

        public abstract bool Equals(NetworkSyncItem obj);
        public abstract object GetValue();
    }

    [SerializeAbleClass]
    public class NetworkSyncPass : NetworkSyncItem {

        public NetworkSyncPass() {
        }

        public override bool Equals(NetworkSyncItem obj) {

            return obj.GetType() == typeof(NetworkSyncPass);
        }

        public override object GetValue() {

            return this;
        }
    }

    [SerializeAbleClass]
    public class NetworkSyncInt : NetworkSyncItem {

        [SerializeAbleField(0)]
        public int Value;

        public NetworkSyncInt() {
        }

        public NetworkSyncInt(int value) {
            this.Value = value;
        }

        public override bool Equals(NetworkSyncItem obj) {

            if (obj.GetType() != this.GetType())
                return false;

            return this.Value == ((NetworkSyncInt)obj).Value;
        }

        public override object GetValue() {

            return Value;
        }
    }

    [SerializeAbleClass]
    public class NetworkSyncFloat : NetworkSyncItem {

        [SerializeAbleField(0)]
        public float Value;

        public NetworkSyncFloat() {
        }

        public NetworkSyncFloat(float value) {
            this.Value = value;
        }

        public override bool Equals(NetworkSyncItem obj) {

            if (obj.GetType() != this.GetType())
                return false;

            return this.Value == ((NetworkSyncFloat)obj).Value;
        }

        public override object GetValue() {

            return Value;
        }
    }

    [SerializeAbleClass]
    public class NetworkSyncBool : NetworkSyncItem {

        [SerializeAbleField(0)]
        public bool Value;

        public NetworkSyncBool() {
        }

        public NetworkSyncBool(bool value) {
            this.Value = value;
        }

        public override bool Equals(NetworkSyncItem obj) {

            if (obj.GetType() != this.GetType())
                return false;

            return this.Value == ((NetworkSyncBool)obj).Value;
        }

        public override object GetValue() {

            return Value;
        }
    }

    [SerializeAbleClass]
    public class NetworkSyncVector3 : NetworkSyncItem {

        [SerializeAbleField(0)]
        private float x;
        [SerializeAbleField(1)]
        private float y;
        [SerializeAbleField(2)]
        private float z;

        public Vector3 Value {

            get {
                return new Vector3(x, y, z);
            }
            set {

                this.x = value.x;
                this.y = value.y;
                this.z = value.z;
            }
        }

        public NetworkSyncVector3() { }

        public NetworkSyncVector3(Vector3 vector) {

            this.x = vector.x;
            this.y = vector.y;
            this.z = vector.z;
        }

        public override bool Equals(NetworkSyncItem obj) {

            if (obj.GetType() != this.GetType())
                return false;

            return this.Value == ((NetworkSyncVector3)obj).Value;
        }

        public override object GetValue() {

            return Value;
        }
    }

    [SerializeAbleClass]
    public class NetworkSyncQuaternion : NetworkSyncItem {

        [SerializeAbleField(0)]
        private float x;
        [SerializeAbleField(1)]
        private float y;
        [SerializeAbleField(2)]
        private float z;
        [SerializeAbleField(3)]
        private float w;

        public Quaternion Value {

            get {
                return new Quaternion(x, y, z, w);
            }
            set {
                this.x = value.x;
                this.y = value.y;
                this.z = value.z;
                this.w = value.w;
            }

        }

        public NetworkSyncQuaternion() { }

        public NetworkSyncQuaternion(Quaternion rotation) {

            this.x = rotation.x;
            this.y = rotation.y;
            this.z = rotation.z;
            this.w = rotation.w;
        }

        public override bool Equals(NetworkSyncItem obj) {

            if (obj.GetType() != this.GetType())
                return false;

            return this.Value == ((NetworkSyncQuaternion)obj).Value;
        }

        public override object GetValue() {

            return Value;
        }
    }

    [SerializeAbleClass]
    public class NetworkSyncUInt : NetworkSyncItem {

        [SerializeAbleField(0)]
        public uint Value;

        public NetworkSyncUInt() {
        }

        public NetworkSyncUInt(uint value) {
            this.Value = value;
        }

        public override bool Equals(NetworkSyncItem obj) {

            if (obj.GetType() != this.GetType())
                return false;

            return this.Value == ((NetworkSyncUInt)obj).Value;
        }

        public override object GetValue() {

            return Value;
        }
    }

    [SerializeAbleClass]
    public class NetworkSyncString : NetworkSyncItem {

        [SerializeAbleField(0)]
        public string Value;

        public NetworkSyncString() { }

        public NetworkSyncString(string value) {

            this.Value = value;
        }

        public override bool Equals(NetworkSyncItem obj) {

            if (obj.GetType() != this.GetType())
                return false;

            return this.Value == ((NetworkSyncString)obj).Value;
        }

        public override object GetValue() {

            return Value;
        }
    }

    [SerializeAbleClass]
    public class NetworkSyncStringList : NetworkSyncItem {

        [SerializeAbleField(0)]
        public List<string> Value;

        public NetworkSyncStringList() { }

        public NetworkSyncStringList(List<string> value) {

            this.Value = value;
        }

        public override bool Equals(NetworkSyncItem obj) {

            if (obj.GetType() != this.GetType())
                return false;

            List<string> otherList = ((NetworkSyncStringList)obj).Value;

            if (Value.Count != otherList.Count)
                return false;

            bool allEqual = true;

            for (int i = 0; i < Value.Count; i++) {

                allEqual = allEqual && (Value[i].Equals(otherList[i]));
            }

            return allEqual;
        }

        public override object GetValue() {

            return Value;
        }
    }

    [SerializeAbleClass]
    public class NetworkSyncVarValue : NetworkSyncItem {

        [SerializeAbleField(0)]
        public List<string> Names;

        [SerializeAbleField(1)]
        public List<NetworkSyncItem> Value;

        public NetworkSyncVarValue() { }

        public NetworkSyncVarValue(List<string> names, List<NetworkSyncItem> value) {

            this.Value = value;
            this.Names = names;
        }

        public override bool Equals(NetworkSyncItem obj) {

            if (obj.GetType() != this.GetType())
                return false;

            NetworkSyncVarValue other = (NetworkSyncVarValue)obj;

            List<NetworkSyncItem> otherList = other.Value;

            if (Value.Count != otherList.Count)
                return false;

            bool allEqual = true;

            for (int i = 0; i < Value.Count; i++) {

                allEqual = allEqual && (Value[i].Equals(otherList[i]));
            }

            return allEqual;
        }

        public override object GetValue() {

            return Value;
        }
    }
}