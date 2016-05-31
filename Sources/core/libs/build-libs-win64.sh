#!/bin/sh

die()
{
    echo "Fatal error. Aborting."
    exit 1
}

build_charm()
{
    echo "Building Charm++"
    CHARM_VERSION=6.7.1
    CHARM_LINK=http://charm.cs.illinois.edu/distrib/charm-6.7.1.tar.gz

    echo "...cleaning"
    rm -r -f charm

    if [ ! -f "charm-${CHARM_VERSION}.tar.gz" ]
    then
        wget --no-check-certificate --output-document="charm-${CHARM_VERSION}.tar.gz" $CHARM_LINK
    fi

    echo "...unpacking"
    tar --extract --gzip --file="charm-${CHARM_VERSION}.tar.gz"
    mv charm-$CHARM_VERSION charm
    cd charm

    echo "...fixing scripts"

    cat src/arch/win64/unix2nt_cc | sed 's/\/I`cygpath -d \\"$SDK_DIR\/Include\\"`/\/I`cygpath -d \\"$SDK_DIR\/Include\/$WindowsSDKLibVersion\/shared\\"` \/I`cygpath -d \\"$SDK_DIR\/Include\/$WindowsSDKLibVersion\/um\\"`/g' > src/arch/win64/unix2nt_cc.tmp
    mv -f src/arch/win64/unix2nt_cc.tmp src/arch/win64/unix2nt_cc

    cat src/arch/win64/unix2nt_cc | sed 's/$SDK_DIR\/Lib\/x64/$SDK_DIR\/Lib\/$WindowsSDKLibVersion\/um\/x64/g' > src/arch/win64/unix2nt_cc.tmp
    mv -f src/arch/win64/unix2nt_cc.tmp src/arch/win64/unix2nt_cc
    chmod +x src/arch/win64/unix2nt_cc

    ./build charm++ net-win64 smp --destination=net-debug -g -no-optimize 2>&1 | tee net-debug.log
    ./build charm++ net-win64 smp --destination=net-release --with-production -j8 | tee net-release.log

    cd ..
}

build_tbb()
{
    echo "Building TBB"
    local TBB_VERSION=44_20160526
    local TBB_LINK="https://www.threadingbuildingblocks.org/sites/default/files/software_releases/windows/tbb${TBB_VERSION}oss_win.zip"

    download_and_unzip "$TBB_LINK" "tbb" "$TBB_VERSION" "tbb${TBB_VERSION}oss"

    cp -f -r tbb/bin/intel64/vc14/* tbb/bin
    cp -f -r tbb/lib/intel64/vc14/* tbb/lib
}

build_sparsehash()
{
    echo "Building sparsehash"
    local SPARSEHASH_VERSION=2.0.3
    local SPARSEHASH_LINK="https://github.com/sparsehash/sparsehash/archive/sparsehash-${SPARSEHASH_VERSION}.zip"

    download_and_unzip "$SPARSEHASH_LINK" "sparsehash" "$SPARSEHASH_VERSION" "sparsehash-sparsehash-${SPARSEHASH_VERSION}"

    echo "...fixing scripts"

    for FILE in \
            "sparsehash/src/windows/config.h" \
            "sparsehash/src/windows/sparsehash/internal/sparseconfig.h" \
            "sparsehash/src/windows/google/sparsehash/sparseconfig.h"
    do
        sed -i -e 's/stdext/std/g' -e 's/<hash_map>/<unordered_map>/g' -e 's/<hash_set>/<unordered_set>/g' "$FILE"
    done

    cp -f -r sparsehash/src/windows/* sparsehash/src
}

build_json()
{
    echo "Building json"
    local JSON_VERSION=1.1.0
    local JSON_LINK="https://github.com/nlohmann/json/archive/v${JSON_VERSION}.zip"

    download_and_unzip "$JSON_LINK" "json" "$JSON_VERSION"
}

build_flatbuffers()
{
    echo "Building FlatBuffers"
    local FLATBUFFERS_VERSION=1.3.0
    local FLATBUFFERS_LINK="https://github.com/google/flatbuffers/archive/v${FLATBUFFERS_VERSION}.zip"
    
    download_and_unzip "$FLATBUFFERS_LINK" "flatbuffers" "$FLATBUFFERS_VERSION"

    echo "...more cleaning"
    rm -r -f bin
    
    cd flatbuffers
    
    echo "...building"
    
    devenv.exe /Upgrade build/VS2010/FlatBuffers.sln
    msbuild.exe build/VS2010/FlatBuffers.sln /property:Configuration=Release /property:Platform=x64
    
    mkdir bin
    mv build/VS2010/x64/Release/* bin

    cd ..
}

build_catch()
{
    echo "Getting Catch"
    local CATCH_VERSION=1.5.4
    local CATCH_LINK="https://github.com/philsquared/Catch/archive/v${CATCH_VERSION}.zip"

    download_and_unzip "$CATCH_LINK" "catch" "$CATCH_VERSION" "Catch-${CATCH_VERSION}"
}

download_and_unzip()
{
    local LINK="$1"
    local TARGET_DIR="$2"
    local VERSION="$3"
    local UNPACKED_DIR="$4"  # Optional.

    [ -n "$UNPACKED_DIR" ] || UNPACKED_DIR="${TARGET_DIR}-${VERSION}"

    local ARCHIVE_NAME="${TARGET_DIR}-${VERSION}.zip"

    echo "...cleaning"
    rm -r -f "$TARGET_DIR"

    redownload_package "$LINK" "$ARCHIVE_NAME"

    echo "...unpacking"
    unzip -q -n "$ARCHIVE_NAME"  || die
    mv "$UNPACKED_DIR" "$TARGET_DIR" || die
}

redownload_package()
{
    local PACKAGE_LINK="$1"
    local ARCHIVE_NAME="$2"

    if [ ! -f "$ARCHIVE_NAME" ]
    then
        wget --no-check-certificate --output-document="${ARCHIVE_NAME}" "$PACKAGE_LINK" || die
    fi
}

if [ -z "$1" ]
then
    echo "Nothing to build."
    echo
    echo "Usage:"
    echo "  $0 <list> <of> <dependencies>"
    echo "(use \"all\" to build all)"
    exit 1
fi

for option
do
    case $option in
        charm)
            build_charm
        ;;
        tbb)
            build_tbb
        ;;
        sparsehash)
            build_sparsehash
        ;;
        json)
            build_json
        ;;
        flatbuffers)
            build_flatbuffers
        ;;
        catch)
            build_catch
        ;;
        all)
            build_charm
            build_tbb
            build_sparsehash
            build_json
            build_flatbuffers
            build_catch
        ;;
        *)
            echo "Unknown option '${option}'"
            exit 1
        ;;
    esac
done

echo
echo "Dependencies have been built successfully"
