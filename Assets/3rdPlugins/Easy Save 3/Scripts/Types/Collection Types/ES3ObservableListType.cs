#if CORE_RP
using ES3Internal;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace ES3Types
{
    public class ES3ObservableListType : ES3GenericType
    {
        public ES3ObservableListType(Type type) : base(type){}
        public ES3ObservableListType() : base(typeof(ObservableList<>)){}

        public override void Write(object obj, ES3Writer writer)
        {
            var list = ES3Reflection.GetField(type, "m_List").GetValue(obj);
            writer.WriteProperty<object>("m_List", list);
        }

        public override object Read<T>(ES3Reader reader)
        {
            var listType = ES3Reflection.MakeGenericType(typeof(List<>), genericArguments[0]);
            var list = ES3Reflection.GetMethod(typeof(ES3Reader), "ReadProperty", new Type[] { listType }, new Type[0]).Invoke(reader);

            var observableList = ES3Reflection.CreateInstance(type);
            reader.SetPrivateField("m_List", list, observableList);

            return observableList;
        }
    }
}
#endif