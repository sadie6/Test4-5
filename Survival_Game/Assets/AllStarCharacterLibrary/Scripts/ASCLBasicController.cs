using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine.AI;
#pragma warning disable

public class ASCLBasicController : MonoBehaviour 
{
	Animator animator;
	public Collider	floorPlane;//in this demonstration this is set manually, the Retail Ability system has methods for dealing with this automatically via data structures for environments
	public Collider	attackPlane;
	public Enemy[]	enemies;//this array is filled during START by searching for prefabs that have the enemy script attached to them
	public Transform hitReport;//this is for a text mesh object that tells us what damage we did...we need to know where this instance is so we can instantiate off of it
	public Transform particleHit;//this is for a particle emmiter that shows us a hit...we need to know where this instance is so we can instantiate off of it
										//the retail ability system will have this "Inside" an abilities class structual data

	public List<Ability> 	abilities = new List<Ability>();//not really actual abilities as yet, HERE they are only attacks...the retail Ability System package will have actual ones 
	int						ahc=1; //ability hit counter, combo attacks have more than one hit, this variable keeps track of how many hits we have used in update

	public bool 		hitCheck;//<<< IMPORTANT mecanim tells us to perform a hit check at a specific point in attack animations by settting this to TRUE
	
	public int 			WeaponState=0;//unarmed, 1H, 2H, bow, dual, pistol, rifle, spear and ss(sword and shield)
	public bool 		wasAttacking;// we need this so we can take lock the direction we are facing during attacks, mecanim sometimes moves past the target which would flip the character around wildly

	public Renderer		movementTarget;
	Transform 			destFloor;

	float				rotateSpeed = 20.0f; //used to smooth out turning

	public Vector3 		attackPos;
	public Vector3		lookAtPos;
	float				gravity = -0.3f;//unused in this demonstration
	float				fallspeed = 0.0f;

    private List<int> ranklist = new List<int>();    //存放排行榜数据
    private int count = 0;      //计分
    public Text CountText;   //计分板
    public GameObject WinGame;   //赢了的画面
    public GameObject restartBt;   //重新开始游戏按钮
    public GameObject GameOverText;  //输了的画面
    public GameObject StartGamePrefab;   //3 2 1开始动画
    public GameObject[] Export;       //游戏胜利出口，在此游戏中我设置了三个出口，每次游戏会随机实例化一个出口
    private Boolean status = true;
    AudioSource hit_audio;   //打击音效
    public Text TimeText;     //计时
    private int time = 20;
    private float intervalTime = 1;     //倒计时间隔

    SocketControl socketControl = new SocketControl();   //调用SocketControl类
    private Transform ExportTransform;








    public bool rightButtonDown=false;//we use this to "skip out" of consecutive right mouse down input...
	
