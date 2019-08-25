using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OPS.Serialization.Attributes;
using PRNet.Core;

namespace PRNet.Requests {

    [Serializable]
    [SerializeAbleClass]
    public class RpcArgs {

        [SerializeAbleField(0)]
        public NetworkSyncItem[] values;
        [SerializeAbleField(1)]
        public string[] types;

        public RpcArgs() {
        }

        public RpcArgs(params NetworkSyncItem[] args) {

            this.values = args;
            this.types = args.Select(arg => arg.GetType().ToString()).ToArray();
        }
    }
}