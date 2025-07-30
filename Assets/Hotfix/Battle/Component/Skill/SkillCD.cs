namespace LccHotfix
{
    public class SkillCD
    {
        private SkillData _data;
        private float _coldTime;

        public bool isCoolable;
        public bool isCooling;
        public float cdTimer;


        public int SkillId => _data.SkillId;
        public float ColdTime
        {
            get
            {
                return _coldTime;
            }
            set
            {
                _coldTime = value;
            }
        }

        public void Init(SkillData data)
        {
            _data = data;
            _coldTime = data.ColdTime;
            isCoolable = true;
            isCooling = false;
            cdTimer = 0;
        }

        public void EnterCD()
        {
            if (!isCooling)
            {
                cdTimer = 0;
                isCooling = true;
            }
        }
    }
}