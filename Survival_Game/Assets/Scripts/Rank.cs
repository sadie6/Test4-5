using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class Rank : MonoBehaviour {
    private List<int> ranklist = new List<int>();
    private List<string> user = new List<string>();
    public GameObject Achievement;

	// Use this for initialization
	void Start () {
        RankDown();
    }

    void RankDown()
    {
        Time.timeScale = 1;
        RankList ranks = SocketControl.rankList;    //获取排行榜数据
        if(ranks != null && ranks.ranks.Length > 0)        //如果排行榜数据>0条 则加入ranklist列表
        {
            foreach(RankInfo rankInfo in ranks.ranks)
            {
                ranklist.Add(rankInfo.score);
                user.Add(rankInfo.username);
            }
        }

        //注释掉客户端文件获取方式
       /* if (!File.Exists(Application.dataPath + "/Resources/RankingList.txt"))   //判断文件是否存在不存在则新建一个
        {
            FileStream fs = new FileStream(Application.dataPath + "/Resources/RankingList.txt", FileMode.Create);
            fs.Close();
        }
        StreamReader sr = new StreamReader(Application.dataPath + "/Resources/RankingList.txt");  
        string nextLine;
        while ((nextLine = sr.ReadLine()) != null)
        {
            ranklist.Add(int.Parse(nextLine));
        }
        sr.Close();
        */

        for (int i = 0; i < ranklist.Count; i++)              //实例化排行榜 
        {
            GameObject item = Instantiate(Achievement);
            GameObject itemplace = GameObject.Find("Grid_Achievements");
            item.SetActive(true);
            item.transform.SetParent(itemplace.transform,false);
            item.transform.Find("Text_AchievementDescription").GetComponent<Text>().text = (i + 1).ToString();
            item.transform.Find("Text_RewardAmount").GetComponent<Text>().text = ranklist[i].ToString();
            item.transform.Find("Text_userinfo").GetComponent<Text>().text = user[i].ToString();
        }
        ranklist.Clear();
    }

   
	
}
