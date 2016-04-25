#include "requesthandler.h"

void RequestHandler::EnqueueClientRequest(RequestId token, std::vector<uint8_t> &request)
{
	mClientRequests.push_back(std::pair<RequestId, std::vector<uint8_t>>(token, request));
}

void RequestHandler::ProcessClientRequests() {
	for (auto requestPair : mClientRequests) {
		RequestId token = requestPair.first;
		std::vector<uint8_t> request = requestPair.second;

		// FlatBuffers work with an uint8_t* buffer.
		uint8_t* data = &request[0];
		const RequestMessage* requestMessage = GetRequestMessage(data);
		ProcessClientRequest(requestMessage, token);
	}
}

void RequestHandler::ProcessClientRequest(const RequestMessage* requestMessage, const RequestId token) {
	Request requestType = requestMessage->request_type();

	switch (requestType) {
	case Request_CommandRequest:
	{
		const CommandRequest* commandRequest = static_cast<const CommandRequest*>(requestMessage->request());
		ProcessCommandRequest(commandRequest, token);
		break;
	}
	case Request_GetStateRequest:
	{
		const GetStateRequest* getStateRequest = static_cast<const GetStateRequest*>(requestMessage->request());
		ProcessGetStateRequest(getStateRequest, token);
		break;
	}
	default:
		CkPrintf("Unknown request type %d", requestType);
	}
}

void RequestHandler::ProcessCommandRequest(const CommandRequest* commandRequest, const RequestId token)
{
	CommandType commandType = commandRequest->command();
	CkPrintf("Received command request");

	// TODO(HonzaS): Add actual logic here.
	std::vector<uint8_t> message = CreateStateMessage(StateType_Running);
	mCore->SendResponseToClient(token, message);
}

void RequestHandler::ProcessGetStateRequest(const GetStateRequest* getStateRequest, const RequestId token)
{
	CkPrintf("Received GetState request");
	// TODO(HonzaS): Add actual logic here.
	std::vector<uint8_t> message = CreateStateMessage(StateType_Running);
	mCore->SendResponseToClient(token, message);
}

const std::vector<uint8_t> RequestHandler::CreateStateMessage(const StateType state)
{
	flatbuffers::FlatBufferBuilder builder;

	flatbuffers::Offset<StateResponse> stateResponseOffset = CreateStateResponse(builder, state);
	builder.Finish(stateResponseOffset);

	uint8_t* buffer = builder.GetBufferPointer();
	std::vector<uint8_t> vecResponse(buffer, buffer + builder.GetSize());

	return vecResponse;
}
