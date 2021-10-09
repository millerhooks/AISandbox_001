using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cubemos
{
    /// <summary>
    /// Class to manage the initialisation and results from cubemos skeleton _skeletonTracking 
    /// </summary>
    public class SkeletonTracker
    {
        // Set the network input size to 192 for optimal speed vs accuracy
        private int _networkHeight = 192;
        private Cubemos.SkeletonTracking.Api _skeletontrackingApi;

        /// <summary>
        /// Initialise the cubemos skeleton tracking pipeline
        /// </summary>
        public void Initialize()
        {
            // Initialize logging to output all messages with severity level INFO or higher to the file
            Cubemos.Api.InitialiseLogging(Cubemos.LogLevel.CM_LL_INFO, bWriteToConsole: true, logFolder: Common.DefaultLogDir());

            try
            {
                _skeletontrackingApi = new Cubemos.SkeletonTracking.Api(Common.DefaultLicenseDir());
            }
            catch (Exception)
            {
                Debug.LogWarning("[SkeletonTracker] Could not find an activation key in the path " + Common.DefaultLicenseDir());
            }

            // Initialise CUBEMOS DNN framework with the required deep learning model and the target compute
            String modelPath = Common.DefaultModelDir() + "\\fp16\\skeleton-tracking.cubemos";

            try
            {
                Debug.Log("[SkeletonTracker] Loading model from " + modelPath + "..");
                _skeletontrackingApi.LoadModel(Cubemos.TargetComputeDevice.CM_GPU, modelPath);
            }
            catch (Exception ex)
            {
                Debug.LogError("[SkeletonTracker] Unable to load model from path " + modelPath + " Exception: " + ex.Message);
            }

        }


        /// <summary>
        /// Abstraction layer for performing skeleton _skeletonTracking on a realsense frame
        /// </summary>
        /// <param name="tracking">The cubemos skeleton _skeletonTracking API object</param>
        public List<Skeleton> TrackSkeletonsWithRealsenseFrames(Intel.RealSense.VideoFrame colorFrame, 
                                                                Intel.RealSense.DepthFrame alignedDepthFrame, 
                                                                Intel.RealSense.Intrinsics depthIntrinsics)
        {
            List<Skeleton> _skeletons = new List<Skeleton>();

            // Send inference request and get the _skeletons
            System.Collections.Generic.List<Cubemos.SkeletonTracking.Api.SkeletonKeypoints> skeletonKeypoints;

            // Send inference request and get the _skeletons
            _skeletontrackingApi.RunSkeletonTracking(colorFrame.Data,
                                                            colorFrame.Width,
                                                            colorFrame.Height,
                                                            colorFrame.BitsPerPixel / 8,
                                                            _networkHeight,
                                                            out skeletonKeypoints);

            for (int skeletonIndex = 0; skeletonIndex < skeletonKeypoints.Count; skeletonIndex++)
            {
                var sk = new Skeleton(skeletonIndex);

                var skeleton = skeletonKeypoints[skeletonIndex];
                for (int jointIndex = 0; jointIndex < skeleton.listJoints.Count; jointIndex++)
                {
                    Cubemos.SkeletonTracking.Api.Coordinate coordinate = skeleton.listJoints[jointIndex];

                    if (jointIndex < 18 && (coordinate.x > 0 && coordinate.y > 0))
                    {
                        float[,] depthValues = ConversionHelpers.GetDepthInKernel(
                            alignedDepthFrame, (int)coordinate.x, (int)coordinate.y, kernelSize: 5);

                        float averageDepth = ConversionHelpers.AverageValidDepthFromNeighbourhood(depthValues);
                        Vector3 pos = ConversionHelpers.Calculate3DPosition((int)coordinate.x, (int)coordinate.y, averageDepth, depthIntrinsics);

                        sk.Joints.Add(jointIndex, new Skeleton.Joint { position = pos, confidence = skeleton.listConfidences[jointIndex] });
                    }
                    else if(jointIndex < 18)
                    {
                        sk.Joints.Add(jointIndex, new Skeleton.Joint { position = new Vector3(-1,-1,-1), confidence = skeleton.listConfidences[jointIndex] });
                    }
                }
                _skeletons.Add(sk);
            }
            return _skeletons;
        }
    }
}