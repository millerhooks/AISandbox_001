using System;
using Intel.RealSense;
using UnityEngine;

namespace Cubemos
{
    /// <summary>
    /// Class to manage the realsense frame acquisition
    /// </summary>
    public class RealsenseManager
    {
        private Align _align = new Align(Intel.RealSense.Stream.Color);
        private Pipeline _pipeline;
        private Intrinsics _intrinsics;

        /// <summary>
        /// Intrinsics of the intel realsense sensor
        /// </summary>
        public Intrinsics Intrinsics => _intrinsics;

        /// <summary>
        /// Initialise the cubemos skeleton tracking pipeline
        /// </summary>
        public void Initialize()
        {
            // Initialise the intel realsense pipeline as an acquisition device 
            _pipeline = new Pipeline();
            Config cfg = new Config();
            Context context = new Intel.RealSense.Context();
            cfg.EnableStream(Intel.RealSense.Stream.Color, 1280, 720, Format.Bgr8, framerate: 30);
            cfg.EnableStream(Intel.RealSense.Stream.Depth, 1280, 720, framerate: 30);
            PipelineProfile pp = _pipeline.Start(cfg);
            _intrinsics = (pp.GetStream(Intel.RealSense.Stream.Depth).As<VideoStreamProfile>()).GetIntrinsics();
        }

        /// <summary>
        /// Get the skeleton _skeletonTracking results of a particular frame
        /// </summary>
        /// <returns>The object containing the result of skeleton _skeletonTracking call on the realsense frame</returns>
        public Frames GetFrame()
        {
            return new Frames(this);
        }

        /// <summary>
        /// Interface class to manage the skeleton _skeletonTracking call on a new intel realsense frame
        /// </summary>
        public class Frames : IDisposable
        {
            private bool _disposed = false;
            private FramesReleaser _releaser;
            private FrameSet _frames;
            private VideoFrame _colorFrame;
            private DepthFrame _depthFrame;

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (_disposed) return;

                if (disposing)
                {
                    _releaser.Dispose();
                    _frames.Dispose();
                }

                _disposed = true;
            }

            public VideoFrame ColorFrame => _colorFrame;
            public DepthFrame DepthFrame => _depthFrame;

            /// <summary>
            /// Abstraction layer for performing skeleton _skeletonTracking on a realsense frame
            /// </summary>
            /// <param name="realsense">The cubemos skeleton _skeletonTracking API object</param>
            internal Frames(RealsenseManager realsense)
            {
                _releaser = new FramesReleaser();
                {
                    _frames = realsense._pipeline.WaitForFrames();
                    {
                        if (_frames.Count != 2)
                        {
                            Debug.LogWarning("[SkeletonTracker] Not all frames are available...");
                        }
                        else
                        {
                            // Get the color frame from the intel realsense
                            FrameSet f = _frames.ApplyFilter(realsense._align).DisposeWith(_releaser).AsFrameSet().DisposeWith(_releaser);
                            _colorFrame = f.ColorFrame.DisposeWith(_releaser);
                            _depthFrame =
                              realsense._align.Process<DepthFrame>(f.DepthFrame.DisposeWith(_releaser)).DisposeWith(f);
                        }
                    }
                }
            }
        }
    }
}
