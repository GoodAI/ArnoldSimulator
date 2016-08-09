#include "catch.hpp"

#include "common.h"
#include "core.h"
#include "region.h"

#include "core_tests.h"
#include "mnist_reader.h"
#include "data_utils.h"

#include "components.h"

TEST_CASE("NeuronId packing is correct", "[common]")
{
    const NeuronIndex neuronIndex = 500000;
    const RegionIndex regionIndex = 400;

    REQUIRE(neuronIndex <= NEURON_INDEX_MAX);
    REQUIRE(regionIndex <= REGION_INDEX_MAX);

    NeuronId neuronId = GetNeuronId(regionIndex, neuronIndex);

    REQUIRE(GetNeuronIndex(neuronId) == neuronIndex);
    REQUIRE(GetRegionIndex(neuronId) == regionIndex);
}


extern CProxy_RegionBase gRegions;

const std::string coreTestRegionType("CORE-TEST-Region");

class RogerRegion : public Region
{
public:
    RogerRegion(RegionBase &base, json &params)
        : Region(base, params)
    {
    }

    void pup(PUP::er &p) override {}
    const char * GetType() const override { return coreTestRegionType.c_str(); }
    void Control(size_t brainStep) override {}
    void AcceptContributionFromNeuron(NeuronId neuronId, const uint8_t *contribution, size_t size) override {}
    size_t ContributeToBrain(uint8_t *&contribution) override {
        return 0;
    }
};

void SetupCharmTests()
{
    Box3D box;
    box.first = Point3D(0.0f, 0.0f, 0.0f);
    box.second = Size3D(BOX_DEFAULT_SIZE_X, BOX_DEFAULT_SIZE_Y, BOX_DEFAULT_SIZE_Z);

    RegionFactory *regionFactory = RegionFactory::GetInstance();
    regionFactory->Register(coreTestRegionType, [](RegionBase &base, nlohmann::json &params) -> Region* {
        return new RogerRegion(base, params);
    });

    gRegions[0].insert("Roger", coreTestRegionType, box, "{}");
    gRegions.doneInserting();
}

TEST_CASE("Region can foo", "[charm]")
{
    CProxyElement_RegionBase regionProxy = gRegions[0];

    Box3D box;
    box.first = Point3D(1.0f, 2.0f, 3.0f);
    box.second = Size3D(BOX_DEFAULT_SIZE_X, BOX_DEFAULT_SIZE_Y, BOX_DEFAULT_SIZE_Z);
    regionProxy.SetBox(box);

    RegionBase *region = regionProxy.ckLocal();

    REQUIRE(region != nullptr);
    REQUIRE(strcmp(region->GetName(), "Roger") == 0);
}
