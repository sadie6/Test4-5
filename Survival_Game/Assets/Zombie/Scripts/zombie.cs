using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class zombie : MonoBehaviour {

    private Animator zombie_animator;
    public GameObject player;
    private CharacterController zombies;
    private NavMeshAgent navMeshAgent;
    public GameObject wayPointGo;
    private List<Vector3> waypoints = new List<Vector3>();
    private int index = 0;




	// Use this for initialization
	void Start () {
        zombie_animator = this.GetComponent<Animator>();
        zombies = this.GetComponent<CharacterController>();
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        foreach(Transform t in wayPointGo.transform)
        {
            waypoints.Add(t.position);
        }
	}

    // Update is called once per frame
    void Update()
    {
        //print(speed);
        
        Vector3 targetPos = player.transform.position;     
        targetPos.y = transform.position.y;          //y轴为自己
        //transform.transform.LookAt(targetPos);       //僵尸锁定目标
        float d = Vector3.Distance(targetPos, transform.position);     //僵尸和目标的距离
        if (d>8)               
        {
            navMeshAgent.speed = 0.4f;
            Patrol();
            //zombie_animator.SetBool("walk", false);    //离目标超过12m 则不播放走路动画
        }
        else
        {
            if (d > 1.4)
            {
                navMeshAgent.speed = (navMeshAgent.speed + 0.015f)%3.6f;    //随着时间推移，僵尸速度越来越快
                zombie_animator.SetBool("attack", false);
                navMeshAgent.SetDestination(player.transform.position);
                zombie_animator.SetBool("walk", true);
            }
            else
            {
                navMeshAgent.speed = 0.1f;
                zombie_animator.SetBool("attack", true);     //到达攻击距离，播放攻击动画
            }
            
        }
    }
    
    void Patrol()
    {
        if(Mathf.Abs(transform.position.x - waypoints[index].x) >= 0.15 && Mathf.Abs(transform.position.x -waypoints[index].y) >= 0.15)
        {
            navMeshAgent.SetDestination(waypoints[index]);
            zombie_animator.SetBool("walk", true);
        }
        else
        {
            index = (index + 1) % waypoints.Count;      //自动寻路下一个点
        }
    }

   void Behit()     //僵尸被攻击
    {
        navMeshAgent.speed = 0.0f;
        zombies.SimpleMove(-transform.forward*40);    //向后移动一段距离
        
    }




}
