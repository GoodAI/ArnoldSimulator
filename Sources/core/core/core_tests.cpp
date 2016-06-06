#include "catch.hpp"

#include "core.h"
#include "region.h"

#include "core_tests.h"

unsigned int Factorial(unsigned int number)
{
    return number <= 1 ? number : Factorial(number - 1)*number;
}

TEST_CASE("Factorials are computed", "[factorial]")
{
    REQUIRE(Factorial(1) == 1);
    REQUIRE(Factorial(2) == 2);
    REQUIRE(Factorial(3) == 6);
    REQUIRE(Factorial(10) == 3628800);
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
