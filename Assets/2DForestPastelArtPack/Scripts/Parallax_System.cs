using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*
public class tags
{
	string AddTagsToParallax  (tags : string[] )
	{
		for(int i=0; i < tags.Length; i++)
			
			
	}
}*/


public class Parallax_System : MonoBehaviour
{
	public Transform[] parallaxObjects;			// Array of all the backgrounds to be parallaxed.
	//public GameObject[] parallaxObjectsHolder;
	public List<GameObject> parallaxObjectsHolder = new List<GameObject>();
	public float[] depthOfParallaxObjects;      // The proportion of the camera's movement to move the backgrounds by.
    //public float parallaxScale;				// The proportion of the camera's movement to move the backgrounds by.
    //public float parallaxReductionFactor;		// How much less each successive layer should parallax.
    public float smoothing = 1f;				// How smooth the parallax effect should be.
	 
	private string[] tags = new string[4] { "BackGround", "GameGroundBack","GameGroundFront","ForeGround"};						// tags that I want to parallax
	private Transform cam;						// Shorter reference to the main camera's transform.
	private Vector3 previousCamPos;				// The postion of the camera in the previous frame.
	
	
	void Awake ()
	{
		// Setting up the reference shortcut.
		cam = Camera.main.transform;
	}
	
	
	void Start ()
	{
		// The 'previous frame' had the current frame's camera position.
		previousCamPos = cam.position;

		for ( int i = 0; i < tags.Length; i++)
		{
			parallaxObjectsHolder.AddRange (GameObject.FindGameObjectsWithTag(tags[i]));
			//Debug.Log(parallaxObjectsHolder[0]);
		}
		parallaxObjects= new Transform[parallaxObjectsHolder.Count];
		depthOfParallaxObjects = new float [parallaxObjects.Length];

		for(int i = 0 ; i < parallaxObjectsHolder.Count ; i ++)
		{
			parallaxObjects[i]=parallaxObjectsHolder[i].GetComponent<Transform>();
			depthOfParallaxObjects[i] = -parallaxObjectsHolder[i].transform.position.z;
			
		}
	}
	

	void Update ()
	{
		
		// For each successive background...
		for(int i = 0; i < parallaxObjects.Length; i++)
		{
			// The parallax is the opposite of the camera movement since the previous frame multiplied by the scale.
			float parallaxX = (previousCamPos.x - cam.position.x) * depthOfParallaxObjects[i];
			// ... set a target x position which is their current position plus the parallax multiplied by the reduction.
			float backgroundTargetPosX = parallaxObjects[i].position.x + parallaxX;// * (i * parallaxReductionFactor + 1);


			// The parallax is the opposite of the camera movement since the previous frame multiplied by the scale.
			float parallaxY = (previousCamPos.y - cam.position.y) * (depthOfParallaxObjects[i]*0.5f);
			// ... set a target y position which is their current position plus the parallax multiplied by the reduction.
			float backgroundTargetPosY = parallaxObjects[i].position.y + parallaxY;// * (i * parallaxReductionFactor + 1);
             

			// Create a target position which is the background's current position but with it's target x position.
			Vector3 backgroundTargetPos = new Vector3(backgroundTargetPosX, backgroundTargetPosY, parallaxObjects[i].position.z);
			
			// Lerp the background's position between itself and it's target position.
			parallaxObjects[i].position = Vector3.Lerp(parallaxObjects[i].position, backgroundTargetPos, smoothing * Time.deltaTime);
		}
		
		// Set the previousCamPos to the camera's position at the end of this frame.
		previousCamPos = cam.position;
	}
}
