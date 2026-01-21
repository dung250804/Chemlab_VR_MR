using System;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.model.position.topology.rings
{
    public class DefaultRingsTopology
    {
        public int Size { get; private set; }

        private float _ringAngle;

        public void SetRingSize(int size)
        {
            Size = size;
            // calculate angle based on ring size
            _ringAngle = (Size - 2) * 180f / Size;
        }

        /// <summary>
        /// Get the next position in the ring
        /// </summary>
        /// <param name="inDirection"> The first connection to the Atom </param>
        /// <param name="isFirst"> If the first connection is outside the Ring </param>
        /// <param name="maxBranch"> Max connection of current Atom (Additional for only if isFirst) </param>
        /// <returns> The next priority position of the ring topology </returns>
        /// <exception cref="Exception"></exception>
        public Vector3 GetNextPositionInRing(Vector3 inDirection, bool isFirst = false, int maxBranch = 0)
        {
            var angle = _ringAngle;
            if (isFirst)
            {
                // Note: if first, which mean the inDirection is the connection from outside ring, so we need to modify the angle to get the next position
                angle = maxBranch switch
                {
                    0 => throw new Exception("maxBranch should not be 0 when isFirst is true"),
                    < 2 => throw new Exception("maxBranch should not be less than 2 when isFirst is true"),
                    _ => (360f - _ringAngle) / (maxBranch - 1) * (maxBranch - 2)
                };
            }

            var rotation = Quaternion.Euler(0, angle, 0);
            return rotation * inDirection;
        }

        /// <summary>
        /// Get the nextPriority branch position in the ring
        /// </summary>
        /// <param name="inDirection"> The first connection to the Atom, this is base to calculate the out value (need to be the connection in side the ring, not from branch) </param>
        /// <param name="maxBranch"> Number of connection of current atom, include the inner 2 ring-connection </param>
        /// <param name="branchIndex"> Index of out branch value (should not bigger than maxBranch). The first 2 index (0, 1) is for the inDirection and the next Ring connection </param>
        /// <returns> The next priority position of the ring topology </returns>
        public Vector3 GetNextPriorityPosition(Vector3 inDirection, int maxBranch, int branchIndex)
        {
            switch (branchIndex)
            {
                case 0:
                    return inDirection;
                case 1:
                    return GetNextPositionInRing(inDirection);
            }

            var angle = (360f - _ringAngle) / (maxBranch - 1) * 2;
            var rotation = Quaternion.Euler(0, angle * branchIndex, 0);
            return rotation * inDirection;
        }
    }
}