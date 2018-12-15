using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;



public class SocketControl 
{

    private TextMesh text;
    private string message;
    private static Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private string host = "127.0.0.1";
    private int port = 12080;
    private byte[] MsgBuffer = new byte[2048];
    public static AccountInfo accountInfo;
    public static RankList rankList;
   
    
    public void ConnectSocket()    //连接socket
    {
        if (!client.Connected)  
        {
            try
            {
                client.Connect(host, port);
                ReceiveData();
            }
            catch (Exception e)
            {
                GameControl.FormatPrint(e.Message);
                return;
            }
        }
    }

    public void Dispose()    //断开socke
    {
        if (client.Connected)
        {
            try
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch (Exception e)
            {
                GameControl.FormatPrint(e.Message);
                return;
            }
        }
    }

    void ReceiveData()    //异步接受消息
    {
        client.BeginReceive(MsgBuffer, 0, MsgBuffer.Length, SocketFlags.None, ReceiveCallback, null);
    }

    private void ReceiveCallback(IAsyncResult ar)   //回调函数
    {
        try
        {
            int Rend = client.EndReceive(ar);
            if (Rend > 0)     //如果收到数据为0 则断开连接
            {
                string res = System.Text.Encoding.ASCII.GetString(MsgBuffer, 0, Rend);
                ActionHand(res);
                ReceiveData();
            }
            else
            {
                Dispose();
            }
        }
        catch (Exception e)
        {
            GameControl.FormatPrint(e.Message);
        }
    }

    public void  Send(string data)     //发送数据函数
    {
        //GameControl.FormatPrint("request_data : " + data);
        byte[] _data = Encoding.ASCII.GetBytes(data);
        ConnectSocket();
        client.Send(_data);
    }

    /*String Recv()
    {
        byte[] rv = new byte[1024];
        int i = client.Receive(rv);
        string res = System.Text.Encoding.ASCII.GetString(rv, 0, i);
        return res;
    }*/

    void ActionHand(string res)       //action处理函数
    {
        
        ActionRes actionRes = JsonUtility.FromJson<ActionRes>(res);
        //GameControl.FormatPrint(actionRes.action);
        if (actionRes.action == "Login")
        {
            LoginAction(actionRes.result);
        }
        else if (actionRes.action == "GetRankList")
        {
            GetRankListAction(actionRes.result);
        }
        else
        {
            GameControl.FormatPrint("don't have this action");
        }
    }

    void GetRankListAction(string result)
    {
        string rankListStr = "{\"ranks\":" + result + "}";
        //GameControl.FormatPrint(rankListStr);
        RankList _rankList = JsonUtility.FromJson<RankList>(rankListStr);
        rankList = _rankList;
        //GameControl.FormatPrint("ranklist_data : " + rankList);
    }

    void LoginAction(string result)
    {
        if (result == "Success")
        {
            GameControl.isLogin = true;
        }
        else
        {
            GameControl.FormatPrint("密码错误");
        }
    }


    public void SubmitRankInfo(int score)
    {
        RankInfo rankinfo = new RankInfo();
        rankinfo.username = accountInfo.username;
        rankinfo.score = score;
        SubmitScore submitScore = new SubmitScore();
        submitScore.action = "SubmitScore";
        submitScore.data = rankinfo;
        string senddata = JsonUtility.ToJson(submitScore);
        Send(senddata);
    }
    
}
