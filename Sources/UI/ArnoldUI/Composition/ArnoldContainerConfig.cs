using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArnoldUI;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Forms;
using GoodAI.Arnold.Logging;
using GoodAI.Arnold.Communication;
using GoodAI.Arnold.Project;
using GoodAI.Logging;
using GoodAI.Net.ConverseSharpFlatBuffers;
using GoodAI.TypeMapping;
using Serilog;
using SimpleInjector;

namespace GoodAI.Arnold
{
    internal class ArnoldContainerConfig : IContainerConfiguration
    {
        public void Configure(Container container)
        {
            container.Options.PropertySelectionBehavior = new PropertyInjectionForType<ILog>(container);

            // Keep the type so that it is clear what is being registered here.
            container.RegisterSingleton<LoggerConfiguration>(() => LoggingConfig.Setup(container.GetInstance<LogForm>().TextBox));

            container.RegisterConditional(
                typeof(ILog),
                typeFactory => typeof(SerilogRobe<>).MakeGenericType(typeFactory.Consumer.ImplementationType),
                Lifestyle.Singleton,
                predicateContext => predicateContext.Consumer != null);
            container.RegisterConditional(
                typeof(ILog),
                typeFactory => typeof(SerilogRobe),
                Lifestyle.Singleton,
                predicateContext => predicateContext.Consumer == null);

            container.RegisterSingleton<IResponseParser, CoreResponseParser>();
            container.RegisterSingleton<ICoreLinkFactory, CoreLinkFactory>();
            container.RegisterSingleton<ICoreControllerFactory, CoreControllerFactory>();

            container.RegisterSingleton<ICoreProcessFactory, CoreProcessFactory>();
            container.RegisterSingleton<ICoreProxyFactory, CoreProxyFactory>();

            container.RegisterSingleton<IModelDiffApplier, ModelDiffApplier>();
            container.RegisterSingleton<IModelUpdaterFactory, ModelUpdaterFactory>();
            container.RegisterSingleton<IModelProviderFactory, ModelProviderFactory>();

            container.RegisterSingleton<IDesigner, Designer>();
            container.RegisterSingleton<IConductor, Conductor>();

            container.RegisterSingleton<UIMain>();
            container.RegisterSingleton<LogForm>();
            container.RegisterSingleton<GraphForm>();
            container.RegisterSingleton<JsonEditForm>();

            container.RegisterSingleton<MainForm>();
        }
    }
}
