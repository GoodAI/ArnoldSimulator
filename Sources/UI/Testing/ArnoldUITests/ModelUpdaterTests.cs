using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatBuffers;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Network;
using Moq;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class ModelUpdaterTests
    {
        class DummyModelResponseCoreLink : ICoreLink
        {
            public Task<TResponse> Request<TRequest, TResponse>(IConversation<TRequest, TResponse> conversation, int timeoutMs) where TRequest : Table where TResponse : Table, new()
            {
                var request = conversation.RequestData as GetModelRequest;
                if (request == null)
                    throw new ArgumentException("Can only respond to a model request");

                return Task<TResponse>.Factory.StartNew(() =>
                {
                    return new TResponse();
                });
            }
        }

        private readonly ICoreController m_coreController;
        private readonly DummyModelResponseCoreLink m_coreLink;
        private readonly ModelUpdater m_modelUpdater;

        public ModelUpdaterTests()
        {
            var coreControllerMock = new Mock<ICoreController>();
            m_coreController = coreControllerMock.Object;

            m_coreLink = new DummyModelResponseCoreLink();
            m_modelUpdater = new ModelUpdater(m_coreLink, m_coreController);
        }

        [Fact]
        public void GetsModelSeveralTimes()
        {
            const int timeoutMs = 100;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            m_modelUpdater.Start();

            for (int i = 0; i < 5; i++)
            {
                Assert.Null(m_modelUpdater.GetNewModel());

                m_modelUpdater.AllowModelRequest();

                while (true)
                {
                    if (m_modelUpdater.GetNewModel() != null)
                        break;

                    Assert.True(stopwatch.ElapsedMilliseconds < timeoutMs);
                }
            }

            m_modelUpdater.Stop();
        }
    }
}
