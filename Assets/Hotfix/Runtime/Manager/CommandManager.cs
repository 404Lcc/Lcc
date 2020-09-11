using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Hotfix
{
    public class CommandManager : Singleton<CommandManager>
    {
        public List<CommandData> commandDataList = new List<CommandData>();
        public int index;
        public CommandType commandType;
        public override void Update()
        {
            switch (commandType)
            {
                case CommandType.Automatic:
                    AutomaticExcute();
                    break;
                case CommandType.Manually:
                    ManuallyExcute();
                    break;
            }
        }
        public void AddCommand(CommandData commandData)
        {
            if (commandData == null) return;
            CommandData target;
            switch (commandData.variety)
            {
                default:
                    target = new CommandData();
                    break;
            }
            FieldInfo[] fieldInfos = target.GetType().GetFields();
            foreach (FieldInfo item in fieldInfos)
            {
                item.SetValue(target, item.GetValue(commandData));
            }
            commandDataList.Add(target);
        }
        public void AddCommands(CommandData[] commandDatas)
        {
            if (commandDatas == null) return;
            List<CommandData> targetList = new List<CommandData>();
            foreach (CommandData item in commandDatas)
            {
                CommandData target;
                switch (item.variety)
                {
                    default:
                        target = new CommandData();
                        break;
                }
                FieldInfo[] fieldInfos = target.GetType().GetFields();
                foreach (FieldInfo fieldinfoitem in fieldInfos)
                {
                    fieldinfoitem.SetValue(target, fieldinfoitem.GetValue(item));
                }
                targetList.Add(target);
            }
            commandDataList.AddRange(targetList.ToArray());
        }
        public void SetCommandType(CommandType type)
        {
            commandType = type;
        }
        public void AutomaticExcute()
        {
            if (index < commandDataList.Count)
            {
                Excute();
                Next();
            }
        }
        public void ManuallyExcute()
        {
            if (index < commandDataList.Count)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                {
                    Excute();
                }
                Next();
            }
        }
        public bool SetCondition
        {
            set
            {
                if (index < commandDataList.Count)
                {
                    commandDataList[index].condition = value;
                }
            }
        }
        public bool GetCondition
        {
            get
            {
                if (index < commandDataList.Count)
                {
                    return commandDataList[index].condition;
                }
                return true;
            }
        }
        public bool SetFinish
        {
            set
            {
                if (index < commandDataList.Count)
                {
                    commandDataList[index].finish = value;
                }
            }
        }
        public bool GetFinish
        {
            get
            {
                if (index < commandDataList.Count)
                {
                    return commandDataList[index].finish;
                }
                return true;
            }
        }
        public void Excute()
        {
            if (commandDataList[index].condition)
            {
                if (!commandDataList[index].excute)
                {
                    commandDataList[index].Execute();
                    commandDataList[index].excute = true;
                }
            }
        }
        public void Next()
        {
            if (commandDataList[index].excute && commandDataList[index].finish)
            {
                index += 1;
                if (index >= commandDataList.Count)
                {
                    commandDataList.Clear();
                    index = 0;
                    commandType = CommandType.Automatic;
                }
            }
        }
        public bool IsFinishAllCommand()
        {
            if (commandDataList.Count == 0) return true;
            return false;
        }
    }
}