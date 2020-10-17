namespace Model
{
    public class DelayDestroy : ObjectBase
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