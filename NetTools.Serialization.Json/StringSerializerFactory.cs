using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using NetTools.Serialization.Attributes;
using System.Runtime.Serialization;

namespace NetTools.Serialization.Serializers
{
    /// <summary>
    /// This public class is sealed so it can't be inherited. It also has an internal constructor so it can not be instantiated outside of the
    /// NetTools.Serialization class library. It follows the idea of the singleton design pattern and should only be accessed from the
    /// Json.SerializationFactory public static object.
    /// </summary>
    public sealed class StringSerializerFactory
    {
        private readonly object Padlock = new object();
        private bool IsDirty = false;


        /// <summary>
        /// This dictionary is populated with default serializers at runtime. It can be added to or updated using the method below and shouldn't
        /// be modified directly.
        /// </summary>
        private Dictionary<Type, IStringSerializer> Serializers;


        internal bool HasSerializer(Type type)
        {
            if (Serializers != null)
            {
                return Serializers.Keys.Any(x => x == type);
            }

            return false;
        }

        /// <summary>
        /// Force internal constructor, creating an instance of this class is not allowed and is used from within the Json static class only!
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
                    // It means that the internal class implementing IStringSerializer can't be instantiated using the default
                    // parameterless constructor - which should never happen since these are internal serializers.
                }
            }
        }

        public void AddSerializer(IStringSerializer serializer)
        {
            var attributes = serializer.GetType().GetAttributes<KnownObject>();

            if (attributes.Count == 0)
            {
                var builtIn = serializer.GetType().GetAttributes<KnownTypeAttribute>().Where(x => x.Type != null);

                foreach (var attribute in builtIn)
                {
                    attributes.Add(new KnownObject(attribute.Type));
                }

                if (attributes.Count == 0)
                {
                    throw new InvalidOperationException("When adding an IStringSerializer to JsonSerializationFactory the serializer class must have a NetTools.Serialization.Attributes.KnownObjectAttribute (or a System.Runtime.Serialization.KnownTypeAttribute with a specified type).");
                }
            }

            lock (Padlock)
            {
                foreach (var known in attributes)
                {
                    Serializers[known.KnownType] = serializer;
                    IsDirty = true;
                }
            }
        }


        void CleanDirtyDictionary()
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
            CleanDirtyDictionary();

            if (Serializers.TryGetValue(t, out var s))
            {
                return s.FromString(obj, t);
            }

            return null;
        }


        internal string ToString(object obj)
        {
            CleanDirtyDictionary();

            var t = obj.GetType();

            if (Serializers.TryGetValue(t, out var s))
            {
                return s.ToString(obj);
            }

            return null;
        }
    }
}