using System;
using UnityEngine;

namespace ES3Types
{
    [UnityEngine.Scripting.Preserve]
    [ES3PropertiesAttribute("_items", "_size", "_version")]
    public class ES3Type_ArrayList : ES3ObjectType
    {
        public static ES3Type Instance = null;

        public ES3Type_ArrayList() : base(typeof(System.Collections.ArrayList)) { Instance = this; }


        protected override void WriteObject(object obj, ES3Writer writer)
        {
            var instance = (System.Collections.ArrayList)obj;

            writer.WritePrivateField("_items", instance);
            writer.WritePrivateField("_size", instance);
            writer.WritePrivateField("_version", instance);
        }

        protected override void ReadObject<T>(ES3Reader reader, object obj)
        {
            var instance = (System.Collections.ArrayList)obj;
            foreach (string propertyName in reader.Properties)
            {
                switch (propertyName)
                {

                    case "_items":
                        instance = (System.Collections.ArrayList)reader.SetPrivateField("_items", reader.Read<System.Object[]>(), instance);
                        break;
                    case "_size":
                        instance = (System.Collections.ArrayList)reader.SetPrivateField("_size", reader.Read<System.Int32>(), instance);
                        break;
                    case "_version":
                        instance = (System.Collections.ArrayList)reader.SetPrivateField("_version", reader.Read<System.Int32>(), instance);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }

        protected override object ReadObject<T>(ES3Reader reader)
        {
            var instance = new System.Collections.ArrayList();
            ReadObject<T>(reader, instance);
            return instance;
        }
    }


    public class ES3UserType_ArrayListArray : ES3ArrayType
    {
        public static ES3Type Instance;

        public ES3UserType_ArrayListArray() : base(typeof(System.Collections.ArrayList[]), ES3Type_ArrayList.Instance)
        {
            Instance = this;
        }
    }
}