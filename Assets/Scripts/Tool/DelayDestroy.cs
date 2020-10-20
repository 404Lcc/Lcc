namespace Model
{
    public class DelayDestroy : AObjectBase
    {
        public float time = 1;
        public override void Start()
        {
            Invoke("DelayFunction", time);
        }
        public void DelayFunction()
        {
            gameObject.SafeDestroy();
        }
    }
}