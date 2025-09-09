using UnityEngine;

namespace LccHotfix
{
    public class ComORCA : LogicComponent
    {
        public IORCA orca;

        public override void PostInitialize(LogicEntity owner)
        {
            base.PostInitialize(owner);
        }

        public override void Dispose()
        {
            base.Dispose();
            orca.Dispose();
        }

        public void Update()
        {
            orca.Update();
        }
    }

    public partial class LogicEntity
    {
        public ComORCA ComORCA
        {
            get { return (ComORCA)GetComponent(LogicComponentsLookup.ComORCA); }
        }

        public bool hasComORCA
        {
            get { return HasComponent(LogicComponentsLookup.ComORCA); }
        }

        public void ORCAMove(Vector3 dir, float speed)
        {
            if (!hasComTransform)
            {
                return;
            }

            if (HasComponent(LogicComponentsLookup.ComORCA))
            {
                ComORCA.orca.SetDir(dir);
                ComORCA.orca.SetSpeed(speed);
                ReplaceComponent(LogicComponentsLookup.ComORCA, ComORCA);
            }
            else
            {
                var index = LogicComponentsLookup.ComORCA;
                var component = (ComORCA)CreateComponent(index, typeof(ComORCA));
                component.orca = new ORCA3D();
                component.orca.InitOrca(this);
                component.orca.SetDir(dir);
                component.orca.SetSpeed(speed);
                AddComponent(index, component);
            }
        }

        public void StopORCA()
        {
            if (HasComponent(LogicComponentsLookup.ComORCA))
            {
                ComORCA.orca.StopORCA();
            }
        }

        public void RemoveComORCA()
        {
            RemoveComponent(LogicComponentsLookup.ComORCA);
        }
    }

    public sealed partial class LogicMatcher
    {
        private static Entitas.IMatcher<LogicEntity> _matcherComORCA;

        public static Entitas.IMatcher<LogicEntity> ComORCA
        {
            get
            {
                if (_matcherComORCA == null)
                {
                    var matcher = (Entitas.Matcher<LogicEntity>)Entitas.Matcher<LogicEntity>.AllOf(LogicComponentsLookup.ComORCA);
                    matcher.ComponentNames = LogicComponentsLookup.componentNames;
                    _matcherComORCA = matcher;
                }

                return _matcherComORCA;
            }
        }
    }

    public static partial class LogicComponentsLookup
    {
        public static int ComORCA;
    }
}