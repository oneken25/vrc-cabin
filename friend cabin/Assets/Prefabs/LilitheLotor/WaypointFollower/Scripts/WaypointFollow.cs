using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WaypointFollow : UdonSharpBehaviour {
    public Transform[] waypoints;
    public Transform objectToMove;
    public float speed = 0.25f;
    public float rotationSpeed = 0.25f;
    public float secondsToWait = 100f;

    private float countdown;
    private string state;
    private int waypointIndex;
    private VRCPlayerApi lp;

    void Start() {
        lp = Networking.LocalPlayer;

        Debug.Log("Local Player");
        Debug.Log(lp.GetPosition());

        if (waypoints != null && waypoints.Length > 0) {
            objectToMove.position = waypoints[0].position;
            waypointIndex = 0;
            state = "waiting";
        } else {
            Debug.LogError("Waypoints not set up! Please set some waypoints first!");
        }
    }

    void LateUpdate() {
        if(state == "moving" && waypoints.Length > 0) {
            if (Vector3.Distance(objectToMove.position, waypoints[waypointIndex].position) < 1f) {
                objectToMove.position = Vector3.Lerp(objectToMove.position, waypoints[waypointIndex].position, Time.deltaTime * speed);
                //objectToMove.rotation = Quaternion.Lerp(objectToMove.rotation, waypoints[waypointIndex].rotation, Time.deltaTime * speed);
            } else {
                Vector3 force = (waypoints[waypointIndex].position - objectToMove.position).normalized;
                objectToMove.position += force * speed * Time.deltaTime;

                // Determine which direction to rotate towards
                Vector3 targetDirection = waypoints[waypointIndex].position - objectToMove.position;

                // The step size is equal to speed times frame time.
                float singleStep = rotationSpeed * Time.deltaTime;

                // Rotate the forward vector towards the target direction by one step
                Vector3 newDirection = Vector3.RotateTowards(objectToMove.forward, targetDirection, singleStep, 0.0f);

                // Draw a ray pointing at our target in
                Debug.DrawRay(objectToMove.position, newDirection, Color.red);

                // Calculate a rotation a step closer to the target and applies rotation to this object
                objectToMove.rotation = Quaternion.LookRotation(newDirection);
            }

            if (Vector3.Distance(objectToMove.position, waypoints[waypointIndex].position) < .1) {
                if (waypointIndex >= waypoints.Length) {
                    state = "waiting";
                    countdown = secondsToWait;
                } else {
                    state = "next";
                    countdown = 0;
                }
                Debug.Log(state);
			}
		}

        if(state == "waiting" || state == "next") {
            countdown--;

            if(countdown <= 0) {   
                waypointIndex++;
                if (waypointIndex >= waypoints.Length) {
                    state = "waiting";
                    waypointIndex = 0;
                } else {
                    state = "moving";
                }
                Debug.Log(state);
			}
		}
	}
}
