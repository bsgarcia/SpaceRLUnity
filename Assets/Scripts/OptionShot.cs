using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
// include stopwatch
using Stopwatch = System.Diagnostics.Stopwatch;


public class OptionShot: MonoBehaviour
{

	private GameController gameController;
    private OptionController optionController;
    private PlayerController playerController;
    
    public bool shootable = false;
    
    public bool isLeaving = false;

	void Awake()
	{
		GameObject gameControllerObject = GameObject.FindWithTag(
            "GameController");
        gameController = gameControllerObject.GetComponent<GameController>();
        playerController = gameController.GetPlayerController();
        optionController = gameController.GetOptionController();

    }
    
    void OnTriggerEnter(Collider other)
    {   
        // make the option shootable if it crosses the upper boundary
        if (other.tag == "BoundaryShootable")
        {
            shootable = true;
        }

        // the option is shot
        if (other.tag == "Bolt" && shootable)
        {
            shootable = false;

            //DeviateShot(other);
            optionController.SetChoice(tag, other);
            return;
        }
    }
    

    IEnumerator MoveInArc(Transform transform_, Vector3 startPos, Vector3 endPos, float speed, float height) 
    {
        isLeaving = true;
        float i = 0.0f;
        float rate = 1.0f/speed;
        while (i < 1.0f) 
        {
            i += Time.deltaTime * rate;
            float arc = Mathf.Sin(i * Mathf.PI) * height;
            
            Vector3 newPos = Vector3.Lerp(startPos, endPos, i);
            newPos.z -= arc;
            
            transform_.position = newPos;
            
            yield return null;
        }
        isLeaving = false;
    }
    
    public void LeaveScreen(float speed = .8f, float height = 2.2f) 
    {
        shootable = false;
        
        // end position is down the screen on the left
        // find object by name
        Transform targetLeft = GameObject.FindWithTag("LeaveScreenTargetLeft").transform;
        Transform targetRight = GameObject.FindWithTag("LeaveScreenTargetRight").transform;
        Transform myTarget = transform.position.x < 0 ? targetLeft : targetRight;
        Vector3 endPos = myTarget.position;
        Vector3 startPos = transform.position;

        StartCoroutine(MoveInArc(transform, startPos, endPos, speed, height));
        
    }
    
    public void DeviateShot(Collider other)
    {
        Vector3 collisionNormal = transform.position - other.transform.position;

        float direction;
        // Determine if the collision is on the left or right side
        if (collisionNormal.x > 0)
        {
            direction = Random.Range(-15, -5);
        }
        else
        {
            direction = Random.Range(5, 15);
        } 

        other.GetComponent<Rigidbody>().velocity = new Vector3(
            direction, 0, 1) * other.GetComponent<Mover>().speed;
        
        // get that direction and apply to bold rotation (to make it look like it's going in that direction)
        other.transform.rotation = Quaternion.LookRotation(other.GetComponent<Rigidbody>().velocity);
        // StartCoroutine(gameController.DestroyWithDelay(other.gameObject, 1.5f));
    }
}
