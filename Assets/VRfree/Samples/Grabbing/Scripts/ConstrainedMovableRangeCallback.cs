using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VRfreePluginUnity {
    [RequireComponent(typeof(ConstrainedMovable))]
    public class ConstrainedMovableRangeCallback : MonoBehaviour {
        private ConstrainedMovable mConstrainedMovable;

        public float lowerBound = 0;
        public float upperBound = 0;

        public UnityEvent onEnterRange;
        public UnityEvent onExitRange;

        private float lastMovedDistance = 0;
        // Use this for initialization
        void Start() {
            mConstrainedMovable = GetComponent<ConstrainedMovable>();

            lastMovedDistance = mConstrainedMovable.movedDistance;

            if(lowerBound > upperBound) {
                // switch bounds
                float temp = lowerBound;
                lowerBound = upperBound;
                upperBound = temp;
            }
        }

        // Update is called once per frame
        void Update() {
            if(isInsideBounds(lastMovedDistance)) {
                // was inside bounds before
                if(!isInsideBounds(mConstrainedMovable.movedDistance)) {
                    onExitRange.Invoke();
                }
            } else {
                // was outside bounds before
                if(isInsideBounds(mConstrainedMovable.movedDistance)) {
                    onEnterRange.Invoke();
                }
            }
            lastMovedDistance = mConstrainedMovable.movedDistance;
        }

        bool isInsideBounds(float movedDistance) {
            return movedDistance <= upperBound && movedDistance >= lowerBound;
        }

        void OnDrawGizmos() {
            if(mConstrainedMovable == null)
                return;
            Vector3 ax = mConstrainedMovable.getAxisWorldSpace();
            Vector3 normal;
            if(Mathf.Abs(Vector3.Dot(ax, mConstrainedMovable.transform.forward)) < 0.5f) {
                normal = Vector3.Cross(ax, mConstrainedMovable.transform.forward);
            } else if(Mathf.Abs(Vector3.Dot(ax.normalized, transform.up)) < 0.5f) {
                normal = Vector3.Cross(ax, mConstrainedMovable.transform.up);
            } else {
                normal = Vector3.Cross(ax, mConstrainedMovable.transform.right);
            }
            normal.Normalize();
            if(mConstrainedMovable.type == ConstrainedMovable.ConstraintType.slider) {
                Gizmos.color = Color.red;
                // bar marking the origin
                Gizmos.DrawRay(mConstrainedMovable.transform.position - 0.01f * normal + ax * (upperBound - mConstrainedMovable.movedDistance), 0.02f * normal);
                Gizmos.DrawRay(mConstrainedMovable.transform.position - 0.01f * normal + ax * (lowerBound - mConstrainedMovable.movedDistance), 0.02f * normal);
            } /*else if (type == ConstraintType.hinge) {
			    Vector3 origin = getAxisOriginWorldSpace();
			    Gizmos.color = Color.white;
			    // draw axis
			    Gizmos.DrawRay(origin-0.25f*ax, 0.5f*ax.normalized);

			    Gizmos.color = Color.black;
			    // draw lower bound normal
			    Gizmos.DrawRay(origin, 0.5f*(Quaternion.AngleAxis((lowerBound-movedDistance),ax)*normal));
			    // draw circle section to lower bound
			    int numSections = (int)Mathf.Abs((lowerBound-movedDistance)) / 5;
			    Vector3 start = 0.4f*normal;
			    for (int i = 0; i < numSections; i++) {
				    Vector3 end = Quaternion.AngleAxis((lowerBound-movedDistance) / numSections, ax) * start;
				    Gizmos.DrawLine(origin + start, origin + end);
				    start = end;
			    }

			    Gizmos.color = Color.gray;
			    // draw normal forward
			    Gizmos.DrawRay(origin, 0.1f*normal);

			    Gizmos.color = Color.blue;
			    // draw upper bound normal
			    Gizmos.DrawRay(origin, 0.5f*(Quaternion.AngleAxis((upperBound-movedDistance),ax)*normal));
			    // draw circle section to upper bound
			    numSections = (int)Mathf.Abs((upperBound-movedDistance)) / 5;
			    start = 0.4f*normal;
			    for (int i = 0; i < numSections; i++) {
				    Vector3 end = Quaternion.AngleAxis((upperBound-movedDistance) / numSections, ax) * start;
				    Gizmos.DrawLine(origin + start, origin + end);
				    start = end;
			    }
		    }*/
        }
    }
}