/* 
 * This script should be added to an object in the ConstrainedMovable Layer. On it, the type of constraint can be 
 * chosen between slider and hinge. In case of a slider, the axis determines the allowed movement diretion, in case 
 * of a hinge the rotation axis.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRfreePluginUnity {
    public class ConstrainedMovable : MonoBehaviour, IResettable {
        public enum ConstraintType { slider, hinge, slider2d }
        [Tooltip("Slider: can move along single axis within bounds, no rotation. \n"
            + "Hinge: can rotate around single axis within bounds, considers only position of hand.\n"
            + "Slider2d: can move along plane spanned by axis and axis2d within bounds, no rotation.")]
        public ConstraintType type = ConstraintType.slider;

        [Header("Common Properties")]
        [Tooltip("In case of a slider or slider2d, the axis determines the allowed movement diretion, in case of a hinge the rotation axis.")]
        public Vector3 axis = Vector3.forward;

        [Tooltip("Origin around which to rotate. Only used when type == hinge")]
        public Vector3 rotationAxisOrigin = Vector3.zero;


        [Tooltip("In case of a slider, the upper and lower bounds determine the allowed movement range along the axis (in gloabal space). "
             + "lowerBound is the allowed movement in the opposite direction of the axis, upperBound in the direction of the axis. "
             + "In case of a hinge, they determine the allowed rotation range. lowerBound is the allowed rotation in clockwise direction "
             + "around the axis, upperBound in counter-clockwise direction.")]
        public float lowerBound = 0;
        [Tooltip("In case of a slider, the upper and lower bounds determine the allowed movement range along the axis (in gloabal space). "
             + "lowerBound is the allowed movement in the opposite direction of the axis, upperBound in the direction of the axis. "
             + "In case of a hinge, they determine the allowed rotation range. lowerBound is the allowed rotation in clockwise direction "
             + "around the axis, upperBound in counter-clockwise direction.")]
        public float upperBound = 0;

        [ReadOnly]
        public float movedDistance;

        [Header("Slider2d Only Properties")]
        [Tooltip("In Slider2d, the axis2 determines the second allowed movement diretion. Must be normal to axis.")]
        public Vector3 axis2 = Vector3.forward;
        [Tooltip("Only used in slider2d. The upper and lower bounds determine the allowed movement range along axis2 (in gloabal space). "
        + "lowerBound is the allowed movement in the opposite direction of the axis, upperBound in the direction of the axis.")]
        public float lowerBound2 = 0;
        [Tooltip("Only used in slider2d. The upper and lower bounds determine the allowed movement range along axis2 (in gloabal space). "
        + "lowerBound is the allowed movement in the opposite direction of the axis, upperBound in the direction of the axis.")]
        public float upperBound2 = 0;

        [ReadOnly]
        public float movedDistance2;


        // Use this for initialization
        void Start() {
            if(lowerBound > upperBound) {
                float temp = lowerBound;
                lowerBound = upperBound;
                upperBound = temp;
            }
        }

        private void OnValidate() {
            if(type == ConstraintType.slider2d) {
                // make sure axes are normal to each other
                Vector3 normal = Vector3.Cross(axis, axis2);
                axis2 = Vector3.Cross(normal, axis).normalized;
            }
        }

        public Vector3 getAxisWorldSpace() {
            return transform.TransformDirection(axis).normalized;
        }

        public Vector3 getAxis2WorldSpace() {
            return transform.TransformDirection(axis2).normalized;
        }

        public Vector3 getAxisOriginWorldSpace() {
            return transform.position + transform.TransformVector(rotationAxisOrigin);
        }

        void OnDrawGizmos() {
            Vector3 ax = getAxisWorldSpace();
            Vector3 ax2 = getAxis2WorldSpace();
            Vector3 normal;
            if(Mathf.Abs(Vector3.Dot(ax, transform.forward)) < 0.5f) {
                normal = Vector3.Cross(ax, transform.forward);
            } else if(Mathf.Abs(Vector3.Dot(ax, transform.up)) < 0.5f) {
                normal = Vector3.Cross(ax, transform.up);
            } else {
                normal = Vector3.Cross(ax, transform.right);
            }
            normal.Normalize();
            if(type == ConstraintType.slider) {
                Gizmos.color = Color.black;
                // backwards axis
                Gizmos.DrawRay(transform.position, ax * (lowerBound-movedDistance));
                // backwards arrow
                Gizmos.DrawRay(transform.position + ax * (lowerBound-movedDistance), 0.01f *ax + 0.01f * normal);
                Gizmos.DrawRay(transform.position + ax * (lowerBound-movedDistance), 0.01f *ax - 0.01f * normal);

                Gizmos.color = Color.blue;
                // bar marking the origin
                Gizmos.DrawRay(transform.position - 0.01f * normal, 0.02f * normal);
                // forwards axis
                Gizmos.DrawRay(transform.position, ax * (upperBound-movedDistance));
                // backwards arrow
                Gizmos.DrawRay(transform.position + ax * (upperBound-movedDistance), -0.01f *ax + 0.01f * normal);
                Gizmos.DrawRay(transform.position + ax * (upperBound-movedDistance), -0.01f *ax - 0.01f * normal);

            } else if(type == ConstraintType.hinge) {
                Vector3 origin = getAxisOriginWorldSpace();
                Gizmos.color = Color.white;
                // draw axis
                Gizmos.DrawRay(origin-0.25f*ax, 0.5f*ax);

                Gizmos.color = Color.black;
                // draw lower bound normal
                Gizmos.DrawRay(origin, 0.5f*(Quaternion.AngleAxis((lowerBound-movedDistance), ax)*normal));
                // draw circle section to lower bound
                int numSections = (int)Mathf.Abs((lowerBound-movedDistance)) / 5;
                Vector3 start = 0.4f*normal;
                for(int i = 0; i < numSections; i++) {
                    Vector3 end = Quaternion.AngleAxis((lowerBound-movedDistance) / numSections, ax) * start;
                    Gizmos.DrawLine(origin + start, origin + end);
                    start = end;
                }

                Gizmos.color = Color.gray;
                // draw normal forward
                Gizmos.DrawRay(origin, 0.1f*normal);

                Gizmos.color = Color.blue;
                // draw upper bound normal
                Gizmos.DrawRay(origin, 0.5f*(Quaternion.AngleAxis((upperBound-movedDistance), ax)*normal));
                // draw circle section to upper bound
                numSections = (int)Mathf.Abs((upperBound-movedDistance)) / 5;
                start = 0.4f*normal;
                for(int i = 0; i < numSections; i++) {
                    Vector3 end = Quaternion.AngleAxis((upperBound-movedDistance) / numSections, ax) * start;
                    Gizmos.DrawLine(origin + start, origin + end);
                    start = end;
                }
            } else if(type == ConstraintType.slider2d) {
                Gizmos.color = Color.black;
                normal = ax2;
                // backwards axis
                Gizmos.DrawRay(transform.position, ax * (lowerBound-movedDistance));
                // backwards arrow
                Gizmos.DrawRay(transform.position + ax * (lowerBound-movedDistance), 0.01f *ax + 0.01f * normal);
                Gizmos.DrawRay(transform.position + ax * (lowerBound-movedDistance), 0.01f *ax - 0.01f * normal);

                Gizmos.color = Color.blue;
                // bar marking the origin
                Gizmos.DrawRay(transform.position - 0.01f * normal, 0.02f * normal);
                // forwards axis
                Gizmos.DrawRay(transform.position, ax * (upperBound-movedDistance));
                // backwards arrow
                Gizmos.DrawRay(transform.position + ax * (upperBound-movedDistance), -0.01f *ax + 0.01f * normal);
                Gizmos.DrawRay(transform.position + ax * (upperBound-movedDistance), -0.01f *ax - 0.01f * normal);

                normal = ax;
                Gizmos.color = Color.black;
                // backwards axis
                Gizmos.DrawRay(transform.position, ax2 * (lowerBound2-movedDistance2));
                // backwards arrow
                Gizmos.DrawRay(transform.position + ax2 * (lowerBound2-movedDistance2), 0.01f *ax2 + 0.01f * normal);
                Gizmos.DrawRay(transform.position + ax2 * (lowerBound2-movedDistance2), 0.01f *ax2 - 0.01f * normal);

                Gizmos.color = Color.cyan;
                // bar marking the origin
                Gizmos.DrawRay(transform.position - 0.01f * normal, 0.02f * normal);
                // forwards axis
                Gizmos.DrawRay(transform.position, ax2 * (upperBound2-movedDistance2));
                // backwards arrow
                Gizmos.DrawRay(transform.position + ax2 * (upperBound2-movedDistance2), -0.01f *ax2 + 0.01f * normal);
                Gizmos.DrawRay(transform.position + ax2 * (upperBound2-movedDistance2), -0.01f *ax2 - 0.01f * normal);

            }
        }

        public void reset() {
            movedDistance = 0;
        }
    }
}