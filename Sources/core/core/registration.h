#pragma once
#include <cstdint>
#include <unordered_map>
#include "log.h"
#include "common.h"

typedef uint64_t Token64;
typedef uint8_t Token8;

#define RESERVED_TOKEN_MAX  0

template<typename T, typename TToken>
class Registration
{
public:
    Registration()
    {
        if (std::numeric_limits<TToken>::is_signed) {
            TToken foo;
            Log(LogLevel::Error, "Token type is signed (%s)", typeid(foo).name());
            throw std::invalid_argument("Token type must be unsigned");
        }

        if (RESERVED_TOKEN_MAX > (std::numeric_limits<TToken>::max)() / 2) {
            const char * message = "RESERVED_TOKEN_MAX is too high";
            Log(LogLevel::Error, message);
            throw std::invalid_argument(message);
        }
    }

    TToken GetToken(const std::string &name) const
    {
        auto tokenIt = mTokens.find(name);
        if (tokenIt == mTokens.end()) {
            Log(LogLevel::Error, "Item with name %s was not registered", name.c_str());
            throw std::invalid_argument("Name not registered");
        }
        return tokenIt->second;
    }

    std::string GetName(TToken token) const
    {
        auto nameIt = mNames.find(token);
        if (nameIt == mNames.end()) {
            Log(LogLevel::Error, "Token %d does not exist", token);
            throw std::invalid_argument("Token does not exist");
        }
        return nameIt->second;
    }

    static T * GetInstance()
    {
        return &mInstance;
    }

protected:
    TToken GetNewToken(const std::string &name, const size_t salt = 0)
    {
        if (mTokens.find(name) != mTokens.end()) {
            Log(LogLevel::Error, "Name '%s' is already registered", name.c_str());
            throw std::invalid_argument("Name already registered");
        }

        TToken token = GenerateToken(name, salt);
        if (mNames.find(token) != mNames.end()) {
            std::ostringstream message;
            message << "Component registration: Generated token for '"
                << name << "' collides, please use this 'salt' value: " << SuggestHashSeed(name);

            Log(LogLevel::Error, message.str().c_str());
            throw std::overflow_error(message.str());
        }

        //printf("-- Generated token %X (%u) of size %u for '%s'.\n", token, token, sizeof(token), name.c_str());

        mTokens[name] = token;
        mNames[token] = name;

        return token;
    }
private:
    TToken GenerateToken(const std::string &name, const size_t salt)
    {
        auto hash = hash_combine(salt, name);

        TToken token = static_cast<TToken>(hash);

        // Do not move all tokens that collide with reserved tokens to one item; distribute them.
        if (token <= RESERVED_TOKEN_MAX)
            token += RESERVED_TOKEN_MAX + 1;

        return token;
    }

    size_t SuggestHashSeed(const std::string &name)
    {
        for (size_t salt = 1; salt < 1000000; salt++) {
            if (mNames.find(GenerateToken(name, salt)) == mNames.end())
                return salt;
        }

        throw std::overflow_error("Component registration: Generated token collides and no salt found!");
    }


    TToken mNextToken;
    std::unordered_map<std::string, TToken> mTokens;
    std::unordered_map<TToken, std::string> mNames;

    static T mInstance;
};

template<typename T, typename TToken>
T Registration<T, TToken>::mInstance;
