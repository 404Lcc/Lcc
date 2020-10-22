namespace LccModel
{
    public class Delay : AObjectBase
    {
        public float time = 1;
        public override void Start()
        {
            gameObject.SetActive(false);
            Invoke("DelayFunction", time);
        }
        public void DelayFunction()
        {
            gameObject.SetActive(true);
        }
    }
}