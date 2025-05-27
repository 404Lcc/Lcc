using System;
using UnityEngine;
using System.Collections.Generic;
using ES3Internal;

namespace ES3Types
{
    [UnityEngine.Scripting.Preserve]
    [ES3PropertiesAttribute("layer", "isStatic", "tag", "name", "hideFlags", "children", "components")]
    public class ES3Type_GameObject : ES3UnityObjectType
    {
        private const string prefabPropertyName = "es3Prefab";
        private const string transformPropertyName = "transformID";
        public static ES3Type Instance = null;
        public bool saveChildren = false;

        public ES3Type_GameObject() : base(typeof(UnityEngine.GameObject)) { Instance = this; }

        public override void WriteObject(object obj, ES3Writer writer, ES3.ReferenceMode mode)
        {
            if (WriteUsingDerivedType(obj, writer))
                return;
            var instance = (UnityEngine.GameObject)obj;

            var mgr = ES3ReferenceMgrBase.GetManagerFromScene(instance.scene);

            if (mode != ES3.ReferenceMode.ByValue)
            {
                writer.WriteRef(instance, ES3ReferenceMgrBase.referencePropertyName, mgr);

                if (mode == ES3.ReferenceMode.ByRef)
                    return;

                var es3Prefab = instance.GetComponent<ES3Prefab>();
                if (es3Prefab != null)
                    writer.WriteProperty(prefabPropertyName, es3Prefab, ES3Type_ES3PrefabInternal.Instance);

                // Write the ID of this Transform so we can assign it's ID when we load.
                writer.WriteRef(instance.transform, transformPropertyName, mgr);
            }

            var es3AutoSave = instance.GetComponent<ES3AutoSave>();

            if(es3AutoSave == null || es3AutoSave.saveLayer)
                writer.WriteProperty("layer", instance.layer, ES3Type_int.Instance);
            if (es3AutoSave == null || es3AutoSave.saveTag)
                writer.WriteProperty("tag", instance.tag, ES3Type_string.Instance);
            if (es3AutoSave == null || es3AutoSave.saveName)
                writer.WriteProperty("name", instance.name, ES3Type_string.Instance);
            if (es3AutoSave == null || es3AutoSave.saveHideFlags)
                writer.WriteProperty("hideFlags", instance.hideFlags);
            if (es3AutoSave == null || es3AutoSave.saveActive)
                writer.WriteProperty("active", instance.activeSelf);

            if ((es3AutoSave == null && saveChildren) || (es3AutoSave != null && es3AutoSave.saveChildren))
                    writer.WriteProperty("children", GetChildren(instance), ES3.ReferenceMode.ByRefAndValue);

            List<Component> components;

            var es3GameObject = instance.GetComponent<ES3GameObject>();

            // If there's an ES3AutoSave attached and Components are marked to be saved, save these.
            if (es3AutoSave != null)
            {
                es3AutoSave.componentsToSave.RemoveAll(c => c == null);
                components = es3AutoSave.componentsToSave;
            }
            // If there's an ES3GameObject attached, save these.
            else if (es3GameObject != null)
            {
                es3GameObject.components.RemoveAll(c => c == null);
                components = es3GameObject.components;
            }
            // Otherwise, only save explicitly-supported Components, /*or those explicitly marked as Serializable*/.
            else
            {
                components = new List<Component>();
                foreach (var component in instance.GetComponents<Component>())
                    if (component != null && ES3TypeMgr.GetES3Type(component.GetType()) != null)
                        components.Add(component);
            }

            if(components != null & components.Count > 0)
                writer.WriteProperty("components", components, ES3.ReferenceMode.ByRefAndValue);
        }

        protected override object ReadObject<T>(ES3Reader reader)
        {
            UnityEngine.Object obj = null;
            var refMgr = ES3ReferenceMgrBase.Current;
            long id = 0;

            // Read the intial properties regarding the instance we're loading.
            while (true)
            {
                if (refMgr == null)
                    throw new InvalidOperationException($"An Easy Save 3 Manager is required to save references. To add one to your scene, exit playmode and go to Tools > Easy Save 3 > Add Manager to Scene. Object being saved by reference is {obj.GetType()} with name {obj.name}.");

                var propertyName = ReadPropertyName(reader);

                if (propertyName == ES3Type.typeFieldName)
                    return ES3TypeMgr.GetOrCreateES3Type(reader.ReadType()).Read<T>(reader);
                else if (propertyName == ES3ReferenceMgrBase.referencePropertyName)
                {
                    id = reader.Read_ref();
                    obj = refMgr.Get(id, true);
                }
                else if (propertyName == transformPropertyName)
                {
                    // Now load the Transform's ID and assign it to the Transform of our object.
                    long transformID = reader.Read_ref();
                    if (obj == null)
                        obj = CreateNewGameObject(refMgr, id);
                    refMgr.Add(((GameObject)obj).transform, transformID);
                }
                else if (propertyName == prefabPropertyName)
                {
                    if (obj != null || ES3ReferenceMgrBase.Current == null)
                    {
                        reader.ReadInto<GameObject>(obj); // ReadInto to apply the prefab references.
                    }
                    else
                    {
                        obj = reader.Read<GameObject>(ES3Type_ES3PrefabInternal.Instance);
                        ES3ReferenceMgrBase.Current.Add(obj, id);
                    }
                }
                else if (propertyName == null)
                {
                    /*if (obj == null)
                        obj = CreateNewGameObject(refMgr, id);*/
                    return obj;
                }
                else
                {
                    reader.overridePropertiesName = propertyName;
                    break;
                }
            }

            if (obj == null)
                obj = CreateNewGameObject(refMgr, id);

            ReadInto<T>(reader, obj);
            return obj;
        }

