using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VRfreePluginUnity {
    [RequireComponent(typeof(Rigidbody))]
    public class KnobCollisionHandler : CollisionHandler, IResettable {
        /* 
         * The axis determines the rotation axis of the knob. 
         */
        [SerializeField]
        public Vector3 axis = Vector3.forward;

        /*
         * Determines the allowed rotation range. lowerBound is the allowed rotation in clockwise direction 
         * around the axis, upperBound in counter-clockwise direction.
         */
        public float lowerBound = 0;
        public float upperBound = 0;

        /* the minimum distance of a touch from the axis to be able to turn the knob */
        public float minRadius = 0.02f;

        /* the maximum movement angle of a touch not to be considered an outlier */
        public float maxMovementAngle = 10f;

        /* the minimum amount of touches required at the same time to turn the knob (e.g. a single finger can't turn it) */
        public int minNumTouches = 1;

        //[ReadOnly]
        public float movedDistance;

        [SerializeField]
        private Vector3 rotationAxisOrigin = Vector3.zero;

        [ReadOnly]
        public int debugNumMovements = 0;
        [ReadOnly]
        public float debugMovementAngle = 0;
        [ReadOnly]
        public Vector3 debugContactPoint;

        [System.Serializable]
        public class UnityEventFloat : UnityEvent<float> { }
        /* Returns the angle the knob would turn if not constrained by the upper and lower bounds*/
        public UnityEventFloat onRotate = new UnityEventFloat();


        // used to keep track of collisions in last frame
        [SerializeField]
        private List<KnobContact> previousContacts = new List<KnobContact>();

        [System.Serializable]
        public class KnobContact {
            public ContactPoint contact;
            public int timesSinceValid = 0;

            public KnobContact(ContactPoint contact) {
                this.contact = contact;
                timesSinceValid = 0;
            }
        }

        private Quaternion initialRotation;
        private float initialMovedDistance;

        // Use this for initialization
        void Start() {
            initialRotation = transform.rotation;
            initialMovedDistance = movedDistance;

            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            if(lowerBound > upperBound) {
                float temp = lowerBound;
                lowerBound = upperBound;
                upperBound = temp;
            }
        }

        public override void notifyNewCollisionList(ContactItemList list, HandCollisionMaster handCollisionMaster) {
        }

        public override void notifyRemoveCollisionList(ContactItemList list, HandCollisionMaster handCollisionMaster) {
            previousContacts.Clear();
            list.isGrabbed = false;
        }

        public override void handleCollisionList(ContactItemList list, HandCollisionMaster handCollisionMaster) {
            foreach(KnobContact contact in previousContacts) {
                contact.timesSinceValid++;
            }

            // iterate through all collisions, compare them to the previous collisions and for every contact check how it moved around the axis
            int numMovements = 0;
            float averageMovementAngle = 0;
            foreach(ContactItem collisionItem in list.contacts) {
                if(collisionItem.contact.separation < 2f*Physics.defaultContactOffset) {
                    // this collisionItem actually touches the knob
                    Vector3 newContactPoint = collisionItem.contact.point;

                    KnobContact oldContact = previousContacts.Find(x => x.contact.thisCollider == collisionItem.contact.thisCollider && x.contact.otherCollider == collisionItem.contact.otherCollider);
                    if(oldContact != null) {
                        // make sure the contact point is not old
                        debugContactPoint = newContactPoint;
                        Debug.DrawRay(newContactPoint - 0.02f*Vector3.right, 0.04f*Vector3.right, Color.yellow);
                        Debug.DrawRay(newContactPoint - 0.02f*Vector3.up, 0.04f*Vector3.up, Color.yellow);
                        if(oldContact.timesSinceValid == 1) {
                            // we already have a contact with that source from last time, calculate the difference
                            float movementAngle = movementToAngle(newContactPoint, oldContact.contact.point);
                            if(Mathf.Abs(movementAngle) < maxMovementAngle) {
                                averageMovementAngle += movementAngle;
                                numMovements++;
                            }
                        }
                        previousContacts.Remove(oldContact);
                        previousContacts.Add(new KnobContact(collisionItem.contact));
                        //oldContact.contactPoint = newContactPoint;
                        //oldContact.timesSinceValid = 0;
                    } else {
                        // Collision not in list, add it for next time
                        previousContacts.Add(new KnobContact(collisionItem.contact));
                    }
                }
            }
            debugNumMovements = numMovements;
            if(numMovements >= minNumTouches) {
                averageMovementAngle /= numMovements; //average
                debugMovementAngle = averageMovementAngle;
                onRotate.Invoke(averageMovementAngle);

                float totalMoveDist = movedDistance + averageMovementAngle;
                // check if staying in bounds
                if(totalMoveDist < lowerBound) {
                    // clamp lower bound
                    averageMovementAngle = lowerBound - movedDistance;
                    totalMoveDist = movedDistance + averageMovementAngle;
                }
                if(totalMoveDist > upperBound) {
                    // clamp upper bound
                    averageMovementAngle = upperBound - movedDistance;
                    totalMoveDist = movedDistance + averageMovementAngle;
                }

                transform.RotateAround(getAxisOriginWorldSpace(), getAxisWorldSpace(), averageMovementAngle);
                movedDistance += averageMovementAngle;
            }
            previousContacts.RemoveAll(contact => contact.timesSinceValid > 1);
            if(previousContacts.Count > 0)
                list.isGrabbed = true;
        }

        private float movementToAngle(Vector3 newPosition, Vector3 oldPosition) {
            // get axis, around which the object can rotate
            Vector3 ax = getAxisWorldSpace();
            Vector3 origin = getAxisOriginWorldSpace();

            // get the vector normal to the axis from the axis to currentGrabPosition
            Vector3 originToOld = (oldPosition - origin).normalized;
            Vector3 oldNormal = originToOld - Vector3.Dot(originToOld, ax) * ax;
            Debug.DrawRay(oldPosition, -oldNormal, Color.magenta);
            Vector3 originToNew = (newPosition - origin).normalized;
            Vector3 newNormal = originToNew - Vector3.Dot(originToNew, ax) * ax;
            Debug.DrawRay(newPosition, -newNormal, Color.green);

            //TODO: check minRadius
            return Vector3.SignedAngle(oldNormal, newNormal, ax);
        }

        public Vector3 getAxisWorldSpace() {
            return transform.TransformDirection(axis).normalized;
        }

        public Vector3 getAxisOriginWorldSpace() {
            return transform.position + transform.TransformVector(rotationAxisOrigin);
        }

        void OnDrawGizmos() {
            Vector3 ax = getAxisWorldSpace();
            Vector3 normal;
            if(Mathf.Abs(Vector3.Dot(ax, transform.forward)) < 0.5f) {
                normal = Vector3.Cross(ax, transform.forward);
            } else if(Mathf.Abs(Vector3.Dot(axis.normalized, transform.up)) < 0.5f) {
                normal = Vector3.Cross(ax, transform.up);
            } else {
                normal = Vector3.Cross(ax, transform.right);
            }
            normal.Normalize();

            Vector3 origin = getAxisOriginWorldSpace();
            Gizmos.color = Color.white;
            // draw axis
            Gizmos.DrawRay(origin-0.1f*ax.normalized, 0.2f*ax.normalized);

            Gizmos.color = Color.black;
            // draw lower bound normal
            Gizmos.DrawRay(origin, 0.1f*(Quaternion.AngleAxis((lowerBound-movedDistance), ax)*normal));
            // draw circle section to lower bound
            int numSections = (int)Mathf.Abs((lowerBound-movedDistance)) / 5;
            Vector3 start = 0.08f*normal;
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
            Gizmos.DrawRay(origin, 0.1f*(Quaternion.AngleAxis((upperBound-movedDistance), ax)*normal));
            // draw circle section to upper bound
            numSections = (int)Mathf.Abs((upperBound-movedDistance)) / 5;
            start = 0.08f*normal;
            for(int i = 0; i < numSections; i++) {
                Vector3 end = Quaternion.AngleAxis((upperBound-movedDistance) / numSections, ax) * start;
                Gizmos.DrawLine(origin + start, origin + end);
                start = end;
            }

        }

        public void reset() {
            transform.rotation = initialRotation;
            movedDistance = initialMovedDistance;
        }
    }
}
