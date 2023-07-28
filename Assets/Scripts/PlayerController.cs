using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using System.Linq;

// define the screen boundaries in Unity
[System.Serializable]
public class Boundary
{
	public float xMin, xMax, zMin, zMax;	
}

public class PlayerController : MonoBehaviour
{

	public bool fixedMove = false;

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

    private GameController gameController;
	private KeyCode? lastKeyPressed;
	private bool shotAllowed = false;

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
	
	// Coroutine to move the ship to the center of the screen
	// after the player has fired
	public IEnumerator MoveCenter()
	{
		yield return new WaitForSeconds(0.8f);
		StartCoroutine(TempFixed(.8f));
		// smoothly move the ship to the center of the screen
		Vector3 newPos = new Vector3(0f, transform.position.y, transform.position.z);
		transform.position = Vector3.MoveTowards(transform.position, newPos, 5f);
		StartCoroutine(TempFixed(.8f));
	}
	
	public IEnumerator TempFixed(float time)
	{
		fixedMove = true;
		yield return new WaitForSeconds(time);
		fixedMove = false;
	}

	void Update()
	{
		KeyCode? keyDown = GetCurrentKeyDown(); 

		if (keyDown == null)
        {
			lastKeyPressed = null;
        }

		if ((Input.GetButton("Fire1") || Input.GetKey("space")) && (Time.time > nextFire) && (
			new int[] {-4, 4}.Contains((int) transform.position.x)) && shotAllowed)
        {
			shotAllowed = false;
            nextFire = Time.time + fireRate;
            Instantiate(shot, shotSpawn.position, shotSpawn.rotation); 
            GetComponent<AudioSource>().Play(); // fire sound
			fireCount++;
        }

        if (keyDown == lastKeyPressed)
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
		float moveVertical   = Input.GetAxis("Vertical");

		float x = moveHorizontal > 0 ? 4f : -4f;

		if (moveHorizontal == 0)
		{
			return;
		}
		if (fixedMove) 
		{
			Debug.Log("Blocked movements");
			return;
		}


		// only move on position x	
		Vector3 newPos = new Vector3(x, transform.position.y, transform.position.z);

		transform.position = Vector3.MoveTowards(transform.position, newPos, 2f);
		GetComponent<Rigidbody>().rotation = Quaternion.Euler(0f, 0f, moveHorizontal * -tilt * 12);

    }
}
