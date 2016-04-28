#include "requesthandler.h"

void RequestHandler::EnqueueClientRequest(RequestId token, const char *data, int length)
{
	std::vector<uint8_t> message;
	message.assign(data, data + length);

	// Store the data in a vector so it's serializable.
	// The original buffer can still be accessed via &message[0];
	mClientRequests.push_back(std::pair<RequestId, std::vector<uint8_t>>(token, message));
	ProcessClientRequests();
}

void RequestHandler::ProcessClientRequests() {
	CkPrintf("Processing %d client requests\n", mClientRequests.size());
	try
	{
		for (auto requestPair : mClientRequests) {
			RequestId token = requestPair.first;
			std::vector<uint8_t> request = requestPair.second;

			// FlatBuffers work with an uint8_t* buffer.
			uint8_t* data = &request[0];
			const RequestMessage *requestMessage = GetRequestMessage(data);
			ProcessClientRequest(requestMessage, token);
		}
	}
	catch (ShutdownRequestedException &exception)
	{
		mCore->Exit();
	}
}

void RequestHandler::ProcessClientRequest(const RequestMessage *requestMessage, const RequestId token) const
{
	Request requestType = requestMessage->request_type();

	switch (requestType) {
	case Request_CommandRequest:
	{
		const CommandRequest *commandRequest = static_cast<const CommandRequest*>(requestMessage->request());
		ProcessCommandRequest(commandRequest, token);
		break;
	}
	case Request_GetStateRequest:
	{
		const GetStateRequest *getStateRequest = static_cast<const GetStateRequest*>(requestMessage->request());
		ProcessGetStateRequest(getStateRequest, token);
		break;
	}
	default:
		CkPrintf("Unknown request type %d", requestType);
	}
}

void RequestHandler::ProcessCommandRequest(const CommandRequest *commandRequest, RequestId token) const
{
	CommandType commandType = commandRequest->command();

	if (commandType == CommandType_Shutdown)
	{
		std::vector<uint8_t> message = CreateStateMessage(StateType_ShuttingDown);
		mCore->SendResponseToClient(token, message);

		throw ShutdownRequestedException("Shutdown requested by the client");
	}
	else
	{
		// TODO(HonzaS): Add actual logic here.
		std::vector<uint8_t> message = CreateStateMessage(StateType_Running);
		mCore->SendResponseToClient(token, message);
	}
}

void RequestHandler::ProcessGetStateRequest(const GetStateRequest *getStateRequest, RequestId token) const
{
	// TODO(HonzaS): Add actual logic here.
	std::vector<uint8_t> message = CreateStateMessage(StateType_Running);
	mCore->SendResponseToClient(token, message);
}

std::vector<uint8_t> RequestHandler::CreateStateMessage(StateType state)
{
	flatbuffers::FlatBufferBuilder builder;

	flatbuffers::Offset<StateResponse> stateResponseOffset = CreateStateResponse(builder, state);
	builder.Finish(stateResponseOffset);

	uint8_t *buffer = builder.GetBufferPointer();
	std::vector<uint8_t> vecResponse(buffer, buffer + builder.GetSize());

	return vecResponse;
}
