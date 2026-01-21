using UnityEngine;

namespace com.ethnicthv.chemlab.client.model.position.topology
{
    public class DefaultLinear
    {
        private Vector3[] _4BranchDirections;
        
        public DefaultLinear()
        {
            _4BranchDirections = new Vector3[4];
            
            var v0 = new Vector3(1, 1, 1);
            var v1 = new Vector3(-1, 1, -1);
            var v2 = new Vector3(-1, -1, 1);
            var v3 = new Vector3(1, -1, -1);
            
            v0.Normalize();
            v1.Normalize();
            v2.Normalize();
            v3.Normalize();
            
            _4BranchDirections[0] = v0;
            _4BranchDirections[1] = v1;
            _4BranchDirections[2] = v2;
            _4BranchDirections[3] = v3;
        }
        
        public Vector3 GetNextPriorityPosition(Vector3 inDirection, int maxBranch, int branchIndex)
        {
            switch (maxBranch)
            {
                case < 4:
                {
                    var angle = 360f / maxBranch;
                    
                    var axis = Vector3.Cross(Vector3.up, inDirection);
                    if (axis == Vector3.zero)
                    {
                        axis = Vector3.Cross(Vector3.right, inDirection);
                    }
                    
                    var rotation = Quaternion.AngleAxis(angle * (branchIndex + 1), axis);
                    
                    return rotation * inDirection;
                }
                case 4 :
                {
                    var rotation = Quaternion.FromToRotation(-_4BranchDirections[0], inDirection);
                    return rotation * _4BranchDirections[branchIndex];
                }
                default:
                    return Vector3.one + inDirection;
            }
        }
    }
}