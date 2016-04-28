#pragma once

#include <atomic>
#include <unordered_map>

#include "common.h"
#include "core.decl.h"
#include "brain.decl.h"

#include "requesthandler.h"

class RequestHandler;

class Core : public CBase_Core
{
public:
    Core(CkArgMsg *msg);

    void Exit();

    void HandleRequestFromClient(CkCcsRequestMsg *msg);

    void SendResponseToClient(RequestId token, std::vector<uint8_t> &response);
	void NoResponseToClient(RequestId token);

private:
    double mStart;
    CProxy_BrainBase mBrain;
	RequestHandler *mRequestHandler;

    static std::atomic<RequestId> mRequestCounter;
    static std::unordered_map<RequestId, CkCcsRequestMsg*> mTokens;
};
