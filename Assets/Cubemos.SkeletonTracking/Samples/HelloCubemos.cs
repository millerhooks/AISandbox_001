using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cubemos
{
    /// <summary>
    /// Minimal sample demonstrating the use of Cubemos Skeleton Tracking with Intel Realsense D415/D435
    /// </summary>
    public class HelloCubemos : MonoBehaviour
    {
        private SkeletonTracker _skeletonTracker;
        private RealsenseManager _realsense;
        public List<Skeleton> lastSkeletons;

        void Start()
        {
            Debug.Log("Starting Cubemos Skeleton Tracking");

            // Initialise the cubemos skeleton tracking and intel realsense pipeline
            _skeletonTracker = new SkeletonTracker();
            _realsense = new RealsenseManager();

            _skeletonTracker.Initialize();
            _realsense.Initialize();
        }

        void Update()
        {
            if (_realsense != null)
            {
                using (var frame = _realsense.GetFrame())
                {
                    lastSkeletons = _skeletonTracker.TrackSkeletonsWithRealsenseFrames(frame.ColorFrame,
                                                                                       frame.DepthFrame,
                                                                                       _realsense.Intrinsics);
                    Debug.Log("Skeletons detected: " + lastSkeletons.Count);
                    foreach (var sk in lastSkeletons)
                    {
                        var sb = new System.Text.StringBuilder();
                        sb.AppendLine("<b>Skeleton " + sk.Index + "</b>");
                        foreach (var j in sk.Joints)
                        {
                            sb.AppendLine("Joint " + j.Key + ": " + j.Value.position + ", confidence: " + j.Value.confidence.ToString("F2"));
                        }
                        Debug.Log(sb.ToString());
                    }
                }
            }
            else
            {
                Debug.LogError("RealSense pipeline not initialized!");
            }
        }
    }
}