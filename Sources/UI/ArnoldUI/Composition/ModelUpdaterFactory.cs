using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Communication;
using GoodAI.Arnold.Core;
using SimpleInjector;

namespace GoodAI.Arnold
{
    public interface IModelUpdaterFactory
    {
        IModelUpdater Create(ICoreLink coreLink, ICoreController coreController);
    }

    public class ModelUpdaterFactory : PropertyInjectingFactory, IModelUpdaterFactory
    {
        private readonly IModelDiffApplier m_modelDiffApplier;

        public ModelUpdaterFactory(Container container, IModelDiffApplier modelDiffApplier) : base(container)
        {
            m_modelDiffApplier = modelDiffApplier;
        }

        public IModelUpdater Create(ICoreLink coreLink, ICoreController coreController)
        {
            return InjectProperties(new ModelUpdater(coreLink, coreController, m_modelDiffApplier));
        }
    }
}
