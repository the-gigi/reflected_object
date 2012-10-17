using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Gigi.TestUtil
{
    public class ReflectedObject
    {
        object _instance;

        public Type Type
        {
            get { return _instance.GetType(); }
        }

        public object Instance
        {
            get { return _instance; }
        }

        public static object CreateInstance(Assembly assembly, string fullyQualifiedClassName, params object[] constructorArguments)
        {
            return new ReflectedObject(assembly, fullyQualifiedClassName, constructorArguments)._instance;
        }

        public static object CreateInstance(Assembly assembly, string fullyQualifiedClassName, IDictionary<string, object> namedArguments)
        {
            return new ReflectedObject(assembly, fullyQualifiedClassName, namedArguments)._instance;
        }

        public ReflectedObject(Assembly assembly, string fullyQualifiedClassName, params object[] constructorArguments)
        {
            var t = assembly.GetType(fullyQualifiedClassName);
            _instance = Activator.CreateInstance(t, constructorArguments);
        }

        public ReflectedObject(Assembly assembly, string fullyQualifiedClassName, IDictionary<string, object> namedArguments) :
            this(assembly, fullyQualifiedClassName)
        {
            var fields = Type.GetFields();
            var fieldNames = from f in fields select f.Name;
            var properties = Type.GetProperties();
            var propertyNames = from p in properties select p.Name;

            foreach (var a in namedArguments)
            {
                if (fieldNames.Contains(a.Key))
                {
                    SetField(a.Key, a.Value);
                }
                else if (propertyNames.Contains(a.Key))
                {
                    SetProperty(a.Key, a.Value);
                }
                else
                {
                    throw new ArgumentException("No such field or property: " + a.Key);
                }
            }
        }

        public object InvokeMethod(string methodName, params object[] args)
        {
            //MethodInfo method = _instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo method = Type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            return method.Invoke(_instance, args);
        }

        public object GetProperty(string name)
        {
            var properties = Type.GetProperties();
            var property = Type.GetProperty(name);
            return property.GetValue(_instance, null);
        }

        public void SetProperty(string name, object value)
        {
            var property = Type.GetProperty(name);
            property.SetValue(_instance, value, null);
        }

        public object GetField(string name)
        {
            var field = Type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                var fields = Type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                Trace.WriteLine(fields.ToString());
            }

            return field.GetValue(_instance);
        }

        public void SetField(string name, object value)
        {
            var field = Type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(_instance, value);
        }
    }
}