        protected override void ReadObject<T>(ES3Reader reader, object obj)
        {
            var instance = (UnityEngine.GameObject)obj;

            foreach (string propertyName in reader.Properties)
            {
                switch (propertyName)
                {
                    case ES3ReferenceMgrBase.referencePropertyName:
                        ES3ReferenceMgr.Current.Add(instance, reader.Read_ref());
                        break;
                    case "prefab":
                        break;
                    case "layer":
                        instance.layer = reader.Read<System.Int32>(ES3Type_int.Instance);
                        break;
                    case "tag":
                        instance.tag = reader.Read<System.String>(ES3Type_string.Instance);
                        break;
                    case "name":
                        instance.name = reader.Read<System.String>(ES3Type_string.Instance);
                        break;
                    case "hideFlags":
                        instance.hideFlags = reader.Read<UnityEngine.HideFlags>();
                        break;
                    case "active":
                        instance.SetActive(reader.Read<bool>(ES3Type_bool.Instance));
                        break;
                    case "children":
                        var children = reader.Read<GameObject[]>();
                        var parent = instance.transform;
                        // Set the parent of each child to this Transform in case the reference ID of the parent has changed.
                        foreach (var child in children)
                            child.transform.SetParent(parent);
                        break;
                    case "components":
                        ReadComponents(reader, instance);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }

        private void ReadComponents(ES3Reader reader, GameObject go)
        {
            if (reader.StartReadCollection())
                return;

            var components = new List<Component>(go.GetComponents<Component>());

            // Read each Component in Components array
            while (true)
            {
                if (!reader.StartReadCollectionItem())
                    break;

                if (reader.StartReadObject())
                    // We're reading null, so skip this Component.
                    continue;

                string typeName = null;
                Type type = null;

                string propertyName;
                while (true)
                {
                    propertyName = ReadPropertyName(reader);

                    if (propertyName == ES3Type.typeFieldName)
                    {
                        typeName = reader.Read<string>(ES3Type_string.Instance);
                        type = ES3Reflection.GetType(typeName);
                    }
                    else if (propertyName == ES3ReferenceMgrBase.referencePropertyName)
                    {
                        if (type == null)
                        {
                            if (string.IsNullOrEmpty(typeName))
                                throw new InvalidOperationException("Cannot load Component because no type data has been stored with it, so it's not possible to determine it's type");
                            else
                                Debug.LogWarning($"Cannot load Component of type {typeName} because this type no longer exists in your project. Note that this issue will create an empty GameObject named 'New Game Object' in your scene due to the way in which this Component needs to be skipped.");

                            // Read past the Component.
                            reader.overridePropertiesName = propertyName;
                            ReadObject<Component>(reader);
                            break;
                        }

                        var componentRef = reader.Read_ref();

                        // Rather than loading by reference, load using the Components list.
                        var c = components.Find(x => x.GetType() == type);
                        // If the Component exists in the Component list, load into it and remove it from the list.
                        if (c != null)
                        {
                            if (ES3ReferenceMgrBase.Current != null)
                                ES3ReferenceMgrBase.Current.Add(c, componentRef);

                            ES3TypeMgr.GetOrCreateES3Type(type).ReadInto<Component>(reader, c);
                            components.Remove(c);
                        }
                        // Else, create a new Component.
                        else
                        {
                            var component = go.AddComponent(type);
                            ES3TypeMgr.GetOrCreateES3Type(type).ReadInto<Component>(reader, component);
                            ES3ReferenceMgrBase.Current.Add(component, componentRef);
                        }
                        break;
                    }
                    else if (propertyName == null)
                        break;
                    else
                    {
                        reader.overridePropertiesName = propertyName;
                        ReadObject<Component>(reader);
                        break;
                    }
                }

                reader.EndReadObject();

                if (reader.EndReadCollectionItem())
                    break;
            }

            reader.EndReadCollection();
        }

        private GameObject CreateNewGameObject(ES3ReferenceMgrBase refMgr, long id)
        {
            GameObject go = new GameObject();
            if (id != 0)
                refMgr.Add(go, id);
            else
                refMgr.Add(go);
            return go;
        }

        /*
		 * 	Gets the direct children of this GameObject.
		 */
        public static List<GameObject> GetChildren(GameObject go)
        {
            var goTransform = go.transform;
            var children = new List<GameObject>();

            foreach (Transform child in goTransform)
                // If a child has an Auto Save component, let it save itself.
                //if(child.GetComponent<ES3AutoSave>() == null)
                children.Add(child.gameObject);

            return children;
        }

        // These are not used as we've overridden the ReadObject methods instead.
        protected override void WriteUnityObject(object obj, ES3Writer writer) { }
        protected override void ReadUnityObject<T>(ES3Reader reader, object obj) { }
        protected override object ReadUnityObject<T>(ES3Reader reader) { return null; }
    }

    public class ES3Type_GameObjectArray : ES3ArrayType
    {
        public static ES3Type Instance;

        public ES3Type_GameObjectArray() : base(typeof(UnityEngine.GameObject[]), ES3Type_GameObject.Instance)
        {
            Instance = this;
        }
    }
}