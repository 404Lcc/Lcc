using System.Collections.Generic;
using UnityEngine;

namespace Hotfix
{
    public class CommandManager : MonoBehaviour
    {
        public List<CommandData> commanddatalist;
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
            commanddatalist = new List<CommandData>();
        }
        public void AddCommand(CommandData commanddata)
        {
            if (commanddata == null) return;
            commanddatalist.Add(commanddata);
        }
        public void AddCommands(CommandData[] commanddatas)
        {
            if (commanddatas == null) return;
            commanddatalist.AddRange(commanddatas);
        }
        public void SetCommandType(CommandType type)
        {
            commandtype = type;
        }
        public void AutomaticExcute()
        {
            if (index < commanddatalist.Count)
            {
                Excute();
                Next();
            }
        }
        public void ManuallyExcute()
        {
            if (index < commanddatalist.Count)
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
                if (index < commanddatalist.Count)
                {
                    commanddatalist[index].bcondition = value;
                }
            }
        }
        public bool GetCondition
        {
            get
            {
                if (index < commanddatalist.Count)
                {
                    return commanddatalist[index].bcondition;
                }
                return true;
            }
        }
        public bool SetFinish
        {
            set
            {
                if (index < commanddatalist.Count)
                {
                    commanddatalist[index].bfinish = value;
                }
            }
        }
        public bool GetFinish
        {
            get
            {
                if (index < commanddatalist.Count)
                {
                    return commanddatalist[index].bfinish;
                }
                return true;
            }
        }
        public void Excute()
        {
            if (commanddatalist[index].bcondition)
            {
                if (!commanddatalist[index].bexcute)
                {
                    commanddatalist[index].Execute();
                    commanddatalist[index].bexcute = true;
                }
            }
        }
        public void Next()
        {
            if (commanddatalist[index].bexcute && commanddatalist[index].bfinish)
            {
                index += 1;
                if (index >= commanddatalist.Count)
                {
                    commanddatalist.Clear();
                    index = 0;
                    commandtype = CommandType.Automatic;
                }
            }
        }
        public bool IsFinishAllCommand()
        {
            if (commanddatalist.Count == 0) return true;
            return false;
        }
    }
}