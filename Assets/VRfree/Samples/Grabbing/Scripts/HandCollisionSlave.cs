using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRfreePluginUnity {
    public class HandCollisionSlave : MonoBehaviour {
        public HandCollisionMaster handCollisionMaster;

        // to keep track where the collision occured
        public int finger = -1;
        public int phalanx = -1;

        void OnCollisionEnter(Collision collision) {
            handCollisionMaster.ReportCollisionEnter(collision, finger, phalanx);
        }

        void OnCollisionStay(Collision collision) {
            handCollisionMaster.ReportCollisionStay(collision, finger, phalanx);
        }

        void OnCollisionExit(Collision collision) {
            handCollisionMaster.ReportCollisionExit(collision, finger, phalanx);
        }
    }
}