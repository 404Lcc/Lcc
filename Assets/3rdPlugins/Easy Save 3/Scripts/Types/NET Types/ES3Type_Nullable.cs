using System;
using ES3Internal;

namespace ES3Types
{
    [UnityEngine.Scripting.Preserve]
    public class ES3Type_Nullable : ES3Type
    {
        public ES3Type argumentES3Type;
        public Type genericArgument;

        ES3Reflection.ES3ReflectedMember hasValueProperty;
        ES3Reflection.ES3ReflectedMember valueProperty;

        public ES3Type_Nullable() : base(typeof(Nullable<>))
        {
        }

        public ES3Type_Nullable(Type type) : base(type)
        {
            hasValueProperty = ES3Reflection.GetES3ReflectedProperty(type, "HasValue");
            valueProperty = ES3Reflection.GetES3ReflectedProperty(type, "Value");

            genericArgument = ES3Reflection.GetGenericArguments(type)[0];
            argumentES3Type = ES3TypeMgr.GetOrCreateES3Type(genericArgument, false);
            isUnsupported = (argumentES3Type == null);
        }

        public override void Write(object obj, ES3Writer writer)
        {
            var hasValue = (bool)hasValueProperty.GetValue(obj);
            writer.WriteProperty("HasValue", hasValue, ES3Type_bool.Instance);

            if(hasValue)
            {
                var value = valueProperty.GetValue(obj);
                writer.WriteProperty("Value", value, argumentES3Type);
            }
        }

        public override object Read<T>(ES3Reader reader)
        {
            var hasValue = reader.ReadProperty<bool>(ES3Type_bool.Instance);

            if(!hasValue)
            {
                // Call parameterless constructor to set it as null.
                var constructor = ES3Reflection.GetConstructor(type, new Type[0]);
                return constructor.Invoke(new object[0]);
            }
            else
            {
                var value = reader.ReadProperty<object>(argumentES3Type);
                var constructor = ES3Reflection.GetConstructor(type, new Type[] { genericArgument });
                return constructor.Invoke(new object[] { value });
            }
        }
    }
}