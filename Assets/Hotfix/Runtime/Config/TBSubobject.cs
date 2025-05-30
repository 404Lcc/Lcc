
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Luban;
using SimpleJSON;


namespace cfg
{
public partial class TBSubobject
{
    private readonly System.Collections.Generic.Dictionary<int, Subobject> _dataMap;
    private readonly System.Collections.Generic.List<Subobject> _dataList;
    
    public TBSubobject(JSONNode _buf)
    {
        _dataMap = new System.Collections.Generic.Dictionary<int, Subobject>();
        _dataList = new System.Collections.Generic.List<Subobject>();
        
        foreach(JSONNode _ele in _buf.Children)
        {
            Subobject _v;
            { if(!_ele.IsObject) { throw new SerializationException(); }  _v = Subobject.DeserializeSubobject(_ele);  }
            _dataList.Add(_v);
            _dataMap.Add(_v.SubobjectId, _v);
        }
    }

    public System.Collections.Generic.Dictionary<int, Subobject> DataMap => _dataMap;
    public System.Collections.Generic.List<Subobject> DataList => _dataList;

    public Subobject GetOrDefault(int key) => _dataMap.TryGetValue(key, out var v) ? v : null;
    public Subobject Get(int key) => _dataMap[key];
    public Subobject this[int key] => _dataMap[key];

    public void ResolveRef(Tables tables)
    {
        foreach(var _v in _dataList)
        {
            _v.ResolveRef(tables);
        }
    }

}

}