	// Use this for initialization
	void Start () 
	{
        hit_audio = GetComponent<AudioSource>();
        Time.timeScale = 1;
        SetCountText();
        restartBt.SetActive(false);
        animator = GetComponentInChildren<Animator>();//need this...
		movementTarget.transform.position = transform.position;//initializing our movement target as our current position
		movementTarget.enabled = true;
		lookAtPos = transform.position+transform.forward;
		enemies = Transform.FindObjectsOfType(typeof(Enemy))as Enemy[];//find all the instances of the enemy script which are attached to the targets
        //StartCoroutine(StartGame_Animator(StartGamePrefab));
        TimeText.text = "Time:" + string.Format("{0}", time);
        GameObject exportPlane = Instantiate(Export[UnityEngine.Random.Range(0,3)]);      //随机生成一个出口
        ExportTransform = exportPlane.transform;
    }

	
	// Update is called once per frame
	void LateUpdate () 
	{
        //The Update logic does:
        //
        //	Checks whether or not mecanim told us to check for a hit
        //		Mecanim is great for sending us this info because it can play animations at any speed, rather than calculate the time
        //		we use it to tell us the appropriate hit time
        //
        //	Get UI input from keyboard, and mouse clicks
        //
        //	Tells mecanim what animation we should be playing based on variables such as idling, pain or death
        //
        //	Handle movement and direction
        //
        //
        if (status && time > 0)
        {
            intervalTime -= Time.deltaTime;
            if (intervalTime <= 0)
            {
                intervalTime++;
                time--;
                TimeText.text = "Time:" + string.Format("{0}", time);
            }
            if(transform.position.x > ExportTransform.position.x - 1 && transform.position.x < ExportTransform.position.x + 1 && transform.position.z > ExportTransform.position.z - 1 && transform.position.z < ExportTransform.position.z + 1)
            {
                count += 500;     //如果找到出口，额外增加500分
                SetGameWin();
                status = false;
            }

            if (hitCheck)//hitCheck is a boolean variable, it gets set by mecanim attack states, if mecanim set it...then we need to do hit checks right now
            {

                if (ahc < 1) ahc = 1;//we may have a "double pulse" coming from Mecanim so...
                AbilityCollision abilColl = new AbilityCollision();
                abilColl = abilities[WeaponState].collChecks[abilities[WeaponState].collChecks.Count - ahc];
                if (abilColl.type == 0)
                {
                    //ANGLE RANGE which can be used for any angle/range including radial attacks
                    for (int i = 0; i < enemies.Length; i++)//loop throught the enemies
                    {
                        CheckForHit(enemies[i], abilColl);//ahc= ability hit counter, which is used for indexing a a one tow three punch combo for example...
                    }
                    ahc -= 1;// some abilities have multiple checks, so when we use an ability, we set ahc to the number of hits in the ability (combo punches for example)
                }
                else if (abilColl.type == 1)
                {
                    //Missiles
                    //are a special type, my enemies are its enemies, my damage is its damage, it needs to know who I am, and which of my abilities used it this time
                    Transform tm = (Transform)Instantiate(abilColl.missile, abilColl.missile.position, abilColl.missile.rotation);
                    tm.gameObject.SetActive(true);
                    Missile missile = tm.GetComponent<Missile>();
                    missile.speed = abilColl.speed;
                    missile.damage = abilColl.damage;
                    missile.enemies = enemies;
                    missile.abc = this;
                }
                if (abilColl.type == 2)
                {
                    //Beams/Bullets
                    Transform tm = (Transform)Instantiate(abilColl.missile, abilColl.missile.position, abilColl.missile.rotation);
                    tm.gameObject.SetActive(true);
                    RayShot rayshot = tm.GetComponent<RayShot>();
                    rayshot.damage = abilColl.damage;
                    rayshot.enemies = enemies;
                    Vector3 tempPos = attackPos;
                    tempPos.y = abilColl.missile.position.y;
                    Vector3 tempdelta = Vector3.Normalize(tempPos - abilColl.missile.position); //this is the actual vector from the source point to the attack point normalized
                    rayshot.endPos = tempdelta * abilColl.range;
                    rayshot.abc = this;
                }

                hitCheck = false;// we are done checking, reset the hitCheck bool
            }

            RaycastHit hit;// RayCastHits hold very useful info such as hitnormal and location
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//get a ray that goes from the camera -> "THROUGH" the mouse pointer - > and out into the scene

            if (!Input.GetKey(KeyCode.LeftAlt))//if we are not using the ALT key(camera control)...
            {
                // floor goal	
                if (Input.GetMouseButton(0))//is the left mouse button being clicked?
                {
                    if (floorPlane.Raycast(ray, out hit, 500.0f)) //check to see if that ray hits our "floor"												
                    {
                        //Rungy, this is where you need to use a cursor object with a character controller on it
                        //placing the controller on the spot wherre you hit, getting the name of the object...doing another downward trce to it for the actual position

                        movementTarget.transform.position = hit.point;//mark it where it hit
                        //navMeshAgent.SetDestination(hit.point);
                        movementTarget.enabled = true;
                        lookAtPos = hit.point;
                        lookAtPos.y = transform.position.y;
                        wasAttacking = false;//we're moving now, not attacking
                        
                    }
                }
            }

            switch (Input.inputString)//get keyboard input, probably not a good idea to use strings here...Garbage collection problems with regards to local string usage are known to happen
            {                        //the garbage collection memory problem arises from local alloction of memory, and not freeing it up efficiently
                case "p":
                    animator.SetTrigger("Pain");//the animator controller will detect the trigger pain and play the pain animation
                    break;
               /* case "a":
                    animator.SetInteger("Death", 1);//the animator controller will detect death=1 and play DeathA
                    break;
                case "b":
                    animator.SetInteger("Death", 2);//the animator controller will detect death=2 and play DeathB
                    break;
                case "c":
                    animator.SetInteger("Death", 3);//the animator controller will detect death=3 and play DeathC
                    break; */
                case "n":
                    animator.SetBool("NonCombat", true);//the animator controller will detect this non combat bool, and go into a non combat state "in" this weaponstate
                    break;
                default:
                    break;
            }

            animator.SetInteger("WeaponState", WeaponState);// probably would be better to check for change rather than bashing the value in like this

            if (!Input.GetKey(KeyCode.LeftAlt)) // if we're changing camera transforms, do not use "USE"
            {
                if (Input.GetMouseButton(1))// are we using the right button?
                {
                    if (rightButtonDown != true)// was it previously down? if so we are already using "USE" bailout (we don't want to repeat attacks 800 times a second...just once per press please
                    {
                        attackPlane.enabled = true;
                        if (attackPlane.Raycast(ray, out hit, 500.0f))
                        {
                            movementTarget.transform.position = transform.position; //we are attacking so lock our position to where we are                      
                            attackPos = hit.point;// establish the point that we hit with the mouse
                            attackPos.y = transform.position.y;//use our height for the LOOKAT function, so we stay level and dont lean the character in weird angles                     
                            Vector3 attackDelta = attackPos - transform.position;//我们需要一个向量它是非标准化的方向向量                                                                  
                            lookAtPos = attackPos;
                            animator.SetTrigger("Use");//tell mecanim to do the attack animation(trigger)
                                                       //print("attach");
                            ahc = abilities[WeaponState].collChecks.Count;//ahc=ability hit counter, used for animations that have multiple hits like combos
                            animator.SetBool("Idling", true);//stop moving
                            rightButtonDown = true;//right button was not down before, mark it as down so we don't attack 800 frames a second 
                            wasAttacking = true;//some mecanims will actually move us past the target, so we want to keep looking in one direction instead of spinning wildly around the target
                        }
                        attackPlane.enabled = false;
                    }
                }
            }

            if (Input.GetMouseButtonUp(1))//ok, we can clear the right mouse button and use it for the next attack
            {
                if (rightButtonDown == true)
                {
                    rightButtonDown = false;
                }
            }


            Debug.DrawLine((movementTarget.transform.position + transform.up * 2), movementTarget.transform.position);//useful for visuals in editor

            //We need to handle elevation for mecanim here...we will be doing our ground check right now
            //GROUND check

            ray.direction = transform.up * -1;
            ray.origin = transform.position + transform.up;

            //need two casts...one for velocity ahead of us
            //and one for where we are
            //we need the time from the last frame, as well as the distance traveled since that frame

            //RAMPS,Coming down from Jumps and Falling
            if (floorPlane.Raycast(ray, out hit, 1.0f))
            {
                //always hit if we are going up
                if (hit.point.y > (transform.position.y + 0.02f))
                {
                    transform.position = hit.point;
                    lookAtPos.y = transform.position.y;
                    fallspeed = 0.0f;
                }
            }
            else if (floorPlane.Raycast(ray, out hit, 1.2f))
            {
                //lower hit check for going down ramps specifically
                if (hit.point.y < (transform.position.y - 0.02f))
                {
                    transform.position = hit.point;
                    lookAtPos.y = transform.position.y;
                    fallspeed = 0.0f;
                }
            }
            else
            {//Falling
                transform.parent = null;
                lookAtPos = transform.position;
                movementTarget.transform.position = transform.position;
                movementTarget.transform.parent = null;

                fallspeed += 0.3f;
                Vector3 v = new Vector3(0.0f, fallspeed * Time.deltaTime, 0.0f);
                transform.position -= v;
            }

            Debug.DrawLine((movementTarget.transform.position + transform.up * 2), lookAtPos + transform.up * 2);//useful for visuals in editor

            if (transform.parent == floorPlane.transform)
            {
                lookAtPos = movementTarget.transform.position;
            }
            lookAtPos.y = transform.position.y;
            Quaternion tempRot = transform.rotation;    //save current rotation
            transform.LookAt(lookAtPos);
            Quaternion hitRot = transform.rotation;     // store the new rotation
                                                        // now we slerp orientation
            transform.rotation = Quaternion.Slerp(tempRot, hitRot, Time.deltaTime * rotateSpeed);


            if (Vector3.Distance(movementTarget.transform.position, transform.position) > 0.5f)
            {
                animator.SetBool("Idling", false);
            }
            else
            {
                animator.SetBool("Idling", true);
                movementTarget.enabled = false;
            }
        }
        else
        {
            if (status)
            {
                status = false;
                SetGameOver();
            }
           
        }

	}

