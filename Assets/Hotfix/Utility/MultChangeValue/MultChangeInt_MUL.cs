public class MultChangeInt_MUL : MultChangeValue<int>
{
    public MultChangeInt_MUL()
    {
        mCurValue = mBaseValue = 1;
    }

    public MultChangeInt_MUL(int baseValue) : base(baseValue)
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