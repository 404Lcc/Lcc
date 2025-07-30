using System;
using System.Collections.Generic;
using System.Reflection;

namespace LccHotfix
{
    public interface ICommandService : IService
    {
        bool SetIsCondition { set; }
        bool GetIsCondition { get; }
        bool SetIsFinish { set; }

        bool GetIsFinish { get; }

        void AddCommand(CommandData commandData);
        void AddCommands(CommandData[] commandDatas);
        void SetCommandType(CommandType type);
        void AutomaticExcute();
        void ManuallyExcute();
        void Excute();
        void Next();
        bool IsFinishAllCommand();
    }
}