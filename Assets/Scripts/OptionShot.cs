using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
// include stopwatch
using Stopwatch = System.Diagnostics.Stopwatch;
using Random2 = System.Random;

public class OptionShot: MonoBehaviour
{

	private GameController gameController;
    private OptionController optionController;
    private PlayerController playerController;
    
    public bool shootable = false;
    
    public bool isLeaving = false;
    
    public bool isDeviating = false;

    private GameObject bolt;

	void Awake()
	{
		GameObject gameControllerObject = GameObject.FindWithTag(
            "GameController");
        gameController = gameControllerObject.GetComponent<GameController>();
        playerController = gameController.GetPlayerController();
        optionController = gameController.GetOptionController();
        GetComponent<Mover>().speed = TaskParameters.fallSpeed_;

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
        while (isDeviating) 
        {
            yield return new WaitForSeconds(0.1f);
        }
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
    
  

    
    public void DeviateShot(Collider other, double pDestroy)
    {
        bolt = other.gameObject;
        
        Vector3 collisionNormal = transform.position - other.transform.position;

        Random2 rnd = new Random2();
        // Determine if the collision is on the left or right side
        // /5/ /5/ float direction = (float) rnd.NextDouble() > 0.5 ? 45 : 135;
        
        // if option is left of the screen, then we want to deviate it to the right
        // if option is right of the screen, then we want to deviate it to the left
        float direction = transform.position.x > 0 ? 45 : 135;
        
        Vector3 originalVector = new Vector3(-1, -1, 0);
            
        // Define the angle you want to rotate by in degrees
        float angleInDegrees = direction + Random.Range(-10f, 10f);

        // Convert the angle to radians (Unity uses radians for rotation)
        float angleInRadians = Mathf.Deg2Rad * angleInDegrees;

        // Create a rotation quaternion based on the axis and angle
        Quaternion rotation = Quaternion.AngleAxis(angleInDegrees, Vector3.down);

        // Rotate the vector using the quaternion
        Vector3 rotatedVector = rotation * originalVector;

        // Output the result
        // Debug.Log("Original Vector: " + originalVector);
        // Debug.Log("Rotated Vector: " + rotatedVector);
        
        // depending of the value of pDestroy, we wanna have more or less resistance from the forcefield
        // if pDestroy is high, then it's a good shot, so we want to have less resistance (i.e. less reflection speed)
        // if pDestroy is high, we want the bolt stay longer on the forcefield, with particple effects
        // whereas if pDestroy is low, we want the bolt to be reflected quickly and without any particple effects
        // so we want to have more resistance (i.e. more reflection speed)

        // this is a fn of pDestroy (logistic)
        float timeToStay = (float) pDestroy;
        
        // wait for the bolt to stay on the forcefield
        StartCoroutine(Deviate(other, timeToStay, rotatedVector));
        
    }
    
    IEnumerator Deviate(Collider other, float timeToStay, Vector3 rotatedVector)
    {
        
        // other has  child named resistance that has a particle system component
        // activate the particle system
        other.transform.Find("resistance").gameObject.SetActive(true);
        other.transform.Find("resistance").gameObject.GetComponent<ParticleSystem>().Play();
        
        // stop the bolt
        other.GetComponent<Rigidbody>().velocity = Vector3.zero;

        //save velocity of the spaceship
        Vector3 velocity = GetComponent<Rigidbody>().velocity;
        // stop the spaceship
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        
        isDeviating = true;

        yield return new WaitForSeconds(timeToStay);
        // deactivate the particle system
        other.transform.Find("resistance").gameObject.SetActive(false);
        other.transform.Find("resistance").gameObject.GetComponent<ParticleSystem>().Stop();

        GameObject explosionFailed = gameController.GetOptionController().explosionFailed;
        Instantiate(explosionFailed, other.transform.localPosition, other.transform.localRotation);
        
        isDeviating = false;

        // set velocity of the spaceship back
        GetComponent<Rigidbody>().velocity = velocity;

        other.GetComponent<Rigidbody>().velocity =  rotatedVector * other.GetComponent<Mover>().speed;
        
        // other.transform.position = Vector3.Reflect(other.transform.position, Vector3.right);
        
        // get that direction and apply to bold rotation (to make it look like it's going in that direction)
        other.transform.rotation = Quaternion.LookRotation(other.GetComponent<Rigidbody>().velocity);
        // change height of bolt
        Vector3 scale = other.transform.localScale;
        other.transform.localScale = new Vector3(scale.x, scale.y, scale.z*3);

        Mover mover = other.GetComponent<Mover>();
        mover.speed = mover.speed * 20f;
        // StartCoroutine(SlowMotion());
     
    }

    void DestroyBolt() {
        Destroy(bolt);
    }

    IEnumerator SlowMotion()
    {
        // slow down time
        Time.timeScale = .4f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // wait
        yield return new WaitForSecondsRealtime(.4f);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        Debug.Log("Time scale: " + Time.timeScale);
        
        DestroyBolt();
    }
}
