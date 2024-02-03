using UnityEngine;

namespace HJ.Runtime.States
{
    public static class LadderUtility
    {
        public static float LadderEval(Vector3 playerCenter, Vector3 ladderStart, Vector3 ladderEnd)
        {
            Vector3 projection = Vector3.Project(playerCenter - ladderStart, ladderEnd - ladderStart) + ladderStart;
            return Vector3.Distance(ladderStart, projection) / Vector3.Distance(ladderStart, ladderEnd);
        }

        public static float LadderDotUp(Vector3 ladderEnd, Vector3 centerPos)
        {
            Vector3 ladderPos = ladderEnd;
            Vector3 playerPos = centerPos;
            Vector3 p1 = new Vector3(0, playerPos.y, 0);
            Vector3 p2 = new Vector3(0, ladderPos.y, 0);
            return Vector3.Dot((p2 - p1).normalized, Vector3.up);
        }

        public static float LadderDotForward(Transform ladder, Vector3 centerPos)
        {
            Vector3 ladderPos = ladder.position;
            Vector3 playerPos = centerPos;
            Vector3 p1 = new Vector3(playerPos.x, 0, playerPos.z);
            Vector3 p2 = new Vector3(ladderPos.x, 0, ladderPos.z);
            return Vector3.Dot((p2 - p1).normalized, ladder.forward) * 90;
        }
    }
}
