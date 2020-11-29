using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LccHotfix
{
    public class CommandManager : Singleton<CommandManager>
    {
        public List<CommandData> commandList = new List<CommandData>();
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
        public void AddCommand(CommandData command)
        {
            if (command == null) return;
            CommandData target;
            switch (command.dataType)
            {
                default:
                    target = new CommandData();
                    break;
            }
            FieldInfo[] fieldInfos = target.GetType().GetFields();
            foreach (FieldInfo item in fieldInfos)
            {
                item.SetValue(target, item.GetValue(command));
            }
            commandList.Add(target);
        }
        public void AddCommands(CommandData[] commands)
        {
            if (commands == null) return;
            List<CommandData> targetList = new List<CommandData>();
            foreach (CommandData item in commands)
            {
                CommandData target;
                switch (item.dataType)
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
            commandList.AddRange(targetList.ToArray());
        }
        public void SetCommandType(CommandType type)
        {
            commandType = type;
        }
        public void AutomaticExcute()
        {
            if (index < commandList.Count)
            {
                Excute();
                Next();
            }
        }
        public void ManuallyExcute()
        {
            if (index < commandList.Count)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                {
                    Excute();
                }
                Next();
            }
        }
        public bool SetIsCondition
        {
            set
            {
                if (index < commandList.Count)
                {
                    commandList[index].isCondition = value;
                }
            }
        }
        public bool GetIsCondition
        {
            get
            {
                if (index < commandList.Count)
                {
                    return commandList[index].isCondition;
                }
                return true;
            }
        }
        public bool SetIsFinish
        {
            set
            {
                if (index < commandList.Count)
                {
                    commandList[index].isFinish = value;
                }
            }
        }
        public bool GetIsFinish
        {
            get
            {
                if (index < commandList.Count)
                {
                    return commandList[index].isFinish;
                }
                return true;
            }
        }
        public void Excute()
        {
            if (commandList[index].isCondition)
            {
                if (!commandList[index].isExcute)
                {
                    commandList[index].Execute();
                    commandList[index].isExcute = true;
                }
            }
        }
        public void Next()
        {
            if (commandList[index].isExcute && commandList[index].isFinish)
            {
                index += 1;
                if (index >= commandList.Count)
                {
                    commandList.Clear();
                    index = 0;
                    commandType = CommandType.Automatic;
                }
            }
        }
        public bool IsFinishAllCommand()
        {
            if (commandList.Count == 0) return true;
            return false;
        }
    }
}