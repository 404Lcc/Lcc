using System.Collections;
using System.Collections.Generic;
namespace CoreGame
{
    //每个object的生命周期都应该精确知道，Destruct()就相当于主动析构时的析构函数
    public interface IDestruct
    {
        void Destruct();
    }
}