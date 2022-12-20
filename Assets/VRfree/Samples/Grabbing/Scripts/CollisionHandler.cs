using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRfreePluginUnity {
    [RequireComponent(typeof(Rigidbody))]
    public abstract class CollisionHandler : MonoBehaviour {
        public abstract void handleCollisionList(ContactItemList list, HandCollisionMaster handCollisionMaster);
        public abstract void notifyNewCollisionList(ContactItemList list, HandCollisionMaster handCollisionMaster);
        public abstract void notifyRemoveCollisionList(ContactItemList list, HandCollisionMaster handCollisionMaster);
    }
}