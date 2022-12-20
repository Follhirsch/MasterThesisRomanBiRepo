/*
 * This script should be used together with the HandCollisionMaster to handle collision and grabbing of objects.
 * IMPORTANT: Change "Contact Pairs Mode" to "Enable Kinematic Kinematic Pairs" under Edit->Project Settings->Physics,
 * or this will not work. It is then recommended to put kinematic Rigidbodies that you don't want to collide 
 * (such as the fingers of your hand) in a layer without self collision.
 */ 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VRfreePluginUnity {
    [RequireComponent(typeof(Rigidbody))]
    public class MovablesCollisionHandler : CollisionHandler, IResettable {
        public static HashSet<System.Type> typesToDisableOnGrabStatic = new HashSet<System.Type>();
        /*
         * Types of objects in this list will be disabled on grab if they're on the same gameobject as the MovablesCollisionHandler. 
         * The types are added to a static list, so they only have to be added on a single MovablesCollisionHandler.
         */
        public List<Object> typesToDisableOnGrab;

        public bool immediatelyReapplyConstraints = false;

        // we need this additionally to the ContactItemList.isGrabbed to make sure an item can only be grabbed by one hand at a time
        // (the ContactItemList is hand specific and doesn't know about whet the other hand is doing)
        public bool isGrabbed = false;

        // grab coefficient above which a movable object will be "grabbed"
        public float StartGrabCoeff = 2;

        // grab coefficient below which a "grabbed" movable object will be released
        public float EndGrabCoeff = 1.2f;

        [ReadOnly]
        public float debugGrabCoeff = 0;

        protected Rigidbody mRigidbody;
        private RigidbodyConstraints initialConstraints;

        public UnityEvent onGrab;
        public UnityEvent onRelease;

        private Vector3 initialPosition;
        private Quaternion initialRotation;

        public bool improveThrowingSpeed = false;
        public bool drawDebugRays = false;
        public int noHandCollisionLayer;
        protected List<Vector3> lastPositions = new List<Vector3>();
        protected int maxNumLastPositions = 10;
        protected int numSpeedAveragingFrames = 3;

        // Use this for initialization
        protected void Start() {
            foreach(Object o in typesToDisableOnGrab) {
                typesToDisableOnGrabStatic.Add(o.GetType());
            }

            initialPosition = transform.position;
            initialRotation = transform.rotation;

            mRigidbody = GetComponent<Rigidbody>();

            // Maximum velocity of a rigidbody when moving out of penetrating state.
            // Use this property when you want to make your bodies move out of colliding state in a more smooth way than by default.
            mRigidbody.maxDepenetrationVelocity = 1f;
            initialConstraints = mRigidbody.constraints;
        }

        public virtual void reset() {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            mRigidbody.constraints = initialConstraints;
            mRigidbody.velocity = Vector3.zero;
            mRigidbody.angularVelocity = Vector3.zero;

            isGrabbed = false;
        }

        public override void notifyNewCollisionList(ContactItemList list, HandCollisionMaster handCollisionMaster) {
        }

        public override void notifyRemoveCollisionList(ContactItemList list, HandCollisionMaster handCollisionMaster) {
            if(list.isGrabbed)
                release(list, handCollisionMaster);
        }

        public override void handleCollisionList(ContactItemList list, HandCollisionMaster handCollisionMaster) {
            debugGrabCoeff = 0;

            if(list.contacts.Count == 0) {
                // list is empty
                if(list.isGrabbed) {
                    //release grab
                    //Debug.Log("list.collisions.Count == 0");
                    release(list, handCollisionMaster);
                }
                return;
            }

            float grabCoeff = list.getGrabCoefficient();

            //make visible in inspector for debugging
            debugGrabCoeff = grabCoeff;

            if(!isGrabbed && !list.isGrabbed && grabCoeff > StartGrabCoeff && !handCollisionMaster.handController.handPose.isHandFlat && !handCollisionMaster.hasGrabbedItem()) {
                // grab object
                grab(list, handCollisionMaster);
                //Debug.Log(string.Format("movable start grab grabCoeff {0:F3}", grabCoeff));
            } else if((grabCoeff < EndGrabCoeff || handCollisionMaster.handController.handPose.isHandFlat) && list.isGrabbed) {
                //release grab
                release(list, handCollisionMaster);
                //Debug.Log(string.Format("movable end grab grabCoeff {0:F3}", grabCoeff));
            }
            if(list.isGrabbed) {
                if(handCollisionMaster.handController.glove.isWristPositionValid) {
                    lastPositions.Add(list.collisionRigidbody.position);
                    if(lastPositions.Count > maxNumLastPositions)
                        lastPositions.RemoveAt(0);
                } else {
                    lastPositions.Clear();
                }
            }
        }

        protected virtual void grab(ContactItemList list, HandCollisionMaster handCollisionMaster) {
            list.isGrabbed = true;
            isGrabbed = true;
            list.moveWithHand = true;
            list.relativePosition = Quaternion.Inverse(handCollisionMaster.handController.handTransforms.handTransform.rotation) * (transform.position - handCollisionMaster.handController.handTransforms.handTransform.position);
            list.relativeRotation = Quaternion.Inverse(handCollisionMaster.handController.handTransforms.handTransform.rotation)*transform.rotation;

            mRigidbody.isKinematic = true;
            list.collisionRigidbody.constraints = RigidbodyConstraints.None;
            CancelInvoke();

            foreach(System.Type T in typesToDisableOnGrabStatic) {
                Component toDisable = GetComponent(T);
                if(toDisable && toDisable is MonoBehaviour)
                    ((MonoBehaviour)toDisable).enabled = false;
            }

            onGrab.Invoke();
            lastPositions.Clear();
        }

        protected virtual void release(ContactItemList list, HandCollisionMaster handCollisionMaster) {
            isGrabbed = false;
            list.isGrabbed = false;
            list.moveWithHand = false;
            mRigidbody.isKinematic = false;

            foreach(System.Type T in typesToDisableOnGrabStatic) {
                Component toDisable = GetComponent(T);
                if(toDisable && toDisable is MonoBehaviour)
                    ((MonoBehaviour)toDisable).enabled = true;
            }

            handCollisionMaster.handController.handPose.grabEnded();
            onRelease.Invoke();
            if(immediatelyReapplyConstraints) {
                list.collisionRigidbody.constraints = initialConstraints;
            } else {
                list.collisionRigidbody.constraints = RigidbodyConstraints.None;
                StartCoroutine(setConstraintsWhenSleeping(list.collisionRigidbody, initialConstraints));
            }

            if(improveThrowingSpeed) {
                lastPositions.Add(list.collisionRigidbody.position);

                if(lastPositions.Count > numSpeedAveragingFrames + 1) {
                    if(drawDebugRays) {
                        // draw positions from lastPositions and calculate highest velocity
                        for(int i = 0; i < lastPositions.Count - 1; i++) {
                            Vector3 start = lastPositions[i];
                            Vector3 end = lastPositions[i + 1];
                            Vector3 normal = Vector3.Cross(Vector3.up, end - start).normalized;
                            Vector3 velocity = end - start;
                            Debug.DrawLine(start, end, Color.red, 30);
                            Debug.DrawLine(end, end + 0.5f * velocity.magnitude * normal, Color.yellow, 30);
                            if(start == end)
                                Debug.DrawLine(end, end - 0.05f * Vector3.right, Color.white, 30);
                        }
                        Debug.DrawRay(list.collisionRigidbody.position, list.collisionRigidbody.velocity, Color.magenta, 30);
                    }

                    // fill up positions that are the same as the previous one
                    for(int i = 0; i < lastPositions.Count - 2; i++) {
                        Vector3 start = lastPositions[i];
                        Vector3 middle = lastPositions[i + 1];
                        Vector3 end = lastPositions[i + 2];

                        if(start == middle) {
                            lastPositions[i + 1] = (start + end) / 2;
                            if(drawDebugRays)
                                Debug.DrawLine(lastPositions[i + 1], lastPositions[i + 1] - 0.05f * Vector3.right, Color.gray, 30);
                        }
                    }

                    // find highest velocity
                    Vector3 highestVelocity = Vector3.zero;
                    float highestSpeed = 0;
                    int highestSpeedIndex = 0;
                    for(int i = 0; i < lastPositions.Count - numSpeedAveragingFrames; i++) {
                        Vector3 start = lastPositions[i];
                        Vector3 end = lastPositions[i + numSpeedAveragingFrames];

                        Vector3 velocity = end - start;
                        if(velocity.magnitude > highestSpeed) {
                            highestSpeed = velocity.magnitude;
                            highestVelocity = velocity;
                            highestSpeedIndex = i;
                        }
                    }
                    highestVelocity /= 2 * Time.fixedDeltaTime;
                    list.collisionRigidbody.velocity = highestVelocity;
                    list.collisionRigidbody.MovePosition(list.collisionRigidbody.position + highestVelocity * Time.fixedDeltaTime);
                    //StartCoroutine(setLayerDelayed(list.collisionRigidbody.gameObject.layer, list.collisionRigidbody.gameObject, 0.01f));
                    //list.collisionRigidbody.gameObject.layer = noHandCollisionLayer;

                    if(drawDebugRays) {
                        Vector3 start = lastPositions[highestSpeedIndex];
                        Vector3 end = lastPositions[highestSpeedIndex + numSpeedAveragingFrames];
                        Debug.DrawLine(start, end, Color.green, 30);
                        Debug.DrawRay(list.collisionRigidbody.position, highestVelocity * 3 * Time.fixedDeltaTime, Color.cyan, 30);
                        StartCoroutine(drawPath(list.collisionRigidbody, 10));
                    }
                }
            }
        }

        IEnumerator drawPath(Rigidbody rb, int frames) {
            Vector3 pos = rb.position;
            yield return new WaitForFixedUpdate();
            for(int i = 0; i< frames; i++) {
                Debug.DrawLine(pos, rb.position, Color.blue, 30);
                pos = rb.position;
                yield return new WaitForFixedUpdate();
            }
        }

        IEnumerator setLayerDelayed(int layer, GameObject gameObject, float seconds) {
            yield return new WaitForSeconds(seconds);
            gameObject.layer = layer;
            //gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }

        IEnumerator setConstraintsWhenSleeping(Rigidbody rb, RigidbodyConstraints c) {
            // wait until rigidbody is sleeping
            while(!rb.IsSleeping()) {
                yield return null;
            }
            rb.constraints = c;
        }
    }
}