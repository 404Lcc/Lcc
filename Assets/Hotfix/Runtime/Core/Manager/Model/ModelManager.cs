using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class ModelManager : AObjectBase
    {
        public static ModelManager Instance { get; set; }
        public Dictionary<Type, ModelTemplate> modelDict = new Dictionary<Type, ModelTemplate>();

        public override void Awake()
        {
            base.Awake();

            Instance = this;

            foreach (Type item in Manager.Instance.GetTypesByAttribute(typeof(ModelAttribute)))
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
        public override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;


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