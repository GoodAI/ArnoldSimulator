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
    Token8 Register(const std::string name)
    {
        return GetNewToken(name);
    }
};

TEST_CASE("Registration book-keeps tokens")
{
    GIVEN("A registration singleton")
    {
        TestRegistration registration;

        WHEN("A token is requested")
        {
            Token8 token = registration.Register("foo");

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

TEST_CASE("Component system registers factory functions")
{
    GIVEN("A model component factory")
    {
        ModelComponentFactory<ModelComponent, ModelComponentBase, Token64> factory;

        WHEN("Factory functions are added")
        {
            factory.Register("component1", [](ModelComponentBase &base, nlohmann::json &params) -> ModelComponent* {
                return new ModelComponent("1");
            });
            Token64 lastToken = factory.Register("component2", [](ModelComponentBase &base, nlohmann::json &params) -> ModelComponent* {
                return new ModelComponent("2");
            });

            ModelComponentBase base;
            nlohmann::json params;

            THEN("A correct instance can be created")
            {
                ModelComponent *component = factory.Create(1, base, params);

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

TEST_CASE("Components cache registers instances")
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
