using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PRNet.Core;
using PRNet.NetworkEntities;
using PRNet.Requests;

public class NetworkManager : MonoBehaviour {

    public GameObject startServerButton;
    public GameObject startClientButton;

    public GameObject clientUI;

    public EntityDictionaryEntry[] spawnableObjects;
    public NetworkEntity cubie;

    private PRClient client;
    private PRServer server;

    private const int MAX_ATTEMPTS = 40;
    private const float ATTEMPT_DURATION = 0.5f;

    private void Start() {

        //StartCoroutine(DestroyObjects(20.0f));
        Application.runInBackground = true;
    }

    private void Update() {

        if (ClientStage.active)
            ClientStage.Tick();

        if (ServerStage.active)
            ServerStage.Tick();
    }

    public void StartServer() {

        ServerStage.StartServer(47777);
        ServerStage.AddEntityDefinitions(spawnableObjects);

        ServerStage.RegisterMessageEvent(NetworkMessage.SpawnPlayer, SpawnPlayer);

        ServerStage.Ready();

        ServerConsoleCommands.AddConsoleCommand("Try", LoopTry);

        //NetworkEntity cubie1 = Instantiate(cubie, Vector3.right * 3, Quaternion.identity);
        //ServerStage.ServerSpawn(cubie1);
    }

    private void LoopTry(string[] args) {

        StartCoroutine(LoopTry());
    }

    private IEnumerator LoopTry() {

        ServerViewMain.instance.LogMessage("Starting debug loop, hold on to something!");

        int cnt = 0;

        while (true) {

            yield return new WaitForSeconds(1f);
            ServerViewMain.instance.LogMessage("Looping try " + cnt);
            cnt++;
        }
    }

    public void ResetServer() {

        ServerStage.ResetServer();
    }

    public void StartClient() {

        ClientStage.ConnectClient("localhost", 47777);
        ClientStage.AddEntityDefinitions(spawnableObjects);
        StartCoroutine(ReadyClient(1.0f));
    }

    private IEnumerator ReadyClient(float delay) {

        yield return new WaitForSeconds(delay);
        ClientStage.Ready();
    }

    public void DisconnectClient() {

        if (!ClientStage.active)
            return;

        ClientStage.DisconnectClient();

        startServerButton.SetActive(true);
        startClientButton.SetActive(true);
        clientUI.SetActive(false);
    }

    private void OnDestroy() {

        if(ClientStage.active)
            ClientStage.DisconnectClient();

        if (ServerStage.active)
            ServerStage.Disconnect();
    }

    public void RequestPlayerSpawn() {

        if (!ClientStage.active)
            return;

        //Debug.Log("Sending spawn message");

        NetworkMessage.SpawnPlayerMessage msg = new NetworkMessage.SpawnPlayerMessage();
        ClientStage.SendNetworkMessage(msg);

        //Debug.Log("Done sending spawn message");
    }

    private void SpawnPlayer(NetworkMessage msg) {

        //Debug.Log("Received spawn message.");

        NetworkEntity cubie1 = Instantiate(cubie, Vector3.right * 3, Quaternion.identity);
        NetworkSpawnArgs.TestArgs args = new NetworkSpawnArgs.TestArgs();
        args.value = "cubie1";
        ServerStage.ServerSpawn(cubie1, msg.senderConnection, args);
    }
}