    IEnumerator StartGame_Animator(GameObject prefab)            //开始动画
    {
        GameObject mUICanvas = GameObject.Find("Canvas");
        GameObject go = Instantiate(prefab,transform.parent = mUICanvas.transform);
        yield return new WaitForSeconds(3.5F);
        Destroy(go);
        status = true;
    }

    void OnGUI()              //操作说明
	{
		string tempString = "LMB=move RMB=attack ";
		GUI.Label (new Rect (10, 5,1000, 20), tempString);
	}
	
	void CheckForHit(Enemy en, AbilityCollision ac)       
	{
		//AngleRanged
		if(ac.type==0)
		{
			float angle=ac.angle/2;
			Vector3 tDelta = en.gameObject.transform.position - transform.position;
			float tAngle = Vector3.Angle(transform.forward,tDelta);
			if (tAngle< 0) tAngle*=-1;
			if (tAngle<angle)
			{
				if(Vector3.Distance(transform.position, en.gameObject.transform.position)<ac.range)
				{
                    //we have a hit
                    //AngleRanged
                    hit_audio.Play();
                    en.gameObject.SendMessage("Behit");    //击退效果
                    count += 200;  //得分加200
                    SetCountText();   //刷新计分板
                    Transform tm = (Transform) Instantiate(hitReport , (en.gameObject.transform.position + new Vector3(0.0f,1.6f,0.0f)),Quaternion.identity);
					tm.gameObject.SetActive(true);
					Hit tmHit = tm.GetComponent<Hit>();
                    //tmHit.text = ac.damage.ToString();
                    tmHit.text = "200";
                    Transform ph = (Transform) Instantiate(particleHit , (en.gameObject.transform.position + new Vector3(0.0f,1.5f,0.0f)),Quaternion.identity);
					ph.transform.LookAt(Camera.main.transform.position);
					ph.transform.position += (ph.transform.forward * 2.0f);
					ph.gameObject.SetActive(true);
				}
			}
		}
		return;
	}

