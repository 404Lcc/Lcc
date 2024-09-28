using System;
using System.Collections.Generic;

namespace LccHotfix
{
    internal class ModelManager : Module
    {
        public static ModelManager Instance { get; } = Entry.GetModule<ModelManager>();
        public Dictionary<Type, ModelTemplate> modelDict = new Dictionary<Type, ModelTemplate>();

        public ModelManager()
        {

            foreach (Type item in CodeTypesManager.Instance.GetTypes(typeof(ModelAttribute)))
            {
                object[] atts = item.GetCustomAttributes(typeof(ModelAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    ModelAttribute modelAttribute = (ModelAttribute)atts[0];

                    ModelTemplate model = (ModelTemplate)Activator.CreateInstance(item);

                    modelDict.Add(model.GetType(), model);
                }
            }
            foreach (var item in modelDict.Values)
            {
                item.Init();
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

        public T GetModel<T>() where T : ModelTemplate
        {
            if (modelDict.ContainsKey(typeof(T)))
            {
                return (T)modelDict[typeof(T)];
            }
            return null;
        }

    }
}