#pragma once

#include <atomic>
#include <unordered_map>

#include "converse.h"
#include "conv-ccs.h"

#include "common.h"
#include "core.decl.h"
#include "brain.decl.h"

class Core : public CBase_Core
{
public:
    Core(CkArgMsg *msg);

    void Exit();

    static void HandleRequestFromClient(char *request);

    void SendResponseToClient(RequestId token, std::vector<unsigned char> &response);

private:
    double mStart;
    CProxy_BrainBase mBrain;

    static std::atomic<RequestId> mRequestCounter;
    static std::unordered_map<RequestId, CcsDelayedReply> mTokens;
};
