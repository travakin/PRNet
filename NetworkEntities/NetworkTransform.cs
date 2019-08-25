using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PRNet.Core;
using PRNet.NetworkEntities;
using PRNet.Requests;
using PRNet.Utils;

public class NetworkTransform : PRNetworkBehaviour {

    public Transform childTransform;
    public float updatesPerSecond = 16;

    private float updateTimeSeconds;

    private Vector3 previousPosition;
    private Quaternion previousRotation;
    private Vector3 previousChildPosition;
    private Quaternion previousChildRotation;

    private void Start() {

        updateTimeSeconds = 1 / updatesPerSecond;

        StartCoroutine(UpdatePosition());
    }

    private IEnumerator UpdatePosition() {

        while (true) {

            if (transform.position != previousPosition || transform.rotation != previousRotation || childTransform.localPosition != previousChildPosition || childTransform.rotation != previousChildRotation) {

                InvokePositionUpdate();
                previousPosition = transform.position;
                previousRotation = transform.rotation;
                previousChildPosition = childTransform.localPosition;
                previousChildRotation = childTransform.rotation;
            }

            yield return new WaitForSeconds(updateTimeSeconds);
        }
    }

    public void InvokePositionUpdate() {

        if (ClientStage.active && !myNetworkEntity.IsLocalObject())
            return;

        NetworkMessage.UpdateTransformMessage transformMsg = new NetworkMessage.UpdateTransformMessage(myNetworkEntity.instanceId, transform.position, transform.rotation, childTransform.localPosition, childTransform.rotation);

        if (ClientStage.active)
            ClientStage.SendNetworkMessage(transformMsg);

        if (ServerStage.active)
            ServerStage.SendNetworkMessage(transformMsg);
    }

    public void ReceiveTransformUpdate(Vector3 position, Quaternion rotation, Vector3 childPosition, Quaternion childRotation) {

        if (myNetworkEntity.IsLocalObject())
            return;

        transform.position = position;
        transform.rotation = rotation;

        childTransform.localPosition = childPosition;
        childTransform.rotation = childRotation;
    }
}