using System;
using System.Collections;
using System.Collections.Generic;
using Intel.RealSense;
using UnityEngine;

namespace Cubemos
{
    public static class ConversionHelpers
    {
        /// <summary>
        /// Get the average depth inside the neighbourhood of a point defined by a kernel 
        /// </summary>
        /// <param name="depthKernel">The kernel containing depth values which needs to be checked for validity and averaged </param>
        /// <returns>The average depth value of the kernel</returns>
        public static float AverageValidDepthFromNeighbourhood(float[,] depthKernel)
        {
            float average = 0;
            int nValidDepths = 0;
            for (int row = 0; row < depthKernel.GetLength(0); row++)
            {
                for (int col = 0; col < depthKernel.GetLength(1); col++)
                {
                    float depth = depthKernel[row, col];
                    if (depth <= 0.0001)
                        continue;

                    average += depth;
                    nValidDepths++;
                }
            }

            if (nValidDepths == 0)
                return 0.0f;
            else
                return average / nValidDepths;
        }


        /// <summary>
        /// Calculate the 3D position and return it as a 3D Vector
        /// </summary>
        /// <param name="pixelX">X value in color-image coordinates</param>
        /// <param name="pixelY">Y value in color-image coordinates</param>
        /// <param name="depth">Depth value corresponding to the (x, y) pixel for which 3D point is needed</param>
        /// <param name="intrinsics">Intrinsic parameters of the realsense depth sensor</param>
        /// <returns>The 3D vector containing the camera coordinates of the required point</returns>
        public static Vector3 Calculate3DPosition(int pixelX, int pixelY, float depth, Intrinsics intrinsics)
        {
            Vector3 pos = new Vector3(0.0f, 0.0f, 0.0f);

            if (pixelX >= 0 && pixelY >= 0)
            {
                float Z = depth;
                // calculate x and y
                // x = (u - ppx) * z / f;

                float X = ((float)pixelX - intrinsics.ppx) * depth / intrinsics.fx;
                float Y = ((float)pixelY - intrinsics.ppy) * depth / intrinsics.fy;

                pos = new Vector3(X, Y, Z);
            }
            return pos;
        }

        /// <summary>Returns depth values for a square region with the side of the kernelSize centered on the specified coordinate</summary>
        /// <param name="depthFrame">depth map containing float values of distances in mm for every image pixel</param>
        /// <param name="column">x coordinate of the region center</param>
        /// <param name="row">y coordinate of the region center</param>
        /// <param name="kernelSize">side length of the region, e.g. kernelSize = 3 gives a square region 3x3</param>
        /// <returns>The float array of the size kernelSize*kernelSize containing the depth values around (row, column) pixel</returns>
        public static float[,] GetDepthInKernel(Intel.RealSense.DepthFrame depthFrame, int column, int row, int kernelSize)
        {
            if (column >= depthFrame.Width || row >= depthFrame.Height || column < 0 || row < 0)
                throw new IndexOutOfRangeException(
                  String.Format("Requested coordinages x: {0}, y: {1} out of Image Range: {2}*{3}",
                                column,
                                row,
                                depthFrame.Width,
                                depthFrame.Height));

            uint kernelSizeHalf = (uint)(kernelSize / 2);

            uint unStartCol = Math.Max(0, (uint)(column - kernelSizeHalf));
            uint unEndCol = Math.Min((uint)depthFrame.Width, (uint)(column + kernelSizeHalf));

            uint unStartRow = Math.Max(0, (uint)(row - kernelSizeHalf));
            uint unEndRow = Math.Min((uint)depthFrame.Height, (uint)(row + kernelSizeHalf));

            float[,] depthNeigbourhood = new float[unEndRow - unStartRow + 1, unEndCol - unStartCol + 1];
            for (uint i = unStartCol; i <= unEndCol; i++)
            {
                for (uint j = unStartRow; j <= unEndRow; j++)
                {
                    float depth = depthFrame.GetDistance((int)i, (int)j);

                    depthNeigbourhood[j - unStartRow, i - unStartCol] = depth;
                }
            }

            return depthNeigbourhood;
        }

    }
}