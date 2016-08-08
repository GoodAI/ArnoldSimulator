#pragma once
#include <map>
#include <memory>
#include "registration.h"

// A template registry that registers an instance for a given name.
// Keep in mind that the instance must not hold any fields as they are
// not distributed to the other nodes. Used for synapse and spike editors.
template<typename TInstance, typename TToken>
class InstanceCache : public Registration<InstanceCache<TInstance, TToken>, TToken>
{
    using Base = Registration<InstanceCache<TInstance, TToken>, TToken>;
public:
    TToken Register(const std::string &name, TInstance * component)
    {
        TToken token = Registration<InstanceCache<TInstance, TToken>, TToken>::GetNewToken(name);
        mInstances[token].reset(component);

        return token;
    }

    TInstance *Get(TToken token) const
    {
        return mInstances.at(token).get();
    }

private:
    std::map<TToken, std::unique_ptr<TInstance>> mInstances;
};
