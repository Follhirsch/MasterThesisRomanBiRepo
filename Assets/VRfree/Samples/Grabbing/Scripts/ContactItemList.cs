using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VRfreePluginUnity {
    /* each CollisionListItemList will hold CollisionListItems with collisions with the same rigidbody */
    [System.Serializable]
    public class ContactItemList : System.IEquatable<ContactItemList> {
        public CollisionHandler collisionHandler;
        public List<ContactItem> contacts = new List<ContactItem>();
        public Rigidbody collisionRigidbody;
        public bool isGrabbed = false;

        public bool moveWithHand = false;
        public Vector3 relativePosition;
        public Quaternion relativeRotation;

        public ContactItemList(Rigidbody collisionRigidbody) {
            this.collisionRigidbody = collisionRigidbody;
            //joint = collisionRigidbody.gameObject.GetComponent<FixedJoint>();
            collisionHandler = collisionRigidbody.GetComponent<CollisionHandler>();
        }

        /* define two items as equal if their collision's rigidbodies are the same */
        public bool Equals(ContactItemList other) {
            return other.collisionRigidbody == this.collisionRigidbody;
        }


        /*
         * Returns a factor describing from how different angles an object is touched by adding up the normals
         * of all contacts and dividing the number of contacts by the magnitude of the resulting vector.
         * When all contacts are more or less pointing in the same direction, the coefficient is close to 1 and
         * the object is not grabbed. When the contact normals are pointing against each other, they partially 
         * cancel out during summation and the factor becomes larger.
         * Typically a grab can be started at a coefficient of 2, and ended below 1.2.
         */
        public float getGrabCoefficient() {
            Vector3 totalContactNormals = new Vector3();
            float numContacts = 0;
            foreach(ContactItem item in contacts) {
                // check if contact is close enough
                if(item.contact.separation < Physics.defaultContactOffset) {
                    // check that colliders aren't intersecting too much, since that often leads to unintended grabs
                    if(item.contact.separation > -2*Physics.defaultContactOffset) {
                        totalContactNormals += -item.contact.normal;
                        numContacts++;
                    }
                }
            }
            //return numContacts / totalContactNormals.magnitude;
            return 2*(numContacts - totalContactNormals.magnitude); //note that c# float can handle divide by zero
        }

        /*
         * returns the average of all contact points as grabCenter (in world coordinates)
         */
        public Vector3 getGrabCenter() {
            float numContacts = 0;
            Vector3 grabCenter = Vector3.zero;
            foreach(ContactItem item in contacts) {
                if(item.contact.separation < 2f*Physics.defaultContactOffset) {
                    numContacts++;
                    grabCenter += item.contact.point;
                }
            }
            grabCenter /= numContacts;
            return grabCenter;
        }

    }

    [System.Serializable]
    public class ContactItem : System.IEquatable<ContactItem> {
        public ContactPoint contact;
        public int age;

        // to keep track where the collision occured
        public int finger;
        public int phalanx;

        public ContactItem(ContactPoint contact, int finger, int phalanx) {
            this.contact = contact;
            this.age = 0;
            this.finger = finger;
            this.phalanx = phalanx;
        }

        /* define two items as equal if their source GameObjects are the same */
        public bool Equals(ContactItem other) {
            return this.contact.otherCollider == other.contact.otherCollider && this.contact.thisCollider == other.contact.thisCollider;
        }
    }
}