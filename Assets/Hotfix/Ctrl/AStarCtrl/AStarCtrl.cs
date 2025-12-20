using Pathfinding;

namespace LccHotfix
{
    public class AStarCtrl : AIPath
    {
        protected override void Awake()
        {
            base.Awake();
            simulatedPosition = transform.position;
            simulatedRotation = transform.rotation;
            updatePosition = true;
            updateRotation = true;
        }
    }
}