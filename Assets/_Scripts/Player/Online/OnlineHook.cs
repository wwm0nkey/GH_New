using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OnlineHook : NetworkBehaviour {

    public Camera camera_camera;
    public GrapplingHookMode grappling_hook_mode;
    public bool draw_dev_line = true;
    public bool disconent_on_los_break;
    public float break_tether_velo;

    public enum GrapplingHookMode { Ratcheting, Loose, Rigid };

    private Rigidbody rigidbody;
    private GameObject player_camera;

    public GameObject hooked_node;
    private float node_distance;
    private float start_node_distance;
    private GameObject grappling_line;
    public GameObject hook;
    public GameObject physicSphere;
    public int rayDistance;
    public Material mat;
   // public double mph;
    //public Text mphText;
    private bool destorysphere;

    // Use this for initialization
    void Start() {
        this.rigidbody = gameObject.GetComponent<Rigidbody>();
        if (draw_dev_line) {
            grappling_line = new GameObject("GrapplingLine");
            grappling_line.SetActive(false);
            LineRenderer line_renderer = grappling_line.AddComponent<LineRenderer>();
            line_renderer.material = mat;
            line_renderer.endWidth = .1f;
            line_renderer.startWidth = .01f;
        }
        hooked_node = null;
        return;
    }

    // Update is called once per frame
    void Update() {
        if (!isLocalPlayer) return;
        //mph = this.rigidbody.velocity.magnitude * 2.237; RealMPH
        //mph = this.rigidbody.velocity.magnitude * 1.1185;
        //mphText.text = "MPH : " + Mathf.RoundToInt((float)mph).ToString();
        //hooked node is null and player has pressed mouse
        if (Input.GetButtonDown("Fire2") && hooked_node == null) {
            destorysphere = false;
            // Ray ray = this.camera_camera.ViewportPointToRay(new Vector3(.5f, .5f, 0));
            Ray ray;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 rayOrigin = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
            RaycastHit raycast_hit;

            if (Physics.Raycast(rayOrigin, Camera.main.transform.forward, out raycast_hit, rayDistance) && raycast_hit.collider.tag != "NoGrapple" || raycast_hit.collider.tag != "Player") {
                hook = Instantiate(physicSphere, raycast_hit.point, Quaternion.identity);
                hook.name = transform.name + " Hook";
                NetworkServer.Spawn(hook);
                this.GetComponent<OnlineGravityBody>().planet = hook.GetComponent<GravityAttractor>();
                hooked_node = hook;
                node_distance = start_node_distance = (Vector3.Distance(hooked_node.transform.position, gameObject.transform.position));

                if (draw_dev_line) {
                    grappling_line.SetActive(true);
                }

            }

            return;
        }

        if (draw_dev_line && hooked_node != null) {
            Vector3[] line_vortex_arr = { gameObject.transform.position, hooked_node.transform.position };
            grappling_line.GetComponent<LineRenderer>().SetPositions(line_vortex_arr);
        }

        //hook is attached and player let go of mouse
        if (Input.GetButtonUp("Fire2") && hooked_node != null) {
            Destroy(hook);
            BreakTether();
            destorysphere = true;
        }

        return;
    }

    void FixedUpdate() {
        if (!isLocalPlayer) return;
        //hook is attached and player held down mouse
        if (hooked_node != null && Input.GetButton("Fire2")) {
            //if los is broken and option is selected, break tether
            RaycastHit hit;
            Ray ray = new Ray(gameObject.transform.position, (hooked_node.transform.position - gameObject.transform.position).normalized);
            if (disconent_on_los_break && Physics.Raycast(ray, out hit) && hit.collider.tag == "GrappleBreak" && destorysphere) {
                Debug.Log("LOS Break");
                BreakTether();
                return;
            }

            //if the velo is > break_tether_velo, break tether
            //if (rigidbody.velocity.magnitude > break_tether_velo)
            //{
            //    BreakTether();
            //    return;
            //}

            if (draw_dev_line) {
                //Vector3[] line_vortex_arr = { gameObject.transform.position, hooked_node.transform.position };
                //grappling_line.GetComponent<LineRenderer>().SetPositions(line_vortex_arr);
            }

            //gets velocity in units/frame, then gets the position for next frame
            Vector3 curr_velo_upf = rigidbody.velocity * Time.fixedDeltaTime;
            Vector3 test_pos = gameObject.transform.position + curr_velo_upf;

            //Depending on the mode, ApplyTensionForce will be called under certain conditions
            if (grappling_hook_mode == GrapplingHookMode.Ratcheting) {
                if (Vector3.Distance(test_pos, hooked_node.transform.position) < node_distance) {
                    node_distance = Vector3.Distance(test_pos, hooked_node.transform.position);
                } else {
                    ApplyTensionForce(curr_velo_upf, test_pos);
                }
            } else if (grappling_hook_mode == GrapplingHookMode.Loose) {
                if (Vector3.Distance(test_pos, hooked_node.transform.position) > node_distance) {
                    ApplyTensionForce(curr_velo_upf, test_pos);
                }
            } else {
                ApplyTensionForce(curr_velo_upf, test_pos);
            }
        }

        return;
    }

    private void ApplyTensionForce(Vector3 curr_velo_upf, Vector3 test_pos) {
        //finds what the new velocity is due to tension force grappling hook
        //normalized vector that from node to test pos
        Vector3 node_to_test = (test_pos - hooked_node.transform.position).normalized;
        Vector3 new_pos = (node_to_test * node_distance) + hooked_node.transform.position;
        Vector3 new_velocity = new_pos - gameObject.transform.position;

        //force_tension = mass * (d_velo / d_time)
        //where d_velo is new_velocity - old_velocity
        Vector3 delta_velocity = new_velocity - curr_velo_upf;
        Vector3 tension_force = (rigidbody.mass * (delta_velocity / Time.fixedDeltaTime));

        rigidbody.AddForce(tension_force, ForceMode.Impulse);
    }

    public void ChangeGrapplingMode(GrapplingHookMode mode) {
        grappling_hook_mode = mode;
        return;
    }

    public void BreakTether() {
        hooked_node = null;
        if (draw_dev_line) {
            grappling_line.SetActive(false);
        }
        return;
    }

    IEnumerator Hooked() {
        yield return new WaitForSeconds(1);
        Ray ray = this.camera_camera.ViewportPointToRay(new Vector3(.5f, .5f, 0));

        RaycastHit raycast_hit;
        if (Physics.Raycast(ray, out raycast_hit) && (raycast_hit.transform.tag == "Planet")) {
            hooked_node = raycast_hit.collider.gameObject;
            node_distance = start_node_distance = (Vector3.Distance(hooked_node.transform.position, gameObject.transform.position));

            if (draw_dev_line) {
                grappling_line.SetActive(true);
            }

        }
    }

}
