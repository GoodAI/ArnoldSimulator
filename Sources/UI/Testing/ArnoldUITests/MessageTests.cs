using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GoodAI.Arnold.Visualization.Models;
using GoodAI.Arnold.Communication;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Extensions;
using GoodAI.Arnold.Observation;
using OpenTK;
using Xunit;

namespace GoodAI.Arnold.UI.Tests
{
    public class MessageTests
    {
        [Fact]
        public void WritesReadsCommand()
        {
            var message = CommandRequestBuilder.Build(CommandType.Run);

            Assert.Equal(CommandType.Run, message.GetRequest(new CommandRequest()).Command);
        }

        [Fact]
        public void WritesReadsGetState()
        {
            var message = GetStateRequestBuilder.Build();

            Assert.Equal(Request.GetStateRequest, message.RequestType);
        }

        [Fact]
        public void WritesReadsGetModel()
        {
            var position = new Vector3(1, 2, 3);
            var size = new Vector3(4, 5, 6);
            var filter = new ModelFilter
            {
                Boxes = {new FilterBox(position, size)}
            };

            var message = GetModelRequestBuilder.Build(full: true, filter: filter);

            Assert.Equal(Request.GetModelRequest, message.RequestType);

            GetModelRequest getModelRequest = message.GetRequest(new GetModelRequest());
            Assert.True(getModelRequest.Full);
            Assert.Equal(position, getModelRequest.Filter.GetBoxes(0).Position());
            Assert.Equal(size, getModelRequest.Filter.GetBoxes(0).Size());
        }

        [Fact]
        public void WritesReadsStateResponse()
        {
            var message = StateResponseBuilder.Build(StateType.ShuttingDown);

            Assert.Equal(StateType.ShuttingDown, message.GetResponse(new StateResponse()).State);
        }

        [Fact]
        public void WritesReadsModelResponse()
        {
            const string regionName = "test region name";
            const string regionType = "test region type";

            var message =
                ModelResponseBuilder.Build(new List<RegionModel>
                {
                    new RegionModel(1, regionName, regionType, new Vector3(10, 20, 30), new Vector3(40, 30, 20))
                });

            Assert.Equal(regionName, message.GetResponse(new ModelResponse()).GetAddedRegions(0).Name);
        }

        [Fact]
        public void WritesReadsStateResponseError()
        {
            var message = ErrorResponseBuilder.Build("foo");

            Assert.Equal("foo", message.GetResponse(new ErrorResponse()).Message);
        }

        [Fact]
        public void WritesReadsObserverSetupRequest()
        {
            var definitions = new List<ObserverDefinition>
            {
                new ObserverDefinition(1, 2, "foofighter")
            };

            var message = ObserverSetupRequestBuilder.Build(definitions);

            var requestedObserver = message.GetRequest(new ObserverSetupRequest()).GetObservers(0);
            Assert.Equal((uint) 1, requestedObserver.GetNeuron(new NeuronId()).Neuron);
            Assert.Equal((uint) 2, requestedObserver.GetNeuron(new NeuronId()).Region);
            Assert.Equal("foofighter", requestedObserver.Type);
        }
    }
}
