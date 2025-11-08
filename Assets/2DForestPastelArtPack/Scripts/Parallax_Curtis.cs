using UnityEngine;
using System.Collections;

public class Parallax_Curtis : MonoBehaviour
{

    public Transform[] backgrounds;         // Array (list) of all the back- and foregrounds to be parallaxed
    private float[] parallaxScales;         // The proportion of the camera's movement to move the backgrounds by

    private Transform cam;                  // reference to the main cameras transform
    private Vector3 previousCamPos;         // the position of the camera in the previous frame

    // Is called before Start(). Great for references.
    void Awake()
    {
        // set up camera the reference
        cam = Camera.main.transform;
    }

    // Use this for initialization
    void Start()
    {
        // The previous frame had the current frame's camera position
        previousCamPos = cam.position;

        var maxDistance = 0f;
        for (int i = 0; i < backgrounds.Length; i++)
        {
            if (backgrounds[i].position.z > maxDistance)
            {
                maxDistance = backgrounds[i].position.z;
            }
        }

        // asigning coresponding parallaxScales
        parallaxScales = new float[backgrounds.Length];
        for (int i = 0; i < backgrounds.Length; i++)
        {
            parallaxScales[i] = backgrounds[i].position.z / maxDistance * -1;
            //Debug.Log(parallaxScales[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {

        // for each background
        for (int i = 0; i < backgrounds.Length; i++)
        {
            // the parallax is the opposite of the camera movement because the previous frame multiplied by the scale
            float parallax = (previousCamPos.x - cam.position.x) * parallaxScales[i];

            // set a target x position which is the current position plus the parallax
            float backgroundTargetPosX = backgrounds[i].position.x + parallax;

            // create a target position which is the background's current position with it's target x position
            backgrounds[i].position = new Vector3(backgroundTargetPosX, backgrounds[i].position.y, backgrounds[i].position.z);
        }

        // set the previousCamPos to the camera's position at the end of the frame
        previousCamPos = cam.position;
    }
}
