/*
 * This script should be added to an object that can only move in a constrained way.
 * A constrained movable is an object that can be grabbed and moved, however, it can not be moved freely with all degrees of freedom.
 */ 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VRfreePluginUnity {
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(ConstrainedMovable))]
    public class ConstrainedMovablesCollisionHandler : CollisionHandler, IResettable {
        [System.Serializable]
        public class UnityEventVector3 : UnityEvent<Vector3> { }
        public UnityEvent startConstrainSlider;
        public UnityEventVector3 onConstrainSlider;

        // grab coefficient above which a movable object will be "grabbed"
        public float StartGrabCoeff = 2;

        // grab coefficient below which a "grabbed" movable object will be released
        public float EndGrabCoeff = 1.4f;

        [ReadOnly] public float debugGrabCoeff = 0;

        [HideInInspector]
        public ConstrainedMovable constrainedMovable;

        private Transform originalParent;

        public UnityEvent onGrab;
        public UnityEvent onRelease;

        [ReadOnly]
        public bool isGrabbed = false; // to make sure only one hand can grab this at a time

        // Use this for initialization
        void Start() {
            constrainedMovable = GetComponent<ConstrainedMovable>();
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
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

            if(isActiveAndEnabled && !isGrabbed && !list.isGrabbed && grabCoeff > StartGrabCoeff && !handCollisionMaster.handController.handPose.isHandFlat && !handCollisionMaster.hasGrabbedItem()) {
                //Debug.Log(string.Format("constrained movable start grab grabCoeff {0:F3}", grabCoeff));
                // grab object
                grab(list, handCollisionMaster);
            } else if((grabCoeff < EndGrabCoeff || handCollisionMaster.handController.handPose.isHandFlat) && list.isGrabbed) {
                //release grab
                //Debug.Log(string.Format("constrained movable end grab grabCoeff {0:F3}", grabCoeff));
                release(list, handCollisionMaster);
            }
        }

        Vector3 constrainSlider(Vector3 positionRaw, Vector3 grabPositionToWrist, Vector3 grabPositionToRigidbody, ConstrainedMovable movable, HandCollisionMaster handCollisionMaster) {
            // the position where the object would be grabbed if it weren't constrained
            Vector3 rawGrabPosition = positionRaw + handCollisionMaster.handController.handTransforms.wristTransform.TransformVector(grabPositionToWrist);
            // the position of where the object is currently grabbed (constrained)
            Vector3 currentGrabPosition = movable.transform.position + movable.transform.TransformVector(grabPositionToRigidbody);
            // the distance the grabbed objecct would have traveled if it weren't constrained
            Vector3 rawMoveDistance = rawGrabPosition - currentGrabPosition;

            // projecting the rawMoveDistance onto the moveDirection
            float dotP = Vector3.Dot(movable.getAxisWorldSpace(), rawMoveDistance);
            Vector3 projectedMoveDistance = dotP*movable.getAxisWorldSpace().normalized;
            // check if staying in bounds
            float moveDistMagn = Mathf.Sign(dotP)*projectedMoveDistance.magnitude;
            float totalMoveDist = movable.movedDistance + moveDistMagn;
            if(totalMoveDist < movable.lowerBound) {
                // clamp lower bound
                moveDistMagn = movable.lowerBound - movable.movedDistance;
                projectedMoveDistance = Mathf.Sign(dotP) * projectedMoveDistance * moveDistMagn / projectedMoveDistance.magnitude;
                totalMoveDist = movable.movedDistance + moveDistMagn;
            }
            if(totalMoveDist > movable.upperBound) {
                // clamp upper bound
                moveDistMagn = movable.upperBound - movable.movedDistance;
                projectedMoveDistance = Mathf.Sign(dotP) * projectedMoveDistance * moveDistMagn / projectedMoveDistance.magnitude;
                totalMoveDist = movable.movedDistance + moveDistMagn;
            }

            movable.transform.position += projectedMoveDistance;
            movable.movedDistance += moveDistMagn;
            Vector3 constrainedPosition = movable.transform.position + movable.transform.TransformVector(grabPositionToRigidbody) - handCollisionMaster.handController.handTransforms.wristTransform.TransformVector(grabPositionToWrist);
            onConstrainSlider.Invoke(constrainedPosition - positionRaw);
            return constrainedPosition;
        }

        Vector3 constrainSlider2d(Vector3 positionRaw, Vector3 grabPositionToWrist, Vector3 grabPositionToRigidbody, ConstrainedMovable movable, HandCollisionMaster handCollisionMaster) {
            // the position where the object would be grabbed if it weren't constrained
            Vector3 rawGrabPosition = positionRaw + handCollisionMaster.handController.handTransforms.wristTransform.TransformVector(grabPositionToWrist);
            // the position of where the object is currently grabbed (constrained)
            Vector3 currentGrabPosition = movable.transform.position + movable.transform.TransformVector(grabPositionToRigidbody);
            // the distance the grabbed objecct would have traveled if it weren't constrained
            Vector3 rawMoveDistance = rawGrabPosition - currentGrabPosition;

            for(int i = 0; i < 2; i++) {
                // projecting the rawMoveDistance onto the moveDirection
                Vector3 axis = i == 0 ? movable.getAxisWorldSpace() : movable.getAxis2WorldSpace();
                float dotP = Vector3.Dot(axis, rawMoveDistance);
                Vector3 projectedMoveDistance = dotP * axis;
                // check if staying in bounds
                float moveDistMagn = Mathf.Sign(dotP)*projectedMoveDistance.magnitude;
                float totalMoveDist = (i == 0 ? movable.movedDistance : movable.movedDistance2) + moveDistMagn;
                if(totalMoveDist < (i == 0 ? movable.lowerBound : movable.lowerBound2)) {
                    // clamp lower bound
                    moveDistMagn = i == 0 ? movable.lowerBound - movable.movedDistance : movable.lowerBound2 - movable.movedDistance2;
                    projectedMoveDistance = Mathf.Sign(dotP) * projectedMoveDistance * moveDistMagn / projectedMoveDistance.magnitude;
                    totalMoveDist = (i == 0 ? movable.movedDistance : movable.movedDistance2) + moveDistMagn;
                }
                if(totalMoveDist > (i == 0 ? movable.upperBound : movable.upperBound2)) {
                    // clamp upper bound
                    moveDistMagn = i == 0 ? movable.upperBound  - movable.movedDistance : movable.upperBound2 - movable.movedDistance2;
                    projectedMoveDistance = Mathf.Sign(dotP) * projectedMoveDistance * moveDistMagn / projectedMoveDistance.magnitude;
                    totalMoveDist = (i == 0 ? movable.movedDistance : movable.movedDistance2) + moveDistMagn;
                }

                movable.transform.position += projectedMoveDistance;
                if(i == 0) {
                    movable.movedDistance += moveDistMagn;
                } else {
                    movable.movedDistance2 += moveDistMagn;
                }
            }
            Vector3 constrainedPosition = movable.transform.position + movable.transform.TransformVector(grabPositionToRigidbody) - handCollisionMaster.handController.handTransforms.wristTransform.TransformVector(grabPositionToWrist);
            onConstrainSlider.Invoke(constrainedPosition - positionRaw);
            return constrainedPosition;
        }

        Vector3 constrainHinge(Vector3 positionRaw, Vector3 grabPositionToWrist, Vector3 grabPositionToRigidbody, ConstrainedMovable movable, HandCollisionMaster handCollisionMaster) {
            // the position where the object would be grabbed if it weren't constrained
            Vector3 rawGrabPosition = positionRaw + handCollisionMaster.handController.handTransforms.wristTransform.TransformVector(grabPositionToWrist);

            Debug.DrawLine(rawGrabPosition - 0.02f * Vector3.right, rawGrabPosition + 0.02f * Vector3.right, Color.yellow);
            Debug.DrawLine(rawGrabPosition - 0.02f * Vector3.up, rawGrabPosition + 0.02f * Vector3.up, Color.yellow);
            Debug.DrawLine(rawGrabPosition - 0.02f * Vector3.forward, rawGrabPosition + 0.02f * Vector3.forward, Color.yellow);

            // the position of where the object is currently grabbed (constrained)
            Vector3 currentGrabPosition = movable.transform.position + movable.transform.TransformVector(grabPositionToRigidbody);
            Debug.DrawLine(currentGrabPosition - 0.02f * Vector3.right, currentGrabPosition + 0.02f * Vector3.right, Color.red);
            Debug.DrawLine(currentGrabPosition - 0.02f * Vector3.up, currentGrabPosition + 0.02f * Vector3.up, Color.red);
            Debug.DrawLine(currentGrabPosition - 0.02f * Vector3.forward, currentGrabPosition + 0.02f * Vector3.forward, Color.red);

            // get axis, around which the object can rotate
            Vector3 axis = movable.getAxisWorldSpace();
            Vector3 origin = movable.getAxisOriginWorldSpace();

            // get the vector normal to the axis from the axis to currentGrabPosition
            Vector3 originToCurrent = (currentGrabPosition - origin).normalized;
            Vector3 currentNormal = originToCurrent - Vector3.Dot(originToCurrent, axis) * axis;
            Debug.DrawRay(currentGrabPosition, -currentNormal, Color.magenta);
            Vector3 originToNew = (rawGrabPosition - origin).normalized;
            Vector3 newNormal = originToNew - Vector3.Dot(originToNew, axis) * axis;
            Debug.DrawRay(rawGrabPosition, -newNormal, Color.green);

            float angle = Vector3.SignedAngle(currentNormal, newNormal, axis);
            float totalMoveDist = movable.movedDistance + angle;
            // check if staying in bounds
            if(totalMoveDist < movable.lowerBound) {
                // clamp lower bound
                angle = movable.lowerBound - movable.movedDistance;
                totalMoveDist = movable.movedDistance + angle;
            }
            if(totalMoveDist > movable.upperBound) {
                // clamp upper bound
                angle = movable.upperBound - movable.movedDistance;
                totalMoveDist = movable.movedDistance + angle;
            }

            move(angle);

            return movable.transform.position + movable.transform.TransformVector(grabPositionToRigidbody) - handCollisionMaster.handController.handTransforms.wristTransform.TransformVector(grabPositionToWrist);
        }

        public void move(float angle) {
            if(constrainedMovable.type == ConstrainedMovable.ConstraintType.hinge) {
                Vector3 axis = constrainedMovable.getAxisWorldSpace();
                Vector3 origin = constrainedMovable.getAxisOriginWorldSpace();

                constrainedMovable.transform.RotateAround(origin, axis, angle);
                constrainedMovable.movedDistance += angle;
            }
        }

        void grab(ContactItemList list, HandCollisionMaster handCollisionMaster) {
            originalParent = list.collisionRigidbody.transform.parent;
            if(constrainedMovable != null) {
                isGrabbed = true;
                list.isGrabbed = true;

                Vector3 grabCenter = list.getGrabCenter();
                Debug.DrawLine(grabCenter - 0.02f * Vector3.right, grabCenter + 0.02f * Vector3.right, Color.cyan, 0.5f);
                Debug.DrawLine(grabCenter - 0.02f * Vector3.up, grabCenter + 0.02f * Vector3.up, Color.cyan, 0.5f);
                Debug.DrawLine(grabCenter - 0.02f * Vector3.forward, grabCenter + 0.02f * Vector3.forward, Color.cyan, 0.5f);
                Vector3 grabPositionToWrist = handCollisionMaster.handController.handTransforms.wristTransform.InverseTransformPoint(grabCenter);
                Vector3 grabPositionToRigidbody = constrainedMovable.transform.InverseTransformPoint(grabCenter);
                if(constrainedMovable.type == ConstrainedMovable.ConstraintType.slider) {
                    startConstrainSlider.Invoke();
                    handCollisionMaster.handController.handPose.setPositionConstraint(positionRaw => constrainSlider(positionRaw, grabPositionToWrist, grabPositionToRigidbody, constrainedMovable, handCollisionMaster));
                } else if(constrainedMovable.type == ConstrainedMovable.ConstraintType.hinge) {
                    handCollisionMaster.handController.handPose.setPositionConstraint(positionRaw => constrainHinge(positionRaw, grabPositionToWrist, grabPositionToRigidbody, constrainedMovable, handCollisionMaster));
                } else if(constrainedMovable.type == ConstrainedMovable.ConstraintType.slider2d) {
                    handCollisionMaster.handController.handPose.setPositionConstraint(positionRaw => constrainSlider2d(positionRaw, grabPositionToWrist, grabPositionToRigidbody, constrainedMovable, handCollisionMaster));
                }
                onGrab.Invoke();
            }
        }

        void release(ContactItemList list, HandCollisionMaster handCollisionMaster) {
            isGrabbed = false;

            list.isGrabbed = false;
            handCollisionMaster.handController.handPose.grabEnded();
            list.collisionRigidbody.transform.parent = originalParent;

            //list.collisionRigidbody.constraints = RigidbodyConstraints.None;
            handCollisionMaster.handController.handPose.removePositionConstraint();
            onRelease.Invoke();
        }

        public void reset() {
            if(constrainedMovable == null)
                return;
            if(constrainedMovable.type == ConstrainedMovable.ConstraintType.hinge) {
                Vector3 axis = constrainedMovable.getAxisWorldSpace();
                Vector3 origin = constrainedMovable.getAxisOriginWorldSpace();
                constrainedMovable.transform.RotateAround(origin, axis, -constrainedMovable.movedDistance);
                constrainedMovable.movedDistance = 0;
            } else if(constrainedMovable.type == ConstrainedMovable.ConstraintType.slider) {
                Vector3 movedirection = constrainedMovable.getAxisWorldSpace().normalized;
                constrainedMovable.transform.position -= constrainedMovable.movedDistance*movedirection;
                constrainedMovable.movedDistance = 0;
            }
        }
    }
}