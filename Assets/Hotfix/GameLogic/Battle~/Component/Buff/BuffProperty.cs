using cfg;

namespace LccHotfix
{
    public class BuffProperty
    {
        protected int _level = 0;
        protected int _propType;
        protected long _entityId = 0;
        protected int _key = 0;

        public void Init(int level, int propType, long entityId, int key)
        {
            _level = level;
            _propType = propType;
            _entityId = entityId;
            _key = key;
        }

        public virtual void OnActive()
        {

        }

        public virtual void OnDeactive()
        {

        }

        public virtual void Dispose()
        {

        }
    }

    public class FloatBuffProperty : BuffProperty
    {
        protected float _value = 0;
        protected bool _isPercent = false;

        public void Init(int level, int propType, long entityId, int key, float value, bool isPercent)
        {
            base.Init(level, propType, entityId, key);

            _value = value;
            _isPercent = isPercent;
        }

        public override void OnActive()
        {
            var entity = EntityUtility.GetEntity(_entityId);

            if (entity == null)
                return;

            if (!entity.hasComProperty)
                return;

            var comProperty = entity.comProperty;
            float addValue = _value * _level;
            if (_isPercent)
                addValue = comProperty.GetBaseFloat((ValuePropertyType)_propType) * _value * _level * 0.01f;

            comProperty.SetSubFloat((ValuePropertyType)_propType, _key, addValue);
        }

        public override void OnDeactive()
        {
            var entity = EntityUtility.GetEntity(_entityId);

            if (entity == null)
                return;

            if (!entity.hasComProperty)
                return;

            var comProperty = entity.comProperty;
            float addValue = _value * _level;
            if (_isPercent)
                addValue = comProperty.GetBaseFloat((ValuePropertyType)_propType) * _value * _level * 0.01f;

            comProperty.ClearSubFloat((ValuePropertyType)_propType, _key);
        }
    }

    public class BoolBuffProperty : BuffProperty
    {
        protected bool _value = false;

        public void Init(int level, int propType, long entityId, int key, bool value)
        {
            base.Init(level, propType, entityId, key);

            _value = value;
        }

        public override void OnActive()
        {
            var entity = EntityUtility.GetEntity(_entityId);

            if (entity == null)
                return;

            if (!entity.hasComProperty)
                return;

            var comProperty = entity.comProperty;
            comProperty.SetSubBool((BoolPropertyType)_propType, _key, _value);
        }

        public override void OnDeactive()
        {
            var entity = EntityUtility.GetEntity(_entityId);

            if (entity == null)
                return;

            if (!entity.hasComProperty)
                return;

            var comProperty = entity.comProperty;
            comProperty.ClearBool((BoolPropertyType)_propType, _key);
        }
    }
}