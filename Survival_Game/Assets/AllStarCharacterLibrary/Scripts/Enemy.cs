using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour 
{
	public Texture2D cursorTexture;    //攻击图标
    public CursorMode cursorMode = CursorMode.Auto;
    Vector2 hotSpot = new Vector2(16.0f,16.0f);
	public bool targeted=false;
    /*public float speed = 1.0f;
    public GameObject player;*/
	
	void OnMouseEnter() 
	{
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
		targeted = true;
    }
    void OnMouseExit() 
	{
        Cursor.SetCursor(null, new Vector2(16.0f,16.0f), cursorMode);
		targeted = false;
    }

    private void Update()
    {
        /*print(player.transform.position);
        //print(transform.position);
        //print(Vector3.Lerp(transform.position, player.transform.position, 0.5f));
        //transform.Translate( player.transform.position* Time.deltaTime*speed);
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, 0.1f);*/
    }
}