    private void OnTriggerEnter(Collider other)
    {
        //print(other.gameObject);
        animator.Play("2HDeathC");     //play（“function”）立马切换动画 ， crossFade("function",0.2) 准备0.2秒再切换 
        Invoke("SetGameOver",3);          //Invoke("function",time)  time秒后调用function
    }

    void OnCollisionEnter(Collision collision)
    {
        var tag = collision.collider.name;
        print("Tag is " + tag);
    }

    void SetGameOver()           //游戏结束
    {
        Record();          //调用记录分数函数
        restartBt.SetActive(true);    
        GameObject mUICanvas = GameObject.Find("Canvas");
        GameObject gameover = Instantiate(GameOverText, transform.parent = mUICanvas.transform);
        AudioSource gameover_audio = gameover.GetComponent<AudioSource>();
        gameover_audio.Play();
        Time.timeScale = 0;
    }

    void SetCountText()     //设置计分板
    {
        CountText.text = "Score:" + count.ToString();
        if (count >= 20*200)         //得分达到15*200则胜利
        {
            SetGameWin();
        }

    }

    void SetGameWin()
    {
        Record();
        Time.timeScale = 0;
        restartBt.SetActive(true);
        GameObject mUICanvas = GameObject.Find("Canvas");
        Instantiate(WinGame, transform.parent = mUICanvas.transform);
    }

    void Record()    //记录分数
    {
        socketControl.SubmitRankInfo(count);      //调用SubmitRankInfo()提交分数
        //注释掉客户端文件存贮的方式
        /*if (!File.Exists(Application.dataPath + "/Resources/RankingList.txt"))   //不存在就新建文件
        {
            FileStream fs = new FileStream(Application.dataPath + "/Resources/RankingList.txt", FileMode.Create);
            fs.Close();
        }
        StreamReader sr = new StreamReader(Application.dataPath + "/Resources/RankingList.txt");//声明文本对象读取数据流       
        string nextLine;                                      //声明一个数据接受对象 
        while ((nextLine = sr.ReadLine()) != null)            //将所有存储的分数全部存到ranklist中 
        {
            ranklist.Add(int.Parse(nextLine));
        }
        sr.Close();                                            //关闭文件流 
        ranklist.Add(count);
        ranklist.Sort();
        StreamWriter sw = new StreamWriter(Application.dataPath + "/Resources/RankingList.txt");//声明文本对象写入数据流         
        if (ranklist.Count > 10)
        {
            for (int i = 10; i <= ranklist.Count; i++)
            {
                ranklist.RemoveAt(0);                         //移除超出排行榜之外的分数 
            }
            for (int i = 0; i < ranklist.Count; i++)
            {
                sw.WriteLine(ranklist[i]);//将score写入文本
            }
            ranklist.Clear();                                 //保存完毕清除数组 
            sw.Close();                                        //关闭文件流 
        }
        else
        {
            for (int i = 0; i < ranklist.Count; i++)
            {
                sw.WriteLine(ranklist[i]);//将score写入文本
            }
            ranklist.Clear();                                 //保存完毕清除数组 
            sw.Close();
        }*/
    }
}
