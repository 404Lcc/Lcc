using System.Collections.Generic;

//////////////////////////////////////////////////////////////////////////
//函数名和方法名是标准的
//内容都是为工程特化定制的

public static class FuncFlag
{
    public static int Sys = 0;
}

public static class MultChangeSetting
{
    static public Dictionary<int, int> mFlag2Priority = new Dictionary<int, int>(); //<flag, priority>

    static public void SetPriority(int flag, int priority)
    {
        mFlag2Priority[flag] = priority;
    }

    static public int GetPriority(int flag)
    {
        if (mFlag2Priority.ContainsKey(flag))
        {
            return mFlag2Priority[flag];
        }

        return 0;
    }

    static MultChangeSetting()
    {
        SetPriority(FuncFlag.Sys, 0);
    }
}