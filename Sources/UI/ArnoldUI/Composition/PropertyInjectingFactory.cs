using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleInjector;

namespace GoodAI.Arnold
{
    public abstract class PropertyInjectingFactory
    {
        private readonly Container m_container;

        protected PropertyInjectingFactory(Container container)
        {
            m_container = container;
        }

        private static readonly Dictionary<Type, Registration> m_registrations = new Dictionary<Type, Registration>();

        protected TConcreteType InjectProperties<TConcreteType>(TConcreteType instance) where TConcreteType : class
        {
            Type type = typeof(TConcreteType);

            Registration registration;
            if (!m_registrations.TryGetValue(type, out registration))
                registration = m_registrations[type] = Lifestyle.Transient.CreateRegistration<TConcreteType>(m_container);

            registration.InitializeInstance(instance);

            return instance;
        }
    }
}
