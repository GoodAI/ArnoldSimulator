#pragma once
#include <unordered_map>
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
        TToken token = Base::GetNewToken(name);
        mInstances[token].reset(component);

        return token;
    }

    TInstance *Get(TToken token) const
    {
        return mInstances.at(token).get();
    }

    TInstance *Get(const std::string &name) const
    {
        return Get(Base::GetToken(name));
    }

private:
    std::unordered_map<TToken, std::unique_ptr<TInstance>> mInstances;
};
