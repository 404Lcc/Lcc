﻿using LccModel;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LccHotfix
{
    public class CommandManager : AObjectBase, IUpdate
    {
        public static CommandManager Instance { get; set; }

        public List<CommandData> commandDataList = new List<CommandData>();
        public int index;
        public CommandType commandType;
        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            commandDataList.Clear();
            index = 0;
            commandType = CommandType.Automatic;

            Instance = null;
        }
        public void Update()
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
            switch (commandData.dataType)
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
        public bool SetIsCondition
        {
            set
            {
                if (index < commandDataList.Count)
                {
                    commandDataList[index].isCondition = value;
                }
            }
        }
        public bool GetIsCondition
        {
            get
            {
                if (index < commandDataList.Count)
                {
                    return commandDataList[index].isCondition;
                }
                return true;
            }
        }
        public bool SetIsFinish
        {
            set
            {
                if (index < commandDataList.Count)
                {
                    commandDataList[index].isFinish = value;
                }
            }
        }
        public bool GetIsFinish
        {
            get
            {
                if (index < commandDataList.Count)
                {
                    return commandDataList[index].isFinish;
                }
                return true;
            }
        }
        public void Excute()
        {
            if (commandDataList[index].isCondition)
            {
                if (!commandDataList[index].isExcute)
                {
                    commandDataList[index].Execute();
                    commandDataList[index].isExcute = true;
                }
            }
        }
        public void Next()
        {
            if (commandDataList[index].isExcute && commandDataList[index].isFinish)
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