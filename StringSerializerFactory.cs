using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using reblGreen.Serialization.Attributes;

namespace reblGreen.Serialization.Serializers
{
    public class StringSerializerFactory
    {
        object Padlock = new object();
        bool IsDirty;


        /// <summary>
        /// This dictionary is populated with default serializers at runtime.
        /// It can be added to or updated using the method below and shouldn't
        /// be modified directly.
        /// </summary>
        Dictionary<Type, IStringSerializer> Serializers;


        /// <summary>
        /// Force private constructor, creating an instance of this class is not allowed and is used from the static methods only!
        /// This class uses a singleton design pattern internally.
        /// </summary>
        internal StringSerializerFactory()
        {
            Serializers = new Dictionary<Type, IStringSerializer>();
            var @this = typeof(StringSerializerFactory);

            var assembly = typeof(StringSerializerFactory).GetTypeInfo().Assembly;
            var t = typeof(IStringSerializer).GetTypeInfo();

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

                    var serializer = (IStringSerializer)Activator.CreateInstance(type);
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


        public void AddSerializer(IStringSerializer serializer)
        {
            var attributes = serializer.GetType().GetAttributes<KnownObject>();

            if (attributes.Count == 0)
            {
                throw new InvalidOperationException("When adding an IJsonSerializer to JsonSerializationFactory the serializer class must have a reblGreen.Serialization.Attributes.KnownObjectAttribute.");
            }

            lock (Padlock)
            {
                foreach (var known in attributes)
                {

                    if (Serializers.ContainsKey(known.KnownType))
                    {
                        Serializers[known.KnownType] = serializer;
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


        internal object FromString(string obj, Type t)
        {
            CleanDirtyDic();

            if (Serializers.ContainsKey(t))
            {
                return Serializers[t].FromString(obj);
            }

            return null;
        }


        internal string ToString(object obj)
        {
            CleanDirtyDic();

            var t = obj.GetType();

            if (Serializers.ContainsKey(t))
            {
                return Serializers[t].ToString(obj);
            }

            return null;
        }
    }
}