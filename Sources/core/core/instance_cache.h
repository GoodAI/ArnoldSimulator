#pragma once
#include <map>
#include <memory>
#include "registration.h"

// A template registry that registers an instance for a given name.
// Keep in mind that the instance must not hold any fields as they are
// not distributed to the other nodes. Used for synapse and spike editors.
template<typename TInstance>
class InstanceCache : Registration<InstanceCache<TInstance>>
{
    using Base = Registration<InstanceCache<TInstance>>;
public:
    ~InstanceCache()
    {
        for (TInstance* instance : mInstances) {
            delete instance;
        }
        mInstances.clear();
    }

    Token Register(const std::string &name, TInstance * component)
    {
        Token token = Registration<InstanceCache<TInstance>>::GetNewToken(name);
        mInstances[token].reset(component);

        return token;
    }

    TInstance *Get(Token token) const
    {
        return mInstances.at(token).get();
    }

private:
    std::map<Token, std::unique_ptr<TInstance>> mInstances;
};
