using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LccHotfix
{
    public partial class AudioManager
    {
        private readonly Dictionary<string, Type> _eventTypes = new();
        private void InitAttr()
        {
            CollectAttr();
        }
        
        private void CollectAttr()
        {
            _eventTypes.Clear();
            IEnumerable<Type> types = null;
            if (Main.CodeTypesService != null)
            {
                types = Main.CodeTypesService.GetTypes(typeof(AudioEventAttr));
            }
            else
            {
                types = typeof(AudioEventAttr).Assembly.GetTypes()
                    .Where(t => !t.IsAbstract && t.GetCustomAttribute<AudioEventAttr>(true) != null);
            }

            foreach (Type type in types)
            {
                _eventTypes.Add(type.Name, type);
            }
        }

        private Type GetEventType(string typeName)
        {
            if (_eventTypes.TryGetValue(typeName, out var targetType))
            {
                return targetType;
            }
            Audio.LogWarning($"[Audio] Attribute of AudioEvent \"{typeName}\" not found");
            return null;
        }

        public List<string> GetAllEventTypes()
        {
            if (_eventTypes.Count == 0)
            {
                CollectAttr();
            }
            return _eventTypes.Keys.ToList();
        }
    }
}