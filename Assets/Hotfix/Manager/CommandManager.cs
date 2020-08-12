using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Hotfix
{
    public class CommandManager : MonoBehaviour
    {
        public List<CommandData> commanddatas;
        public int index;
        public CommandType commandtype;
        void Awake()
        {
            InitManager();
        }
        void Start()
        {
        }
        void Update()
        {
            switch (commandtype)
            {
                case CommandType.Automatic:
                    AutomaticExcute();
                    break;
                case CommandType.Manually:
                    ManuallyExcute();
                    break;
            }
        }
        public void InitManager()
        {
            commanddatas = new List<CommandData>();
        }
        public void AddCommand(CommandData commanddata)
        {
            if (commanddata == null) return;
            CommandData target;
            switch (commanddata.variety)
            {
                default:
                    target = new CommandData();
                    break;
            }
            FieldInfo[] fieldinfos = target.GetType().GetFields();
            foreach (FieldInfo item in fieldinfos)
            {
                item.SetValue(target, item.GetValue(commanddata));
            }
            commanddatas.Add(target);
        }
        public void AddCommands(CommandData[] commanddatas)
        {
            if (commanddatas == null) return;
            List<CommandData> targetlist = new List<CommandData>();
            foreach (CommandData item in commanddatas)
            {
                CommandData target;
                switch (item.variety)
                {
                    default:
                        target = new CommandData();
                        break;
                }
                FieldInfo[] fieldinfos = target.GetType().GetFields();
                foreach (FieldInfo fieldinfoitem in fieldinfos)
                {
                    fieldinfoitem.SetValue(target, fieldinfoitem.GetValue(item));
                }
                targetlist.Add(target);
            }
            this.commanddatas.AddRange(targetlist.ToArray());
        }
        public void SetCommandType(CommandType type)
        {
            commandtype = type;
        }
        public void AutomaticExcute()
        {
            if (index < commanddatas.Count)
            {
                Excute();
                Next();
            }
        }
        public void ManuallyExcute()
        {
            if (index < commanddatas.Count)
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
                if (index < commanddatas.Count)
                {
                    commanddatas[index].bcondition = value;
                }
            }
        }
        public bool GetCondition
        {
            get
            {
                if (index < commanddatas.Count)
                {
                    return commanddatas[index].bcondition;
                }
                return true;
            }
        }
        public bool SetFinish
        {
            set
            {
                if (index < commanddatas.Count)
                {
                    commanddatas[index].bfinish = value;
                }
            }
        }
        public bool GetFinish
        {
            get
            {
                if (index < commanddatas.Count)
                {
                    return commanddatas[index].bfinish;
                }
                return true;
            }
        }
        public void Excute()
        {
            if (commanddatas[index].bcondition)
            {
                if (!commanddatas[index].bexcute)
                {
                    commanddatas[index].Execute();
                    commanddatas[index].bexcute = true;
                }
            }
        }
        public void Next()
        {
            if (commanddatas[index].bexcute && commanddatas[index].bfinish)
            {
                index += 1;
                if (index >= commanddatas.Count)
                {
                    commanddatas.Clear();
                    index = 0;
                    commandtype = CommandType.Automatic;
                }
            }
        }
        public bool IsFinishAllCommand()
        {
            if (commanddatas.Count == 0) return true;
            return false;
        }
    }
}