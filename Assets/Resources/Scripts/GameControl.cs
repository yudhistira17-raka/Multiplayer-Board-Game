using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Net.Sockets;
using System;
using System.Text;

public class GameControl : MonoBehaviour {

    private static GameObject whoWinsTextShadow, player1MoveText, player2MoveText;

    private static GameObject player1, player2;

    public static int diceSideThrown = 0;
    public static int player1StartWaypoint = 0;
    public static int player2StartWaypoint = 0;

    public static bool gameOver = false;

    private int inboundMessageCounter = 10;

    static NetworkScript netObject;

    static int playerIndex = 0;

    bool isSyncing = false;

    // Use this for initialization
    void Start() {
        whoWinsTextShadow = GameObject.Find("WhoWinsText");
        player1MoveText = GameObject.Find("Player1MoveText");
        player2MoveText = GameObject.Find("Player2MoveText");

        player1 = GameObject.Find("Player1");
        player2 = GameObject.Find("Player2");

        player1.GetComponent<FollowThePath>().moveAllowed = false;
        player2.GetComponent<FollowThePath>().moveAllowed = false;

        whoWinsTextShadow.gameObject.SetActive(false);
        //player1MoveText.gameObject.SetActive(true);
        //player2MoveText.gameObject.SetActive(false);

        netObject = GameObject.FindGameObjectWithTag("NetworkObject").GetComponent<NetworkScript>();

        print("Menu static variable: " + Menu.playerIndex);

        //netObject.SendMessageToServer("s10x");

    }

    // Update is called once per frame
    void Update()
    {
        // read message from queue
        if (NetworkScript.messageQueue.Count > 0)
        {
            string message = NetworkScript.messageQueue.Dequeue();
            print("Message dequeuing: " + message);

            if (message.Substring(2, 1).Equals("P"))
            {
                int messageIndex = Int32.Parse(message.Substring(0, 2));
                print("COMPARE: " + messageIndex + " " + inboundMessageCounter);
                if (messageIndex == inboundMessageCounter)
                {
                    print(message.Substring(2, 1));
                    int player = Int32.Parse(message.Substring(3, 1));
                    int movementAmount = Int32.Parse(message.Substring(5, 1)) + 1;
                    diceSideThrown = movementAmount;
                    print(diceSideThrown);
                    MovePlayer(player);

                    inboundMessageCounter++;

                    if (inboundMessageCounter == messageIndex)
                    {
                        isSyncing = false;
                    }
                }

                else if (messageIndex != inboundMessageCounter && isSyncing == false)
                {
                    netObject.SendMessageToServer("s"+inboundMessageCounter+"x");
                    isSyncing = true;
                    /*netObject.SendMessageToServer("\\quit");
                    whoWinsTextShadow.gameObject.SetActive(true);
                    player1MoveText.gameObject.SetActive(false);
                    player2MoveText.gameObject.SetActive(false);
                    whoWinsTextShadow.GetComponent<Text>().text = "Network Error";
                    */
                    //gameOver = true;
                }
            }
        }

        if (player1.GetComponent<FollowThePath>().waypointIndex > 
            player1StartWaypoint + diceSideThrown)
        {
            player1.GetComponent<FollowThePath>().moveAllowed = false;
            player1MoveText.gameObject.SetActive(false);
            player2MoveText.gameObject.SetActive(true);
            player1StartWaypoint = player1.GetComponent<FollowThePath>().waypointIndex - 1;
        }

        if (player2.GetComponent<FollowThePath>().waypointIndex >
            player2StartWaypoint + diceSideThrown)
        {
            player2.GetComponent<FollowThePath>().moveAllowed = false;
            player2MoveText.gameObject.SetActive(false);
            player1MoveText.gameObject.SetActive(true);
            player2StartWaypoint = player2.GetComponent<FollowThePath>().waypointIndex - 1;
        }

        if (player1.GetComponent<FollowThePath>().waypointIndex == 
            player1.GetComponent<FollowThePath>().waypoints.Length)
        {
            whoWinsTextShadow.gameObject.SetActive(true);
            whoWinsTextShadow.GetComponent<Text>().text = "Player 1 Wins";
            gameOver = true;
        }

        if (player2.GetComponent<FollowThePath>().waypointIndex ==
            player2.GetComponent<FollowThePath>().waypoints.Length)
        {
            whoWinsTextShadow.gameObject.SetActive(true);
            player1MoveText.gameObject.SetActive(false);
            player2MoveText.gameObject.SetActive(false);
            whoWinsTextShadow.GetComponent<Text>().text = "Player 2 Wins";
            gameOver = true;
        }
        
        if (Input.GetKey(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }   
    }

    public static void SendMovePlayerToServer(int playerToMove, int moveAmount)
    {
        switch (playerToMove)
        {
            case 1:
                netObject.SendMessageToServer("P1M" + moveAmount);
                break;

            case 2:
                netObject.SendMessageToServer("P2M" + moveAmount);
                break;
        }
    }

    public static void MovePlayer(int playerToMove)
    {
        print("Player to move " + playerToMove);
        switch (playerToMove) { 
            case 1:
                Dice.whosTurn = -1;
                player1.GetComponent<FollowThePath>().moveAllowed = true;
                break;

            case 2:
                Dice.whosTurn = 1;
                player2.GetComponent<FollowThePath>().moveAllowed = true;
                break;
        }
    }

    

}
