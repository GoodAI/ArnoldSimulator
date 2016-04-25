#pragma once

#include <vector>
#include "common.h"

#include "charm.h"

#include "requests_generated.h"
#include "responses_generated.h"

#include "core.h"

using namespace GoodAI::Arnold::Network;

class Core;

class RequestHandler {
public:
	Core* mCore;

	RequestHandler(Core* core)
	{
		mCore = core;
	}

    RequestHandler(const RequestHandler &other) = delete;
    RequestHandler &operator=(const RequestHandler &other) = delete;

    void EnqueueClientRequest(RequestId token, std::vector<uint8_t> &request);
	void ProcessClientRequests();
private:
    std::vector<std::pair<RequestId, std::vector<uint8_t>>> mClientRequests;

	void ProcessClientRequest(const RequestMessage *requestMessage, const RequestId token);
	void ProcessCommandRequest(const CommandRequest *commandRequest, const RequestId token);
	void ProcessGetStateRequest(const GetStateRequest *getStateRequest, const RequestId token);

	const std::vector<uint8_t> CreateStateMessage(const StateType state);
};
