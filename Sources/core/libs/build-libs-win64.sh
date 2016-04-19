#!/bin/sh

build_charm()
{
    echo "Building Charm++"
    CHARM_VERSION=6.7.0
    CHARM_LINK=http://charm.cs.illinois.edu/distrib/charm-6.7.0.tar.gz

    echo "...cleaning"
    rm -r -f charm

    if [ `ls -1 charm-$CHARM_VERSION.tar.gz 2>/dev/null | wc -l` -le 0 ]
    then
        wget --no-check-certificate --output-document=charm-$CHARM_VERSION.tar.gz $CHARM_LINK
    fi

    echo "...unpacking"
    tar --extract --gzip --file=charm-$CHARM_VERSION.tar.gz
    mv charm-$CHARM_VERSION charm
    cd charm

    echo "...fixing scripts"

    cat src/arch/win64/unix2nt_cc | sed 's/\/I`cygpath -d \\"$SDK_DIR\/Include\\"`/\/I`cygpath -d \\"$SDK_DIR\/Include\/$WindowsSDKLibVersion\/shared\\"` \/I`cygpath -d \\"$SDK_DIR\/Include\/$WindowsSDKLibVersion\/um\\"`/g' > src/arch/win64/unix2nt_cc_tmp
	cat src/arch/win64/unix2nt_cc_tmp > src/arch/win64/unix2nt_cc
	rm src/arch/win64/unix2nt_cc_tmp
	
    cat src/arch/win64/unix2nt_cc | sed 's/$SDK_DIR\/Lib\/x64/$SDK_DIR\/Lib\/$WindowsSDKLibVersion\/um\/x64/g' > src/arch/win64/unix2nt_cc_tmp
	cat src/arch/win64/unix2nt_cc_tmp > src/arch/win64/unix2nt_cc
	rm src/arch/win64/unix2nt_cc_tmp

    ./build charm++ net-win64 --destination=net-debug -g -no-optimize 2>&1 | tee net-debug.log
    ./build charm++ net-win64 --destination=net-release --with-production -j8 | tee net-release.log

    cd ..
}

build_tbb()
{
    echo "Building TBB"
    TBB_VERSION=44_20160128
    TBB_LINK=https://www.threadingbuildingblocks.org/sites/default/files/software_releases/windows/tbb44_20160128oss_win_0.zip

    echo "...cleaning"
    rm -r -f tbb

    if [ `ls -1 tbb${TBB_VERSION}oss_win_0.zip 2>/dev/null | wc -l` -le 0 ]
    then
        wget --no-check-certificate --output-document=tbb${TBB_VERSION}oss_win_0.zip $TBB_LINK
    fi

    echo "...unpacking"
    unzip -q -n tbb${TBB_VERSION}oss_win_0.zip
    mv tbb${TBB_VERSION}oss tbb

    cp -f -r tbb/bin/intel64/vc14/* tbb/bin
    cp -f -r tbb/lib/intel64/vc14/* tbb/lib
}

build_sparsehash()
{
    echo "Building sparsehash"
    SPARSEHASH_VERSION=2.0.3
    SPARSEHASH_LINK=https://github.com/sparsehash/sparsehash/archive/sparsehash-2.0.3.zip

    echo "...cleaning"
    rm -r -f sparsehash

    if [ `ls -1 sparsehash-${SPARSEHASH_VERSION}.zip 2>/dev/null | wc -l` -le 0 ]
    then
        wget --no-check-certificate --output-document=sparsehash-${SPARSEHASH_VERSION}.zip $SPARSEHASH_LINK
    fi

    echo "...unpacking"
    unzip -q -n sparsehash-${SPARSEHASH_VERSION}.zip
    mv sparsehash-sparsehash-${SPARSEHASH_VERSION} sparsehash
    
    echo "...fixing scripts"

    cat sparsehash/src/windows/config.h | sed 's/stdext/std/g' | sed 's/<hash_map>/<unordered_map>/g' | sed 's/<hash_set>/<unordered_set>/g' > sparsehash/src/windows/config.h
    cat sparsehash/src/windows/sparsehash/internal/sparseconfig.h | sed 's/stdext/std/g' | sed 's/<hash_map>/<unordered_map>/g' | sed 's/<hash_set>/<unordered_set>/g' > sparsehash/src/windows/sparsehash/internal/sparseconfig.h
    cat sparsehash/src/windows/google/sparsehash/sparseconfig.h | sed 's/stdext/std/g' | sed 's/<hash_map>/<unordered_map>/g' | sed 's/<hash_set>/<unordered_set>/g' > sparsehash/src/windows/google/sparsehash/sparseconfig.h

    cp -f -r sparsehash/src/windows/* sparsehash/src
}

build_json()
{
    echo "Building json"
    JSON_VERSION=1.1.0
    JSON_LINK=https://github.com/nlohmann/json/archive/v1.1.0.zip

    echo "...cleaning"
    rm -r -f json

    if [ `ls -1 json-${JSON_VERSION}.zip 2>/dev/null | wc -l` -le 0 ]
    then
        wget --no-check-certificate --output-document=json-${JSON_VERSION}.zip $JSON_LINK
    fi

    echo "...unpacking"
    unzip -q -n json-${JSON_VERSION}.zip
    mv json-${JSON_VERSION} json
}

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
    esac
done

echo
echo "Dependencies have been built successfully"
