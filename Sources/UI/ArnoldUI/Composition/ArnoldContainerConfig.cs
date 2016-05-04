using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArnoldUI;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Forms;
using GoodAI.Arnold.Network;
using GoodAI.Arnold.Runtime;
using GoodAI.Logging;
using GoodAI.Net.ConverseSharpFlatBuffers;
using GoodAI.TypeMapping;
using SimpleInjector;

namespace GoodAI.Arnold
{
    internal class ArnoldContainerConfig : IContainerConfiguration
    {
        public void Configure(Container container)
        {
            container.Options.PropertySelectionBehavior = new PropertyInjectionForType<ILog>(container);

            container.RegisterSingleton<IWinFormsLogSink, RichTextBoxLogSink>();

            container.RegisterSingleton(() => LoggingConfig.Setup(container.GetInstance<IWinFormsLogSink>()));
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
            container.RegisterSingleton<ICoreProxyFactory, CoreProxyFactory>();
            container.RegisterSingleton<ICoreProcessFactory, CoreProcessFactory>();
            container.RegisterSingleton<ICoreLinkFactory, CoreLinkFactory>();
            container.RegisterSingleton<ICoreControllerFactory, CoreControllerFactory>();
            container.RegisterSingleton<IConductor, Conductor>();
            container.RegisterSingleton<UIMain>();
            container.RegisterSingleton<LogForm>();
            container.RegisterSingleton<GraphForm>();
            container.RegisterSingleton<MainForm>();
        }
    }
}
