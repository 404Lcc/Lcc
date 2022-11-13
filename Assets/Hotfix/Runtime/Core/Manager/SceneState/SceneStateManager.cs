using System;
using System.Collections.Generic;
using System.Reflection;

namespace LccHotfix
{
    public class SceneStateName
    {
        public const string None = "";
        public const string Login = "Login";
        public const string Main = "Main";
    }
    public class SceneStateManager : Singleton<SceneStateManager>, IUpdate
    {
        private bool _forceStop;
        private SceneState _current;
        private SceneState _last;

        private Dictionary<string, SceneState> _sceneStateDict = new Dictionary<string, SceneState>();





        public void InitManager()
        {
            foreach (Type item in Manager.Instance.typeDict.Values)
            {
                if (item.IsAbstract) continue;
                object[] att = item.GetCustomAttributes(typeof(SceneStateAttribute), false);
                if (att.Length > 0)
                {
                    SceneStateAttribute sceneStateAttribute = (SceneStateAttribute)att[0];


                    SceneState sceneState = (SceneState)Activator.CreateInstance(item);
                    sceneState.sceneName = sceneStateAttribute.sceneName;

                    List<string> paramList = sceneStateAttribute.paramList;

                    if (paramList.Count % 2 == 0)
                    {
                        for (int i = 1; i < paramList.Count + 1; i++)
                        {
                            if (i % 2 == 1)
                            {
                                string sceneName = paramList[i - 1];
                                sceneStateAttribute.targetNameList.Add(sceneName);
                            }
                            else if (i % 2 == 0)
                            {
                                string methodName = paramList[i - 1];
                                MethodInfo method = item.GetMethod(methodName);
                                sceneStateAttribute.conditionList.Add((Func<bool>)method.CreateDelegate(typeof(Func<bool>), sceneState));
                            }
                        }
                    }


                    for (int i = 0; i < sceneStateAttribute.targetNameList.Count; i++)
                    {
                        StatePipeline statePipeline = new StatePipeline(sceneStateAttribute.targetNameList[i], sceneStateAttribute.conditionList[i]);
                        sceneState.AddPipeline(statePipeline);
                    }


                    _sceneStateDict.Add(sceneStateAttribute.sceneName, sceneState);
                }
            }

            _current = _sceneStateDict[SceneStateName.Login];
            _current.OnEnter();
        }



        public SceneState GetState(string name)
        {
            if (!_sceneStateDict.ContainsKey(name)) return null;
            return _sceneStateDict[name];
        }

        public SceneState GetCurrentState()
        {
            return _current;
        }


        public override void Update()
        {
            base.Update();

            if (_current == null) return;
            if (_forceStop) return;

            string result = _current.CheckTarget();

            if (result != SceneStateName.None)
            {
                SceneState target = _sceneStateDict[result];
                _last = _current;
                _current = target;



                _last.OnExit();
                _current.OnEnter();
            }
            else
            {
                _current.Tick();
            }
        }



    }
}