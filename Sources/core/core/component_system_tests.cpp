#include "catch.hpp"
#include "model_component_factory.h"
#include <string>
#include "instance_cache.h"

class ModelComponentBase {};
class ModelComponent
{
public:
    ModelComponent(const std::string name) : Name(name)
    {
    }

    std::string Name;
};

class TestRegistration : public Registration<ModelComponent, Token8>
{
public:
    Token8 Register(const std::string name, const size_t salt = 0)
    {
        return GetNewToken(name, salt);
    }
};

SCENARIO("Registration book-keeps tokens")
{
    GIVEN("A registration singleton")
    {
        TestRegistration registration;

        WHEN("A token is requested")
        {
            auto token = registration.Register("foo");

            THEN("Its name can be retrieved")
            {
                REQUIRE(registration.GetName(token).compare("foo") == 0);
            }
            AND_THEN("The token can be retrieved again")
            {
                REQUIRE(registration.GetToken("foo") == token);
            }
            AND_THEN("Token request for a non-registered name throws exception")
            {
                REQUIRE_THROWS(registration.GetToken("bar"));
            }
            AND_THEN("Name resolution for non-registered token throws exception")
            {
                REQUIRE_THROWS(registration.GetName(token+1));
            }
        }
    }
}

SCENARIO("Registration hash-collision detection")
{
    GIVEN("A registration full of items")
    {
        TestRegistration registration;

        const int firstCollidingNameAsInt = 86;  // Depends on the hash function used.
        
        for (int number = 0; number < firstCollidingNameAsInt; number++) {
            registration.Register(std::to_string(number));
        }

        std::string collidingName = std::to_string(firstCollidingNameAsInt);

        THEN("Item that yields colliding token throws exception")
        {
            REQUIRE_THROWS(registration.Register(collidingName));
        }

        AND_WHEN("The colliding name is registered with a different 'salt' value")
        {
            auto token = registration.Register(collidingName, 1);

            THEN("I can retrieve the right token in the normal way")
            {
                REQUIRE(registration.GetToken(collidingName) == token);
            }
        }
    }
}

SCENARIO("Component system registers factory functions")
{
    GIVEN("A model component factory")
    {
        ModelComponentFactory<ModelComponent, ModelComponentBase, Token64> factory;

        WHEN("Factory functions are added")
        {
            Token64 oneToken = factory.Register("component1", [](ModelComponentBase &base, nlohmann::json &params) -> ModelComponent* {
                return new ModelComponent("1");
            });
            Token64 lastToken = factory.Register("component2", [](ModelComponentBase &base, nlohmann::json &params) -> ModelComponent* {
                return new ModelComponent("2");
            });

            ModelComponentBase base;
            nlohmann::json params;

            THEN("A correct instance can be created")
            {
                ModelComponent *component = factory.Create(oneToken, base, params);

                REQUIRE(component != nullptr);
                REQUIRE(component->Name.compare("1") == 0);
            }
            AND_THEN("Wrong name throws exception")
            {
                REQUIRE_THROWS(factory.Create("component3", base, params));
            }
            AND_THEN("Wrong token throws exception")
            {
                REQUIRE_THROWS(factory.Create(lastToken + 1, base, params));
            }
        }
    }
}

SCENARIO("Components cache registers instances")
{
    GIVEN("An instance cache")
    {
        InstanceCache<ModelComponent, Token8> cache;

        WHEN("Two instances are registered")
        {
            cache.Register("instance1", new ModelComponent("1"));
            cache.Register("instance2", new ModelComponent("2"));

            THEN("A correct instance is retrieved")
            {
                ModelComponent *instance = cache.Get("instance1");
                REQUIRE(instance->Name.compare("1") == 0);
            }
        }
    }
}
