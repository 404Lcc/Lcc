using Entitas;
using UnityEngine;

namespace LccHotfix
{
    public class ComTransform : LogicComponent
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
                Owner.ReplaceComponent(LogicComponentsLookup.ComTransform, this);
                position = newPos;
            }
        }

        public void SetRotation(Quaternion newRot)
        {
            if (rotation != newRot)
            {
                Owner.ReplaceComponent(LogicComponentsLookup.ComTransform, this);
                rotation = newRot;
            }
        }

        public void SetScale(Vector3 newScale)
        {
            if (scale != newScale)
            {
                Owner.ReplaceComponent(LogicComponentsLookup.ComTransform, this);
                scale = newScale;
            }
        }

        public void SetDirX(int newDirX)
        {
            if (dirX != newDirX)
            {
                Owner.ReplaceComponent(LogicComponentsLookup.ComTransform, this);
                dirX = newDirX;
            }
        }
    }

    public partial class LogicEntity
    {
        public ComTransform comTransform
        {
            get { return (ComTransform)GetComponent(LogicComponentsLookup.ComTransform); }
        }

        public bool hasComTransform
        {
            get { return HasComponent(LogicComponentsLookup.ComTransform); }
        }

        public void AddComTransform(Vector3 newPosition, Quaternion newRotation, Vector3 newScale, int newDirX = 1)
        {
            var index = LogicComponentsLookup.ComTransform;
            var component = (ComTransform)CreateComponent(index, typeof(ComTransform));
            component.Init(newPosition, newRotation, newScale, newDirX);
            AddComponent(index, component);

            comTransform.SetPosition(component.position);
            comTransform.SetRotation(component.rotation);
            newScale = new Vector3(component.scale.x * newDirX, component.scale.y, component.scale.z);
            comTransform.SetScale(newScale);
            if (hasComView)
            {
                if (comView.HasActorView)
                {
                    comView.ActorView.Transform.position = component.position;
                    comView.ActorView.Transform.rotation = component.rotation;
                    comView.ActorView.Transform.localScale = newScale;
                }
            }
        }
    }

    public sealed partial class LogicMatcher
    {
        private static IMatcher<LogicEntity> _matcherComTransform;

        public static IMatcher<LogicEntity> ComTransform
        {
            get
            {
                if (_matcherComTransform == null)
                {
                    var matcher = (Matcher<LogicEntity>)Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComTransform);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComTransform = matcher;
                }

                return _matcherComTransform;
            }
        }
    }

    public static partial class LogicComponentsLookup
    {
        public static int ComTransform;
    }
}