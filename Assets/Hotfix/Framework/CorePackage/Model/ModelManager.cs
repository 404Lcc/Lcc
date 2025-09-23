using System;
using System.Collections.Generic;

namespace LccHotfix
{
    internal class ModelManager : Module, IModelService
    {
        public Dictionary<Type, ModelTemplate> modelDict = new Dictionary<Type, ModelTemplate>();

        public ModelManager()
        {

            foreach (Type item in Main.CodeTypesService.GetTypes(typeof(ModelAttribute)))
            {
                object[] atts = item.GetCustomAttributes(typeof(ModelAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    ModelAttribute modelAttribute = (ModelAttribute)atts[0];

                    ModelTemplate model = (ModelTemplate)Activator.CreateInstance(item);

                    modelDict.Add(model.GetType(), model);
                }
            }
        }


        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            foreach (var item in modelDict.Values)
            {
                item.OnDestroy();
            }
            modelDict.Clear();
        }
        
        public void Init()
        {
            foreach (var item in modelDict.Values)
            {
                item.Init();
            }
        }
        
        public T GetModel<T>() where T : ModelTemplate
        {
            if (modelDict.ContainsKey(typeof(T)))
            {
                return (T)modelDict[typeof(T)];
            }
            return null;
        }
        
        public object GetModel(Type type)
        {
            if (modelDict.ContainsKey(type))
            {
                return modelDict[type];
            }
            return null;
        }

    }
}