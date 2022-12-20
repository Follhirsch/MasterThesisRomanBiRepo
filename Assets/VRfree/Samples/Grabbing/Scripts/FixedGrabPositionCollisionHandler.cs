/* 
 * Attach this script to an object that should be able to be grabbed and moved, just like a movable, 
 * but will be attached to the hand in a specific position and rotation. 
 * Examples might be guns, rackets, clubs, weapons, tools etc.
 * 
 * The best way to set the hold position and rotation is to press play in the editor and pause it. Then press 
 * "Move To Grab Position" which will move the object close to the right hand. You can now adjust the relative 
 * position of the object to the hand in the Scene view of the editor and commit the changes using "Set Grab Position".
 * Finally copy the FixedGrabPositionCollisionHandler component, stop the game and paste the values back to the 
 * FixedGrabPositionCollisionHandler. 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VRfreePluginUnity {
    [RequireComponent(typeof(Rigidbody))]
    public class FixedGrabPositionCollisionHandler : MovablesCollisionHandler {
        public enum MirrorAxis { x, y, z, none };
        public MirrorAxis leftHandMirrorAxis = MirrorAxis.x;

        [SerializeField]
        private Vector3 holdPosition = new Vector3();
        [SerializeField]
        private Vector3 holdRotation = new Vector3();

        [Tooltip("Used as a  referece when using the \"Set Grab Position\" and \"Move To Grab Position\" buttons in the editor. Not required in-game.")]
        public HandController setGrabPositionRightHandReference;

        protected override void grab(ContactItemList list, HandCollisionMaster handCollisionMaster) {
            Debug.Log("FixedGrabPositionCollisionHandler grab");
            list.isGrabbed = true;
            isGrabbed = true;
            list.moveWithHand = true;

            mRigidbody.isKinematic = true;
            list.collisionRigidbody.constraints = RigidbodyConstraints.None;
            CancelInvoke();

            foreach(System.Type T in typesToDisableOnGrabStatic) {
                Component toDisable = GetComponent(T);
                if(toDisable && toDisable is MonoBehaviour)
                    ((MonoBehaviour)toDisable).enabled = false;
            }

            if(handCollisionMaster.handController.glove.isRightHand || leftHandMirrorAxis == MirrorAxis.none) {
                list.relativePosition = holdPosition;
                list.relativeRotation = Quaternion.Euler(holdRotation);
            } else {
                if(handCollisionMaster.handController.glove.convertForSteamVrGlove) {
                    //mirror position and rotation for left hand along y axis
                    list.relativePosition = new Vector3(holdPosition.x, -holdPosition.y, holdPosition.z);
                    if(leftHandMirrorAxis == MirrorAxis.x) {
                        list.relativeRotation = Quaternion.Euler(-holdRotation.x, holdRotation.y, -holdRotation.z + 180);
                    } else if(leftHandMirrorAxis == MirrorAxis.y) {
                        list.relativeRotation = Quaternion.Euler(holdRotation.x - 180, holdRotation.y + 180, -holdRotation.z + 180);
                    } else {
                        // leftHandMirrorAxis == MirrorAxis.z
                        list.relativeRotation = Quaternion.Euler(holdRotation.x, holdRotation.y + 180, holdRotation.z - 180);
                    }
                } else {
                    //mirror position and rotation for left hand along x axis
                    list.relativePosition = new Vector3(-holdPosition.x, holdPosition.y, holdPosition.z);
                    Quaternion q = Quaternion.Euler(holdRotation);
                    if(leftHandMirrorAxis == MirrorAxis.x) {
                        list.relativeRotation = Quaternion.Euler(holdRotation.x, -holdRotation.y, -holdRotation.z);
                    } else if(leftHandMirrorAxis == MirrorAxis.y) {
                        list.relativeRotation = Quaternion.Euler(holdRotation.x, -holdRotation.y, -holdRotation.z + 180);
                    } else {
                        // leftHandMirrorAxis == MirrorAxis.z
                        list.relativeRotation = new Quaternion(q.x, q.y, -q.z, -q.w);
                    }
                }
            }

            onGrab.Invoke();
            lastPositions.Clear();
        }

        public void setGrabPosition() {
            holdPosition = Quaternion.Inverse(setGrabPositionRightHandReference.handTransforms.handTransform.rotation) * (transform.position - setGrabPositionRightHandReference.handTransforms.handTransform.position);
            holdRotation = (Quaternion.Inverse(setGrabPositionRightHandReference.handTransforms.handTransform.rotation) * transform.rotation).eulerAngles;
        }

        public void moveToGrabPosition() {
            transform.position = setGrabPositionRightHandReference.handTransforms.handTransform.rotation * holdPosition + setGrabPositionRightHandReference.handTransforms.handTransform.position;
            transform.rotation = setGrabPositionRightHandReference.handTransforms.handTransform.rotation * Quaternion.Euler(holdRotation);
        }

    }
}