using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastDebug : MonoBehaviour {

	public bool DebugOn;
	public int RayDistance;
	public GameObject gravitySphere;
    public GameObject PhysicsSphere;
    public GameObject _hook;

    public bool isGravMode;
    private bool inAir = false;
    private HingeJoint grabHinge;
    public int speed;
    public Transform HookSapwner;
    public bool returnHook;
    //public LineRenderer rope;
    // Use this for initialization
       void Start () {
		
	}

	
	// Update is called once per frame
	void Update () {
		if(DebugOn){
		Vector3 forward = transform.TransformDirection(Vector3.forward) * RayDistance;
        Debug.DrawRay(transform.position, forward, Color.green);
		}

        LineRenderer rope = GetComponent<LineRenderer>();
        rope.SetPosition(0, HookSapwner.transform.position);
        if (returnHook) {
            rope.SetPosition(1, HookSapwner.transform.position);
        }
        if (Input.GetButtonDown("Fire2")){
            returnHook = false;
		    Ray ray;
    	    RaycastHit hit;
    	    ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 rayOrigin = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));


            if (Physics.Raycast(rayOrigin, Camera.main.transform.forward, out hit, RayDistance) && isGravMode && hit.collider.tag != "NoGrapple"){
                Debug.Log(hit.collider.tag.ToString());
                _hook = Instantiate(gravitySphere, hit.point, Quaternion.identity);
                rope.SetPosition(1, _hook.transform.position);
            } else if (Physics.Raycast(rayOrigin, Camera.main.transform.forward, out hit, RayDistance) && !isGravMode && hit.collider.tag != "NoGrapple") {
                _hook = Instantiate(PhysicsSphere, hit.point, Quaternion.identity);
                rope.SetPosition(1, _hook.transform.position);
            }
		}
        if (Input.GetButtonUp("Fire2")) {
            Destroy(_hook);
            returnHook = true;
        }

    }
}
