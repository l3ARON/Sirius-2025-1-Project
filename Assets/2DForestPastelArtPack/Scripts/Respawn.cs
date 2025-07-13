using UnityEngine;
using System.Collections;

public class Respawn : MonoBehaviour {
    
    public Transform mytransform;
    //Vector3 startLocation;

	void Start () {
        mytransform = transform;
        //startLocation = transform.position;
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Application.LoadLevel(Application.loadedLevel);
        }
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Application.LoadLevel(0);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Application.LoadLevel(1);
        }

    }
    
}
