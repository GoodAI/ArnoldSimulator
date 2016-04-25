#include "requesthandler.h"

void RequestHandler::EnqueueClientRequest(RequestId token, std::vector<unsigned char> &request)
{
	mClientRequests.push_back(std::pair<RequestId, std::vector<unsigned char>>(token, request));
}

void RequestHandler::ProcessClientRequests() {
	for (auto requestPair : mClientRequests) {
		RequestId token = requestPair.first;
		std::vector<unsigned char> request = requestPair.second;
		unsigned char *data = &request[0];

		// Deserialize the data from the buffer.
		const RequestMessage *requestMessage = GetRequestMessage(data);

		ProcessClientRequest(requestMessage);
	}
}

void RequestHandler::ProcessClientRequest(const RequestMessage *requestMessage) {
	Request requestType = requestMessage->request_type();

	switch (requestType) {
	case Request_CommandRequest:
		const CommandRequest *commandRequest = static_cast<const CommandRequest*>(requestMessage->request());
		ProcessCommandRequest(commandRequest);
		break;
	case Request_GetStateRequest:
		const GetStateRequest *getStateRequest = static_cast<const GetStateRequest*>(requestMessage->request());
		ProcessGetStateRequest(getStateRequest);
		break;
	default:
		CkPrintf("Unknown request type %d", requestType);
	}
}

void RequestHandler::ProcessCommandRequest(const CommandRequest *commandRequest)
{
	CommandType commandType = commandRequest->command();
	CkPrintf("Received command request");
}

void RequestHandler::ProcessGetStateRequest(const GetStateRequest *getStateRequest)
{
	CkPrintf("Received GetState request");
}
