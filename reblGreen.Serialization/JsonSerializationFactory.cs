using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace reblGreen.Serialization
{
    public class JsonSerializationFactory : IJsonSerializer
    {
        #region Singleton

        /// <summary>
        /// private singleton design pattern
        /// </summary>
        static JsonSerializationFactory Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new JsonSerializationFactory();
                }

                return _Instance;
            }
        }

        static JsonSerializationFactory _Instance;


        object Padlock = new object();
        bool IsDirty;


        /// <summary>
        /// This dictionary is populated with default serializers at runtime.
        /// It can be added to or updated using the method below and shouldn't
        /// be modified directly.
        /// </summary>
        Dictionary<Type, IJsonSerializer> Serializers;



        /// <summary>
        /// Force private constructor, creating an instance of this class is not allowed and is used from the static methods only!
        /// This class uses a singleton design pattern internally.
        /// </summary>
        JsonSerializationFactory()
        {
            Serializers = new Dictionary<Type, IJsonSerializer>();
            var @this = typeof(JsonSerializationFactory);

            var assembly = typeof(JsonSerializationFactory).GetTypeInfo().Assembly;
            var t = typeof(IJsonSerializer).GetTypeInfo();

            var serializers = assembly.DefinedTypes.Where(c =>
                 t.IsAssignableFrom(c) && c.IsClass && !c.IsAbstract
            );
                                      
            foreach (var s in serializers)
            {
                try
                {
                    var type = s.AsType();

                    if (type == @this)
                    {
                        continue;
                    }

                    var serializer = (IJsonSerializer)Activator.CreateInstance(type);
                    AddSerializer(serializer);
                }
                catch
                {
                    // Do Nothing here with a failed activation.
                    // It means that the internal class implementing IJsonSerializer can't be instantiated using the default
                    // parameterless constructor - which should never happen.
                }
            }
        }

        #endregion





        public void AddSerializer(IJsonSerializer serializer)
        {
            var attributes = serializer.GetType().GetAttributes<KnownObject>();

            if (attributes.Count  == 0)
            {
                throw new InvalidOperationException("When adding an IJsonSerializer to JsonSerializationFactory the serializer class must have a KnownObjectAttribute");
            }

            lock (Padlock)
            {
                foreach (var known in attributes)
                {

                    if (Serializers.ContainsKey(known.KnownType))
                    {
                        Instance.Serializers[known.KnownType] = serializer;
                    }
                    else
                    {
                        Serializers.Add(known.KnownType, serializer);
                        IsDirty = true;
                    }
                }
            }
        }

        void CleanDirtyDic()
        {
            if (IsDirty)
            {
                lock (Padlock)
                {
                    var sorted = Serializers.OrderByDescending(i => i.Key.GetInheritanceHierarchy().Count);
                    Serializers = sorted.ToDictionary(pair => pair.Key, pair => pair.Value);
                }
            }
        }


        public object Deserialize(object obj)
        {
            CleanDirtyDic();
            throw new NotImplementedException();
        }

        public object Serialize(object obj)
        {
            CleanDirtyDic();
            throw new NotImplementedException();
        }


        public static T DeserializeJson<T>(object obj)
        {
            return (T)Instance.Deserialize(obj);
            //throw new NotImplementedException();
        }

        public static string SerializeJson<T>(T obj) where T : class
        {
            return (string)Instance.Serialize(obj);
            //throw new NotImplementedException();
        }

    }
}
