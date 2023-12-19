using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using System.Linq;
// include stopwatch
using Stopwatch = System.Diagnostics.Stopwatch;

// define the screen boundaries in Unity
[System.Serializable]
public class Boundary
{
	public float xMin, xMax, zMin, zMax;	
}

public class PlayerController : MonoBehaviour
{

	public bool moveAllowed = true;
	public bool shotAllowed = true;

	public float speed;
	public float tilt;
	public Boundary boundary;

	// allow the ship to fire
	public GameObject shot;
	public Transform shotSpawn;
	public float fireRate;
	private float nextFire;
	public int fireCount = 0;
	public int upCount = 0;
    public int downCount = 0;
	public int leftCount = 0;
    public int rightCount= 0;

	 // reaction time
    public Stopwatch fireTime;
    public Stopwatch moveTime;

    private GameController gameController;
	private KeyCode? lastKeyPressed;

	private static readonly KeyCode[] keyCodes = System.Enum.GetValues(typeof(KeyCode))
												 .Cast<KeyCode>()
												 .Where(k => ((int)k < (int)KeyCode.Mouse0))
												 .ToArray();

	private static KeyCode? GetCurrentKeyDown()
	{
		if (!Input.anyKey)
		{	
			return null;
		}

		for (int i = 0; i < keyCodes.Length; i++)
		{
			if (Input.GetKey(keyCodes[i]))
			{
				return keyCodes[i];
			}
		}
		return null;
	}



	void Start()
    {
		gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
    }

	public void ResetCount()
    {
		Debug.Log("Reset pavlovian count...");
        leftCount = 0;
        rightCount = 0;
        upCount = 0;
        downCount = 0;
        fireCount = 0;
    }

	public void AllowShot(bool value)
	{
		shotAllowed = value;	
	}
	
	public void AllowMove(bool value)
	{
		 moveAllowed = value;
	}
	
	// Coroutine to move the ship to the center of the screen
	// after the player has fired
	public IEnumerator MoveCenter()
	{
		Debug.Log("Player centering...");
		yield return new WaitForSeconds(.8f);
		// StartCoroutine(TempFixed(2.5f));
		// smoothly move the ship to the center of the screen
		Vector3 newPos = new Vector3(0f, transform.position.y, transform.position.z);
		transform.position = Vector3.MoveTowards(transform.position, newPos, 5f);
		// StartCoroutine(TempFixed(2.5f));
	}
	
	public IEnumerator TempFixed(float time)
	{
		moveAllowed = false;
		yield return new WaitForSeconds(time);
		moveAllowed = true;
	}
	
	public void Shoot()
	{
		try {
			fireTime.Stop();
			Debug.Log("Fire time: " + fireTime.ElapsedMilliseconds);
		} catch {
			// Debug.Log("Error: " + e);
			Debug.Log("Fire time error");
		}
		shotAllowed = false;
        nextFire = Time.time + fireRate;
        Instantiate(shot, shotSpawn.position, shotSpawn.rotation); 
        GetComponent<AudioSource>().Play(); // fire sound
		fireCount++;
	}

	void Update()
	{
		KeyCode? keyDown = GetCurrentKeyDown(); 

		if (keyDown == null)
        {
			lastKeyPressed = null;
        }

		// if shot allowed, player is left or right, and fire button is pressed
		if ((Input.GetButton("Fire1") || Input.GetKey("space")) && (Time.time > nextFire) && (
			new int[] {-4, 4}.Contains((int) transform.position.x)) && shotAllowed)
        {
			Shoot();
			AllowMove(false);
			StartCoroutine(MoveCenter());
        }

        if (keyDown == lastKeyPressed)
            return;

		if (!moveAllowed)
			return;

        switch (keyDown)
        {
            case KeyCode.DownArrow:	
    			downCount++;
                break;

            case KeyCode.UpArrow:
                upCount++;
                break;
            case KeyCode.LeftArrow:
                leftCount++;
                break;
            case KeyCode.RightArrow:
                rightCount++;
                break;

        }
        lastKeyPressed = keyDown;

    }


    void FixedUpdate()
	{
		// move the ship
		float moveHorizontal = Input.GetAxis("Horizontal");
		// float moveVertical   = Input.GetAxis("Vertical");

		float x = moveHorizontal > 0 ? 4f : -4f;

		if (moveHorizontal == 0)
		{
			return;
		}
		if (!moveAllowed) 
		{
			Debug.Log("Blocked movements");
			return;
		}
		
		Debug.Log("Right count: " + rightCount);
		Debug.Log("Left count: " + leftCount);

		// check if moveTime exists and is running
		if ((moveTime != null) && moveTime.IsRunning) {
			moveTime.Stop();
			Debug.Log("Move time: " + moveTime.ElapsedMilliseconds);
		}

		// only move on position x	
		Vector3 newPos = new Vector3(x, transform.position.y, transform.position.z);

		transform.position = Vector3.MoveTowards(transform.position, newPos, 2f);
		GetComponent<Rigidbody>().rotation = Quaternion.Euler(0f, 0f, moveHorizontal * -tilt * 12);

    }
}
