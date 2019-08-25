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
    public float distanceBuffer = 3f;

    private float updateTimeSeconds;

    private Vector3 previousPosition;
    private Quaternion previousRotation;
    private Vector3 previousChildPosition;
    private Quaternion previousChildRotation;
    private Vector3 _velocity;
    private Vector3 positionLastFrame;

    private float timeSinceLastClientUpdate = 0;
    private float clientCurrentSpeed;
    private Vector3 lastClientUpdatePosition;
    private Vector3 clientNextPoint;

    private void Start() {

        updateTimeSeconds = 1 / updatesPerSecond;

        positionLastFrame = transform.position;

        StartCoroutine(UpdatePosition());
    }

    private void Update() {

        UpdateCurrentVelocity();
        ClientSmoothTransform();
        TimeLastClientUpdate();
    }

    private void UpdateCurrentVelocity() {

        if (!myNetworkEntity.IsLocalObject())
            return;

        _velocity = (transform.position - positionLastFrame) / Time.deltaTime;

        Debug.Log(_velocity);

        positionLastFrame = transform.position;
    }

    private void ClientSmoothTransform() {

        if (myNetworkEntity.IsLocalObject())
            return;

        transform.position = Vector3.MoveTowards(transform.position, clientNextPoint, clientCurrentSpeed * Time.deltaTime);
    }

    private void TimeLastClientUpdate() {

        if (ServerStage.active)
            return;

        timeSinceLastClientUpdate += Time.deltaTime;
    }

    private IEnumerator UpdatePosition() {

        while (true) {

            if (transform.position != previousPosition 
                || transform.rotation != previousRotation 
                || childTransform.localPosition != previousChildPosition 
                || childTransform.rotation != previousChildRotation) {

                InvokePositionUpdate();
            }

            yield return new WaitForSeconds(updateTimeSeconds);
        }
    }

    public void InvokePositionUpdate() {

        if (ClientStage.active && !myNetworkEntity.IsLocalObject())
            return;

        NetworkMessage.UpdateTransformMessage transformMsg = new NetworkMessage.UpdateTransformMessage(myNetworkEntity.instanceId, transform.position, transform.rotation, childTransform.localPosition, childTransform.rotation, _velocity);

        if (ClientStage.active)
            ClientStage.SendNetworkMessage(transformMsg);

        if (ServerStage.active)
            ServerStage.SendNetworkMessage(transformMsg);

        previousPosition = transform.position;
        previousRotation = transform.rotation;
        previousChildPosition = childTransform.localPosition;
        previousChildRotation = childTransform.rotation;
    }

    public void ReceiveTransformUpdate(Vector3 position, Quaternion rotation, Vector3 childPosition, Quaternion childRotation, Vector3 velocity) {

        if (myNetworkEntity.IsLocalObject())
            return;

        if (ServerStage.active)
            ReceiveForServer(position, rotation, childPosition, childRotation, velocity);

        if (ClientStage.active)
            ReceiveForClient(position, rotation, childPosition, childRotation, velocity);
    }

    private void ReceiveForClient(Vector3 position, Quaternion rotation, Vector3 childPosition, Quaternion childRotation, Vector3 velocity) {

        if((transform.position - position).magnitude > distanceBuffer)
            transform.position = position;

        transform.rotation = rotation;

        childTransform.localPosition = childPosition;
        childTransform.rotation = childRotation;

        timeSinceLastClientUpdate = 0;
        clientCurrentSpeed = velocity.magnitude;
        lastClientUpdatePosition = position;
        clientNextPoint = position + (velocity * updateTimeSeconds);
    }

    private void ReceiveForServer(Vector3 position, Quaternion rotation, Vector3 childPosition, Quaternion childRotation, Vector3 velocity) {

        transform.position = position;
        transform.rotation = rotation;
        _velocity = velocity;

        childTransform.localPosition = childPosition;
        childTransform.rotation = childRotation;
    }
}