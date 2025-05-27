using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ES3Internal;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	public class ES3IListGenericType : ES3CollectionType
	{
		public ES3IListGenericType(Type type) : base(type){}
		public ES3IListGenericType(Type type, ES3Type elementType) : base(type, elementType){}

        public override void Write(object obj, ES3Writer writer, ES3.ReferenceMode memberReferenceMode)
        {
            if (obj == null) { writer.WriteNull(); return; };

            //********************************************************************************************************************
            IEnumerable enumerable = obj as IEnumerable;
			var list = enumerable.Cast<object>();
            //********************************************************************************************************************

            if (elementType == null)
                throw new ArgumentNullException("ES3Type argument cannot be null.");

			//writer.StartWriteCollection();
            int i = 0;
            foreach (object item in list)
            {
                writer.StartWriteCollectionItem(i);
                writer.Write(item, elementType, memberReferenceMode);
                writer.EndWriteCollectionItem(i);
                i++;
            }

            //writer.EndWriteCollection();
        }

        public override object Read<T>(ES3Reader reader)
		{
            return Read(reader);

            /*var list = new List<T>();
			if(!ReadICollection<T>(reader, list, elementType))
				return null;
			return list;*/
        }

		public override void ReadInto<T>(ES3Reader reader, object obj)
		{
			ReadICollectionInto(reader, (ICollection)obj, elementType);
		}

		public override object Read(ES3Reader reader)
		{
            //var instance = (IList)ES3Reflection.CreateInstance(type);

            //********************************************************************************************************************
            // Works to create my ObservableList<PreviewModel>
            var myList = ES3Reflection.CreateInstance(type);
            var enumerable = (IEnumerable)myList;
            var instance1 = enumerable.Cast<object>(); // Good ObservableList<PreviewModel>                                                       
            var instance = instance1 as IList<object>; // Casting results in Null

            // This would probably work, but dynamic imposes .NET Framework restrictions 
            //dynamic instance = enumerable.Cast<object>(); // Good ObservableList<PreviewMod

            // Also works to create my ObservableList<PreviewModel>
            var instance10 = ES3Reflection.CreateInstance(ES3Reflection.MakeGenericType(type.GetGenericTypeDefinition(), elementType.type));
            //var instance = instance10 as IList<object>; // Casting results in Null
            //********************************************************************************************************************

            if (reader.StartReadCollection())
				return null;

			// Iterate through each character until we reach the end of the array.
			while(true)
			{
				if(!reader.StartReadCollectionItem())
					break;
				instance.Add(reader.Read<object>(elementType));

				if(reader.EndReadCollectionItem())
					break;
			}

			reader.EndReadCollection();

			return instance;
		}

		public override void ReadInto(ES3Reader reader, object obj)
		{
			var collection = (IList)obj;

			if(reader.StartReadCollection())
				throw new NullReferenceException("The Collection we are trying to load is stored as null, which is not allowed when using ReadInto methods.");

			int itemsLoaded = 0;

			// Iterate through each item in the collection and try to load it.
			foreach(var item in collection)
			{
				itemsLoaded++;

				if(!reader.StartReadCollectionItem())
					break;

				reader.ReadInto<object>(item, elementType);

				// If we find a ']', we reached the end of the array.
				if(reader.EndReadCollectionItem())
					break;

				// If there's still items to load, but we've reached the end of the collection we're loading into, throw an error.
				if(itemsLoaded == collection.Count)
					throw new IndexOutOfRangeException("The collection we are loading is longer than the collection provided as a parameter.");
			}

			// If we loaded fewer items than the parameter collection, throw index out of range exception.
			if(itemsLoaded != collection.Count)
				throw new IndexOutOfRangeException("The collection we are loading is shorter than the collection provided as a parameter.");

			reader.EndReadCollection();
		}
	}
}