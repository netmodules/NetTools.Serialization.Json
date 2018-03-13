using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using reblGreen.Serialization.JsonTools;

namespace reblGreen.Serialization.Objects
{
    /// <summary>
    /// DynamicJson can be cast to dynamic and offers late property bindings.
    /// </summary>
    public class DynamicJson : DynamicObject
    {
        public IDictionary<string, object> _dictionary { get; private set; }
        public IList<object> _list { get; private set; }

        /// <summary>
        /// Check this property to see if the DynamicJson object is an array of json objects.
        /// </summary>
        public bool IsArray { get { return _list != null; } }

        /// <summary>
        /// Check this property to see if the DynamicJson object is a json object.
        /// </summary>
        public bool IsObject { get { return _dictionary != null; } }

        public DynamicJson(string json)
        {
            var parse = Json.Reader.FromJson<object>(json);

            if (parse is IDictionary<string, object>)
            {
                _dictionary = (IDictionary<string, object>)parse;
            }
            else if (parse is IList<object>)
            {
                _list = (IList<object>)parse;
            }
            else
            {
                throw new Exception("DynamicJson parse error. Object is invalid");
            }

            TraverseSelfToDynamic();
        }

        internal DynamicJson(object jsonObj)
        {
            if (jsonObj is IDictionary<string, object>)
            {
                _dictionary = (IDictionary<string, object>)jsonObj;
            }
            else if (jsonObj is IList<object>)
            {
                _list = (IList<object>)jsonObj;
            }
            else
            {
                throw new Exception("DynamicJson parse error. Object is invalid");
            }

            TraverseSelfToDynamic();
        }


        /// <summary>
        /// Traverses self to dynamic.
        /// This method itterates over each element in this DynamicJson object and converts any Dictionary or List to a DynamicJson.
        /// </summary>
        void TraverseSelfToDynamic()
        {
            if (_dictionary != null)
            {
                foreach (var k in _dictionary.Keys.ToList())
                {
                    if (_dictionary[k] is List<object> || _dictionary[k] is Dictionary<string, object>)
                    {
                        _dictionary[k] = new DynamicJson(_dictionary[k]);
                    }
                }
            }
            else if (_list != null)
            {
                for (var i = 0; i < _list.Count; i++)
                {
                    if (_list[i] is List<object> || _list[i] is Dictionary<string, object>)
                    {
                        _list[i] = new DynamicJson(_list[i]);
                    }
                }
            }
        }



        #region Indexers
        public object this[int index]
        {  
            get
            {
                if (_list != null)
                {
                    return _list[index];
                }

                if (_dictionary != null)
                {
                    return _dictionary.Values.ElementAt(index);
                }

                return null;
            }

            set
            {
                if (_list != null)
                {
                    if (value is List<object> || value is Dictionary<string, object>)
                    {
                        value = new DynamicJson(value);
                    }

                    _list[index] = value;
                }
            }
        }

        public object this[string key]
        {
            get
            {
                if (_dictionary != null && _dictionary.ContainsKey(key))
                {
                    return _dictionary[key];
                }

                return null;
            }

            set
            {
                if (_dictionary != null)
                {
                    if (_dictionary.ContainsKey(key))
                    {
                        if (value is List<object> || value is Dictionary<string, object>)
                        {
                            value = new DynamicJson(value);
                        }
                            
                        _dictionary[key] = value;
                    }
                    else
                    {
                        if (value is List<object> || value is Dictionary<string, object>)
                        {
                            value = new DynamicJson(value);
                        }

                        _dictionary.Add(key, value);
                    }
                }
            }
        }
        #endregion


        #region DynamicObject
        public override bool TryGetIndex(GetIndexBinder binder, Object[] indexes, out Object result)
        {
            if (_list != null)
            {
                int index = (int)indexes[0];

                if (_list.Count > index)
                {
                    result = _list[index];

                    if (result is IDictionary<string, object> || result is IList<object>)
                    {
                        result = (dynamic)new DynamicJson(result);
                    }

                    return true;
                }

                result = null;
                return false;
            }

            return base.TryGetIndex(binder, indexes, out result);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            bool ret;

            if (_dictionary != null)
            {
                ret = _dictionary.TryGetValue(binder.Name, out result);

                if (ret == false)
                {
                    ret = _dictionary.TryGetValue(binder.Name.ToLower(), out result);
                }

                if (ret == false)
                {
                    result = null;
                    return false; // throw new Exception("property not found " + binder.Name);
                }

                if (result is IDictionary<string, object> || result is IList<object>)
                {
                    result = (dynamic)new DynamicJson(result);

                }

                return ret;
            }

            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (_dictionary != null)
            {
                if (value is List<object> || value is Dictionary<string, object>)
                {
                    value = new DynamicJson(value);
                }

                if (!_dictionary.ContainsKey(binder.Name))
                {
                    _dictionary.Add(binder.Name, value);
                }
                else
                {
                    _dictionary[binder.Name] = value;
                }

                return true;
            }

            return base.TrySetMember(binder, value);
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (_list != null)
            {
                if (value is List<object> || value is Dictionary<string, object>)
                {
                    value = new DynamicJson(value);
                }

                int index = (int)indexes[0];
                while (_list.ToList().Count <= index)
                {
                    _list.Add(null);
                }

                _list[index] = value;

                return true;
            }

            return base.TrySetIndex(binder, indexes, value);
        }
        #endregion

        #region operators
        public override string ToString()
        {
            if (_dictionary != null)
            {
                return Json.Writer.ToJson(_dictionary, Json.SerializationFactory);
            }
            else if (_list != null)
            {
                return Json.Writer.ToJson(_list, Json.SerializationFactory);
            }

            return GetType().ToString();
        }

        static public explicit operator string(DynamicJson dj)
        {
            return dj.ToString();
        }

        static public implicit operator Dictionary<string, object>(DynamicJson dj)
        {

            return dj._dictionary as Dictionary<string, object>;
        }

        static public implicit operator List<object>(DynamicJson dj)
        {
            return dj._list as List<object>;
        }
        #endregion
    }
}
