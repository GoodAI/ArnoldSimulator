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
        public void WritesReadsRunStepsCommand()
        {
            var message = CommandRequestBuilder.Build(CommandType.Run, stepsToRun: 1);

            var commandRequest = message.GetRequest(new CommandRequest());
            Assert.Equal(CommandType.Run, commandRequest.Command);
            Assert.Equal((uint) 1, commandRequest.StepsToRun);
        }

        [Fact]
        public void WritesReadsRunToBodyStepCommand()
        {
            var message = CommandRequestBuilder.Build(CommandType.Run, runToBodyStep: true);

            var commandRequest = message.GetRequest(new CommandRequest());
            Assert.Equal(CommandType.Run, commandRequest.Command);
            Assert.True(commandRequest.RunToBodyStep);
        }

        [Fact]
        public void StepsToRunWithRunToBodyStepFails()
        {
            Assert.Throws<InvalidOperationException>(() => CommandRequestBuilder.Build(CommandType.Run, stepsToRun: 10, runToBodyStep: true));
        }

        [Fact]
        public void WritesReadsConfigurationCommand()
        {
            var configContent = "foo";
            var message = CommandRequestBuilder.Build(CommandType.Configure, configuration: new CoreConfiguration(configContent));

            CommandRequest commandRequest = message.GetRequest(new CommandRequest());
            Assert.Equal(CommandType.Configure, commandRequest.Command);
            Assert.Equal(configContent, commandRequest.Configuration.SystemConfiguration);
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
            
            var observerDefinitions = new List<ObserverDefinition>
            {
                new ObserverDefinition(1, 2, "foofighter")
            };

            var message = GetModelRequestBuilder.Build(full: true, filter: filter, observerRequests: observerDefinitions);

            Assert.Equal(Request.GetModelRequest, message.RequestType);

            GetModelRequest getModelRequest = message.GetRequest(new GetModelRequest());
            Assert.True(getModelRequest.Full);
            Assert.Equal(position, getModelRequest.Filter.GetBoxes(0).Position());
            Assert.Equal(size, getModelRequest.Filter.GetBoxes(0).Size());

            var requestedObserver = getModelRequest.GetObservers(0);
            Assert.Equal((uint) 1, requestedObserver.NeuronId.Neuron);
            Assert.Equal((uint) 2, requestedObserver.NeuronId.Region);
            Assert.Equal("foofighter", requestedObserver.Type);

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

            var observer = new ObserverDefinition(1, 2, "foo");
            var data = new byte[] {1, 2, 3};

            var observerData = new ObserverDataContainer(observer,
                new ObserverData(metadata: null, plainData: data, floatData: null));

            var addedRegions = new List<RegionModel>
            {
                new RegionModel(1, regionName, regionType, new Vector3(10, 20, 30), new Vector3(40, 30, 20))
            };
            var message = ModelResponseBuilder.Build(addedRegions: addedRegions,
                observers: new List<ObserverDataContainer> {observerData});

            ModelResponse modelResponse = message.GetResponse(new ModelResponse());
            Assert.Equal(regionName, modelResponse.GetAddedRegions(0).Name);
            Assert.Equal(observer.Type, modelResponse.GetObserverResults(0).Observer.Type);
            Assert.Equal(data[0], modelResponse.GetObserverResults(0).GetPlainData(0));
        }

        [Fact]
        public void WritesReadsStateResponseError()
        {
            var message = ErrorResponseBuilder.Build("foo");

            Assert.Equal("foo", message.GetResponse(new ErrorResponse()).Message);
        }
    }
}
