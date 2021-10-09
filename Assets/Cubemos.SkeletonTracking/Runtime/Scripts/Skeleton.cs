using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cubemos
{
    /// <summary>
    /// Class to wrap the results of the cubemos skeleton _skeletonTracking buffers
    /// </summary>
    public class Skeleton
    {
        /// <summary>
        /// Structure to store the skeleton joint information along with estimation confidence
        /// </summary>
        public struct Joint
        {
            public float confidence;
            public Vector3 position;
        }

        public int Index { get; }
        public Dictionary<int, Joint> Joints { get; }

        public Skeleton(int index)
        {
            this.Index = index;
            this.Joints = new Dictionary<int, Joint>();
        }
    }
}