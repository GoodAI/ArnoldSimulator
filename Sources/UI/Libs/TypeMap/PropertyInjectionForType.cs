using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SimpleInjector;
using SimpleInjector.Advanced;

namespace GoodAI.TypeMapping
{
    public class PropertyInjectionForType<T> : IPropertySelectionBehavior
        where T : class
    {
        private readonly Container m_container;

        public PropertyInjectionForType(Container container)
        {
            m_container = container;
        }

        public bool SelectProperty(Type serviceType, PropertyInfo property)
        {
            // Do not check if the property type is registered, we want to fail in Verify when it isn't.
            // (Also, m_container.GetRegistration(property.PropertyType) crashes on null pointer
            // inside RegisterConditional lambda, because typeFactoryContext.Consumer is not set at the time.)
            return IsInjectableProperty(property) && property.PropertyType.IsAssignableFrom(typeof(T));
        }

        private static bool IsInjectableProperty(PropertyInfo property)
        {
            MethodInfo setMethod = property.GetSetMethod(nonPublic: false);
            return setMethod != null && !setMethod.IsStatic && property.CanWrite;
        }
    }
}
