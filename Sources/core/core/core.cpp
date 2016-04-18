#include <cstdio>
#include <cstdlib>

#include "core.h"

CProxy_Core gCore;

std::atomic<RequestId> Core::mRequestCounter = 0;
std::unordered_map<RequestId, CcsDelayedReply> Core::mTokens;

void Core::HandleRequestFromClient(char *request)
{
    if (CcsIsRemoteRequest()) {
        std::string strRequest(request + CmiMsgHeaderSizeBytes);
        std::vector<unsigned char> vecRequest(strRequest.begin(), strRequest.end());
        RequestId requestId = mRequestCounter++;
        mTokens.insert(std::make_pair(requestId, CcsDelayReply()));
        gCore.ckLocal()->mBrain.EnqueueClientRequest(requestId, vecRequest);
    }
}

Core::Core(CkArgMsg *msg)
{
    //if (msg->argc > 1) someParam1 = atoi(msg->argv[1]);
    //if (msg->argc > 2) someParam2 = atoi(msg->argv[2]);
    //if (msg->argc > 3) someParam3 = atoi(msg->argv[3]);
    delete msg;

    CcsRegisterHandler("request", (CmiHandler)HandleRequestFromClient);

    CkPrintf("Running on %d processors...\n", CkNumPes());
    gCore = thisProxy;

    mBrain = CProxy_BrainBase::ckNew("ThresholdBrain", "");
    mStart = CmiWallTimer();
}

void Core::Exit()
{
    CkPrintf("Exitting after %lf...\n", CmiWallTimer() - mStart);
    CkExit();
}

void Core::SendResponseToClient(RequestId token, std::vector<unsigned char> &response)
{
    CcsSendDelayedReply(mTokens[token], response.size(), response.data());
}

#include "core.def.h"
