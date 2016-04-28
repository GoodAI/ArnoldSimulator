#pragma once

#include <vector>
#include "common.h"

#include "charm.h"

#include "requests_generated.h"
#include "responses_generated.h"

#include "core.h"
#include <queue>

using namespace GoodAI::Arnold::Network;

class Core;

class ShutdownRequestedException : public std::runtime_error
{
public:
	explicit ShutdownRequestedException(const char *reason) : runtime_error(reason) { }
};

class RequestHandler {
public:
	Core *mCore;

	RequestHandler(Core *core)
	{
		mCore = core;
	}

    RequestHandler(const RequestHandler &other) = delete;
    RequestHandler &operator=(const RequestHandler &other) = delete;

    void HandleRequestFromClient(RequestId token, const char *data, int length);
	void ProcessClientRequests();
private:
    std::queue<std::pair<RequestId, std::vector<uint8_t>>> mClientRequests;

	void ProcessClientRequest(const RequestMessage *requestMessage, RequestId token) const;
	void ProcessCommandRequest(const CommandRequest *commandRequest, RequestId token) const;
	void ProcessGetStateRequest(const GetStateRequest *getStateRequest, RequestId token) const;

	static std::vector<uint8_t> CreateStateMessage(const StateType state);
};
