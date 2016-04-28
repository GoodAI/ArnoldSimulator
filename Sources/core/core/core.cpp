#include <cstdio>
#include <cstdlib>
#include <vector>

#include "core.h"

CProxy_Core gCore;

std::atomic<RequestId> Core::mRequestCounter = 0;
std::unordered_map<RequestId, CcsDelayedReply> Core::mTokens;

void Core::HandleRequestFromClient(CkCcsRequestMsg *msg)
{
	RequestId requestId = mRequestCounter++;
	mTokens.insert(std::make_pair(requestId, msg->reply));

	flatbuffers::FlatBufferBuilder builder;

	mRequestHandler->EnqueueClientRequest(requestId, msg->data, msg->length);
	delete msg;
}

Core::Core(CkArgMsg *msg)
{
    //if (msg->argc > 1) someParam1 = atoi(msg->argv[1]);
    //if (msg->argc > 2) someParam2 = atoi(msg->argv[2]);
    //if (msg->argc > 3) someParam3 = atoi(msg->argv[3]);
    delete msg;
	msg = nullptr;
    //CcsRegisterHandler("request", (CmiHandler)HandleRequestFromClient);
    CcsRegisterHandler("request", CkCallback(CkIndex_Core::HandleRequestFromClient(nullptr), thisProxy));

    CkPrintf("Running on %d processors...\n", CkNumPes());
    gCore = thisProxy;

    //mBrain = CProxy_BrainBase::ckNew("ThresholdBrain", "");
	mRequestHandler = new RequestHandler(this);
    mStart = CmiWallTimer();
}

void Core::Exit()
{
	delete mRequestHandler;

    CkPrintf("Exitting after %lf...\n", CmiWallTimer() - mStart);
    CkExit();
}

void Core::SendResponseToClient(RequestId token, std::vector<uint8_t> &response)
{
    CcsSendDelayedReply(mTokens[token], response.size(), response.data());
}

#include "core.def.h"
