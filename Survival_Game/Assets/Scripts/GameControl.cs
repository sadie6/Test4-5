using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

[Serializable]
public class AccountInfo
{
    public string username;
    public string password;
}

[Serializable]
public class ActionLogin
{
    public string action;
    public AccountInfo data;
}

[Serializable]
public class ActionRes
{
    public string action;
    public string result;
}

[Serializable]
public class ActionGetRank
{
    public string action;
}

[Serializable]
public class RankInfo
{
    public string username;
    public int score;
}

[Serializable]
public class SubmitScore
{
    public string action;
    public RankInfo data;
}

[Serializable]
public class RankList
{
    public RankInfo[] ranks;
}

public class GameControl : MonoBehaviour {

    /*private TextMesh text;
    private string message;
    private Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private string host = "127.0.0.1";
    private int port = 12080;
    private byte[] MsgBuffer = new byte[2048]; */
    public static bool isLogin = false;    //开始游戏页面场景名
    //public static bool isRank = false;
    private static string currentscene = "Login";    //当前场景
    SocketControl socketControl = new SocketControl();  

    void Start()
    {
        //GameObject.DontDestroyOnLoad(this.gameObject);
        //ReceiveData();
       
    }

    private void Update()
    {
        if (isLogin && currentscene=="Login")       //如果在登录状态且当前场景为登录场景则切换到游戏开始场景
        {
            SceneManager.LoadScene("StartScene");
            currentscene = "StartScene";
        }
        /*if(isRank && currentscene!="Achievements")
        {
            SceneManager.LoadScene("Achievements");
            currentscene = "Achievements";
        }*/
    }

    
    public void StartGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        currentscene = sceneName;
    }

    public void Login(string sceneName)
    {
        Text nameText = Text.FindObjectOfType<Text>();      
        string user = nameText.text;
        GameObject passwordText = GameObject.FindGameObjectWithTag("pass");
        InputField password = passwordText.GetComponent<InputField>();
        string ps = password.text;
        ActionLogin actionLogin = new ActionLogin();
        actionLogin.action = "Login";
        AccountInfo account = new AccountInfo();
        account.username = user;
        account.password = ps;
        actionLogin.data = account;
        string senddata = JsonUtility.ToJson(actionLogin);     //将对象转成json字符串
        //print(senddata);
        SocketControl.accountInfo = account;
        socketControl.Send(senddata); 
    }   

    public void ExitGame(string sceneName)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneName);
        currentscene = sceneName;
    }

    public void CancelExitGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        currentscene = sceneName;
    }

    public void ConfirmExitGame()
    {
        #if UNITY_EDITOR
            socketControl.Dispose();
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            socketControl.Dispose();
            Application.Quit();
        #endif
    }

    public void RankButton(string sceneName)
    {
        ActionGetRank getrank = new ActionGetRank();
        getrank.action = "GetRankList";
        string senddata = JsonUtility.ToJson(getrank);
        socketControl.Send(senddata);
        currentscene = sceneName;
        SceneManager.LoadScene(sceneName);
        
        /*string res = Recv();
       
        RankList ranklist = JsonUtility.FromJson<RankList>(res);
        print(ranklist.ranks);
        foreach(RankInfo rankinfo in ranklist.ranks)
        {
            print(rankinfo.name);
        }
        */
    }

    public void CloseRank(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        currentscene = sceneName;
    }

    public static void FormatPrint(string message)
    {
        print("----------" + message + "----------");
    }
}
