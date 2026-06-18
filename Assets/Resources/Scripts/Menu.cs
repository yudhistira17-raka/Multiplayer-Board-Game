using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{

    static NetworkScript netObject;
    public static GameObject player1, player2;

    public static int playerIndex = 0;

    Button button1, button2;

    // Start is called before the first frame update
    void Start()
    {
        
        netObject = GameObject.FindGameObjectWithTag("NetworkObject").GetComponent<NetworkScript>();
        player1 = GameObject.Find("Player1");
        player2 = GameObject.Find("Player2");

        button1 = GameObject.FindGameObjectWithTag("button1").GetComponent<Button>();
        button1.onClick.AddListener(OnPlayerOneClicked);

        button2 = GameObject.FindGameObjectWithTag("button2").GetComponent<Button>();
        button2.onClick.AddListener(OnPlayerTwoClicked);
    }


    void StartRoom()
    {
        
    }

    //****************** Lobby events *********************//

    public void OnPlayerOneClicked ()
    {
        playerIndex = 1;
        SceneManager.LoadScene("Game");
    }

    public void OnPlayerTwoClicked()
    {
        playerIndex = 2;
        SceneManager.LoadScene("Game");
    }

}
