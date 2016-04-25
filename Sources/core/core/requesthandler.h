#pragma once

#include <vector>
#include "common.h"

#include "charm.h"

#include "requests_generated.h"
#include "responses_generated.h"

using namespace GoodAI::Arnold::Network;

class RequestHandler {
public:
	RequestHandler() {}

    RequestHandler(const RequestHandler &other) = delete;
    RequestHandler &operator=(const RequestHandler &other) = delete;

    void EnqueueClientRequest(RequestId token, std::vector<unsigned char> &request);
	void ProcessClientRequests();
private:
    std::vector<std::pair<RequestId, std::vector<unsigned char>>> mClientRequests;

	void ProcessClientRequest(const RequestMessage *requestMessage);
	void ProcessCommandRequest(const CommandRequest *commandRequest);
	void ProcessGetStateRequest(const GetStateRequest *getStateRequest);
};
