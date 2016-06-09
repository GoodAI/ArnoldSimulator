#include "catch.hpp"

#include "common.h"
#include "core.h"
#include "region.h"

#include "core_tests.h"

TEST_CASE("NeuronId packing is correct", "[common]")
{
    const NeuronIndex neuronIndex = 500'000;
    const RegionIndex regionIndex = 400;

    REQUIRE(neuronIndex <= NEURON_INDEX_MAX);
    REQUIRE(regionIndex <= REGION_INDEX_MAX);

    NeuronId neuronId = GetNeuronId(regionIndex, neuronIndex);

    REQUIRE(GetNeuronIndex(neuronId) == neuronIndex);
    REQUIRE(GetRegionIndex(neuronId) == regionIndex);
}

extern CProxy_RegionBase gRegions;

void SetupCharmTests()
{
    Box3D box;
    box.first = Point3D(0.0f, 0.0f, 0.0f);
    box.second = Size3D(BOX_DEFAULT_SIZE_X, BOX_DEFAULT_SIZE_Y, BOX_DEFAULT_SIZE_Z);

    gRegions[0].insert("Roger", "type", box, "{}");
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
