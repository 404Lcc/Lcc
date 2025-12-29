public class MultChangeInt_ADD : MultChangeValue<int>
{
    public MultChangeInt_ADD()
    {
        mCurValue = mBaseValue = 0;
    }

    public MultChangeInt_ADD(int baseValue) : base(baseValue)
    {
    }

    protected override void CalcuCurValue()
    {
        mCurValue = mBaseValue;
        foreach (var v in mValueChangeList)
        {
            mCurValue += v.Value;
        }
    }
}