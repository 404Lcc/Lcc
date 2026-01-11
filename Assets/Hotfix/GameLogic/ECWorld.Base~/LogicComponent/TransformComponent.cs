using UnityEngine;

namespace LccHotfix
{
    public class TransformComponent : LogicComponent
    {
        public Vector3 position { get; private set; }
        public Quaternion rotation { get; private set; }
        public Vector3 scale { get; private set; }
        public int dirX { get; private set; }

        public void Init(Vector3 position, Quaternion rotation, Vector3 scale, int dirX)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.dirX = dirX;
        }

        public void SetPosition(Vector3 newPos)
        {
            if (position != newPos)
            {
                position = newPos;
                _owner.ReplaceComponent(LogicComponentsLookup.ComTransform, this);
            }
        }

        public void AddPosition(Vector3 deltaPos)
        {
            SetPosition(position + deltaPos);
        }

        public void SetRotation(Quaternion newRot)
        {
            if (rotation != newRot)
            {
                rotation = newRot;
                _owner.ReplaceComponent(LogicComponentsLookup.ComTransform, this);
            }
        }

        public void AddRotation(Quaternion deltaRot)
        {
            SetRotation(deltaRot * rotation);
        }

        public void SetScale(Vector3 newScale)
        {
            if (scale != newScale)
            {
                scale = newScale;
                _owner.ReplaceComponent(LogicComponentsLookup.ComTransform, this);
            }
        }

        public void SetDirX(int newDirX)
        {
            if (dirX != newDirX)
            {
                dirX = newDirX;
                Owner.ReplaceComponent(LogicComponentsLookup.ComTransform, this);
            }
        }

        public int GetDir()
        {
            return dirX;
        }
    }

    public partial class LogicEntity
    {
        public TransformComponent comTransform
        {
            get { return (TransformComponent)GetComponent(LogicComponentsLookup.ComTransform); }
        }

        public bool hasComTransform
        {
            get { return HasComponent(LogicComponentsLookup.ComTransform); }
        }

        public void AddComTransform(Vector3 newPosition, Quaternion newRotation, Vector3 newScale, int newDirX = 1)
        {
            var index = LogicComponentsLookup.ComTransform;
            var component = (TransformComponent)CreateComponent(index, typeof(TransformComponent));
            component.Init(newPosition, newRotation, newScale, newDirX);
            AddComponent(index, component);

            comTransform.SetPosition(component.position);
            comTransform.SetRotation(component.rotation);
            newScale = new Vector3(component.scale.x * newDirX, component.scale.y, component.scale.z);
            comTransform.SetScale(newScale);
        }

        public Vector3 position
        {
            get { return comTransform.position; }
        }

        public Quaternion rotation
        {
            get { return comTransform.rotation; }
        }

        public Vector3 rightDir
        {
            get { return comTransform.rotation * Vector3.right; }
        }


        public Vector3 scale
        {
            get { return comTransform.scale; }
        }
    }

    public static partial class LogicComponentsLookup
    {
        private static ComponentTypeIndex _ComTransformIndex = new(typeof(TransformComponent));
        public static int ComTransform => _ComTransformIndex.Index;
    }
}