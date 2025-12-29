public class MultChangeFloat_MUL : MultChangeValue<float>
{
    public MultChangeFloat_MUL()
    {
        mCurValue = mBaseValue = 1f;
    }

    public MultChangeFloat_MUL(float baseValue) : base(baseValue)
    {
    }

    protected override void CalcuCurValue()
    {
        mCurValue = mBaseValue;
        foreach (var v in mValueChangeList)
        {
            mCurValue *= v.Value;
        }
    }
}