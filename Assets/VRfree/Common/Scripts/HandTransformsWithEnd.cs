using UnityEngine;

namespace VRfreePluginUnity {
    [System.Serializable]
    public class HandTransformsWithEnd : HandTransforms {
        // the metacarpal transforms are needed to change the positions of the knuckles
        // the end transforms are needed to calculate the length of the final phalanx
        public Transform thumb2EndTransform;
        public Transform indexMetaTransform;
        public Transform index2EndTransform;
        public Transform middleMetaTransform;
        public Transform middle2EndTransform;
        public Transform ringMetaTransform;
        public Transform ring2EndTransform;
        public Transform pinkyMetaTransform;
        public Transform pinky2EndTransform;
        // these are used to determine the width of the hand model
        public Transform knucklesIndexTransform;
        public Transform knucklesPinkyTransform;

        /*
         * finger going from 0 (thumb) to 4 (pinky finger), index going from 0 to 3
         */
        public new Transform getFingerTransform(int finger, int phalanx) {
            if(phalanx == 3) {
                if(finger == 0)
                    return thumb2EndTransform;
                else if(finger == 1)
                    return index2EndTransform;
                else if(finger == 2)
                    return middle2EndTransform;
                else if(finger == 3)
                    return ring2EndTransform;
                else if(finger == 4)
                    return pinky2EndTransform;
                else
                    return null;
            } else if(phalanx == -1) {
                if(finger == 1)
                    return indexMetaTransform;
                else if(finger == 2)
                    return middleMetaTransform;
                else if(finger == 3)
                    return ringMetaTransform;
                else if(finger == 4)
                    return pinkyMetaTransform;
                else
                    return null;
            } else
                return base.getFingerTransform(finger, phalanx);
        }

        /*
     * finger going from 0 (thumb) to 4 (pinky finger), index going from -1 (metacarpal) to 3
     */
        public new void setFingerTransform(int finger, int index, Transform transform) {
            if(index == 3) {
                if(finger == 0)
                    thumb2EndTransform = transform;
                else if(finger == 1)
                    index2EndTransform = transform;
                else if(finger == 2)
                    middle2EndTransform = transform;
                else if(finger == 3)
                    ring2EndTransform = transform;
                else if(finger == 4)
                    pinky2EndTransform = transform;
            } else if(index == -1) {
                if(finger == 1)
                    indexMetaTransform = transform;
                else if(finger == 2)
                    middleMetaTransform = transform;
                else if(finger == 3)
                    ringMetaTransform = transform;
                else if(finger == 4)
                    pinkyMetaTransform = transform;
            } else
                base.setFingerTransform(finger, index, transform);
        }

    }
}