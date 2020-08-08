using System.Collections.Generic;
using UnityEngine;

namespace Hotfix
{
    public class CommandManager : MonoBehaviour
    {
        public List<Command> commandlist;
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
            commandlist = new List<Command>();
        }
        public void AddCommand(Command command)
        {
            if (command == null) return;
            commandlist.Add(command);
        }
        public void AddCommands(Command[] commands)
        {
            if (commands == null) return;
            commandlist.AddRange(commands);
        }
        public void SetCommandType(CommandType type)
        {
            commandtype = type;
        }
        public void AutomaticExcute()
        {
            if (index < commandlist.Count)
            {
                Excute();
                Next();
            }
        }
        public void ManuallyExcute()
        {
            if (index < commandlist.Count)
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
                if (index < commandlist.Count)
                {
                    commandlist[index].bcondition = value;
                }
            }
        }
        public bool GetCondition
        {
            get
            {
                if (index < commandlist.Count)
                {
                    return commandlist[index].bcondition;
                }
                return true;
            }
        }
        public bool SetFinish
        {
            set
            {
                if (index < commandlist.Count)
                {
                    commandlist[index].bfinish = value;
                }
            }
        }
        public bool GetFinish
        {
            get
            {
                if (index < commandlist.Count)
                {
                    return commandlist[index].bfinish;
                }
                return true;
            }
        }
        public void Excute()
        {
            if (commandlist[index].bcondition)
            {
                if (!commandlist[index].bexcute)
                {
                    commandlist[index].Execute();
                    commandlist[index].bexcute = true;
                }
            }
        }
        public void Next()
        {
            if (commandlist[index].bexcute && commandlist[index].bfinish)
            {
                index += 1;
                if (index >= commandlist.Count)
                {
                    commandlist.Clear();
                    index = 0;
                    commandtype = CommandType.Automatic;
                }
            }
        }
        public bool IsFinishAllCommand()
        {
            if (commandlist.Count == 0) return true;
            return false;
        }
    }
}