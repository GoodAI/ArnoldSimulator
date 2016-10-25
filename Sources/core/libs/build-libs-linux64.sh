#!/bin/sh

die()
{
    if [ -n "$1" ]
    then
        echo "Fatal error: $1"
        echo "Aborting."
    else
        echo "Fatal error. Aborting."
    fi

    exit 1
}

obtain_charm()
{
    echo "Building Charm++"
    CHARM_VERSION=6.7.1
    CHARM_LINK="http://charm.cs.illinois.edu/distrib/charm-${CHARM_VERSION}.tar.gz"

    # NOTE: a previous version had UNPACKED_DIR (the 4th param) in the form "charm-$CHARM_VERSION"
    download_and_unpack "$CHARM_LINK" "charm" "$CHARM_VERSION" "charm"

    cd charm

    echo "...fixing scripts"
    
    cat src/arch/util/machine-smp.c | sed '/CmiDestroyLock(comm_mutex);/i   CmiDestroyLock(_smp_mutex); _smp_mutex = 0;' > src/arch/util/machine-smp.c.tmp
    mv -f src/arch/util/machine-smp.c.tmp src/arch/util/machine-smp.c
    
    cat src/xlat-i/xi-Entry.C | sed 's/!(sdagCon || isWhenEntry)/true/g' > src/xlat-i/xi-Entry.C.tmp
    mv -f src/xlat-i/xi-Entry.C.tmp src/xlat-i/xi-Entry.C
    cat src/xlat-i/xi-Message.C | sed 's/CkUnpackFnPtr unpack) {/CkUnpackFnPtr unpack, CkDeallocFnPtr dealloc) {/g' > src/xlat-i/xi-Message.C.tmp
    mv -f src/xlat-i/xi-Message.C.tmp src/xlat-i/xi-Message.C
    cat src/xlat-i/xi-Message.C | sed 's/::unpack);/::unpack,(CkDeallocFnPtr) " << type << "::dealloc);/g' > src/xlat-i/xi-Message.C.tmp
    mv -f src/xlat-i/xi-Message.C.tmp src/xlat-i/xi-Message.C
    cat src/ck-core/charm++.h | sed '/void operator delete(void\* ptr) { CkFreeMsg(ptr); }/i    static void dealloc(void *ptr) { CkFreeMsg(ptr); }' > src/ck-core/charm++.h.tmp
    mv -f src/ck-core/charm++.h.tmp src/ck-core/charm++.h
    
    cat src/libs/ck-libs/completion/completion.ci | sed 's/group CompletionDetector {/group [migratable] CompletionDetector {/g' > src/libs/ck-libs/completion/completion.ci.tmp
    mv -f src/libs/ck-libs/completion/completion.ci.tmp src/libs/ck-libs/completion/completion.ci
    cat src/libs/ck-libs/completion/completion.h | sed '/CompletionDetector();/i    CompletionDetector(CkMigrateMessage *m);' > src/libs/ck-libs/completion/completion.h.tmp
    mv -f src/libs/ck-libs/completion/completion.h.tmp src/libs/ck-libs/completion/completion.h
    cat src/libs/ck-libs/completion/completion.h | sed '/CompletionDetector();/i    void pup(PUP::er &p) {}' > src/libs/ck-libs/completion/completion.h.tmp
    mv -f src/libs/ck-libs/completion/completion.h.tmp src/libs/ck-libs/completion/completion.h
    cat src/libs/ck-libs/completion/completion.C | sed '/CompletionDetector::CompletionDetector()/i CompletionDetector::CompletionDetector(CkMigrateMessage *m) : CBase_CompletionDetector(m) { init(); }' > src/libs/ck-libs/completion/completion.C.tmp
    mv -f src/libs/ck-libs/completion/completion.C.tmp src/libs/ck-libs/completion/completion.C
    
    cat src/ck-ldb/DistributedLB.h | sed '/DistributedLB(const CkLBOptions &);/i    void DistributedLB_init(const CkLBOptions &); virtual void pup(PUP::er &p);' > src/ck-ldb/DistributedLB.h.tmp
    mv -f src/ck-ldb/DistributedLB.h.tmp src/ck-ldb/DistributedLB.h
    cat src/ck-ldb/DistributedLB.C | sed '/DistributedLB::DistributedLB(const CkLBOptions &opt) : CBase_DistributedLB(opt) {/i void DistributedLB::pup(PUP::er &p) { if (p.isUnpacking()) DistributedLB_init(CkLBOptions(0)); }' > src/ck-ldb/DistributedLB.C.tmp
    mv -f src/ck-ldb/DistributedLB.C.tmp src/ck-ldb/DistributedLB.C
    cat src/ck-ldb/DistributedLB.C | sed 's/\(DistributedLB::DistributedLB(const CkLBOptions &opt) : CBase_DistributedLB(opt) {\)/\1 DistributedLB_init(opt); } void DistributedLB::DistributedLB_init(const CkLBOptions \&opt) { /g' > src/ck-ldb/DistributedLB.C.tmp
    mv -f src/ck-ldb/DistributedLB.C.tmp src/ck-ldb/DistributedLB.C
    
    cat src/ck-ldb/DistBaseLB.h | sed '/DistBaseLB(const CkLBOptions &);/i    void DistBaseLB_init(const CkLBOptions &); virtual void pup(PUP::er &p);' > src/ck-ldb/DistBaseLB.h.tmp
    mv -f src/ck-ldb/DistBaseLB.h.tmp src/ck-ldb/DistBaseLB.h
    cat src/ck-ldb/DistBaseLB.C | sed '/DistBaseLB::DistBaseLB(const CkLBOptions &opt): CBase_DistBaseLB(opt) {/i void DistBaseLB::pup(PUP::er &p) { if (p.isUnpacking()) DistBaseLB_init(CkLBOptions(0)); }' > src/ck-ldb/DistBaseLB.C.tmp
    mv -f src/ck-ldb/DistBaseLB.C.tmp src/ck-ldb/DistBaseLB.C
    cat src/ck-ldb/DistBaseLB.C | sed 's/\(DistBaseLB::DistBaseLB(const CkLBOptions &opt): CBase_DistBaseLB(opt) {\)/\1  DistBaseLB_init(opt); } void DistBaseLB::DistBaseLB_init(const CkLBOptions \&opt) { /g' > src/ck-ldb/DistBaseLB.C.tmp
    mv -f src/ck-ldb/DistBaseLB.C.tmp src/ck-ldb/DistBaseLB.C

    cat src/ck-ldb/DistributedLB.C | sed '/rand_nbor1 = rand() % CkNumPes();/i    if (CkNumPes() <= 1) break;' > src/ck-ldb/DistributedLB.C.tmp
    mv -f src/ck-ldb/DistributedLB.C.tmp src/ck-ldb/DistributedLB.C
    cat src/ck-ldb/DistributedLB.C | sed '/rand_nbor2 = rand() % CkNumPes();/i    if (CkNumPes() <= 2) break;' > src/ck-ldb/DistributedLB.C.tmp
    mv -f src/ck-ldb/DistributedLB.C.tmp src/ck-ldb/DistributedLB.C
    cat src/ck-ldb/DistributedLB.C | sed '/thisProxy\[rand_nbor1\].GossipLoadInfo(req_hop, CkMyPe(), info_count, p, l);/i    if (CkNumPes() > 1)' > src/ck-ldb/DistributedLB.C.tmp
    mv -f src/ck-ldb/DistributedLB.C.tmp src/ck-ldb/DistributedLB.C
    cat src/ck-ldb/DistributedLB.C | sed '/thisProxy\[rand_nbor2\].GossipLoadInfo(req_hop, CkMyPe(), info_count, p, l);/i    if (CkNumPes() > 2)' > src/ck-ldb/DistributedLB.C.tmp
    mv -f src/ck-ldb/DistributedLB.C.tmp src/ck-ldb/DistributedLB.C
    
    CHARM_COMMON_FLAGS=--enable-lbuserdata
    # the parallel build flag (-j8) is ignored by the build script for the debug build and cannot be enabled for some specific reason of the debug build
    ./build charm++ netlrts-linux-x86_64 smp $CHARM_COMMON_FLAGS --destination=net-debug -g -no-optimize 2>&1 | tee net-debug.log
    ./build charm++ netlrts-linux-x86_64 smp $CHARM_COMMON_FLAGS --destination=net-release --with-production -j8 | tee net-release.log

    cd ..
}

obtain_charmdebug()
{
    echo "Building Charm Debug"
    local CHARMDEBUG_VERSION=6.5.1
    local CHARMDEBUG_LINK="http://charm.cs.illinois.edu/distrib/binaries/charmdebug/charmdebug_${CHARMDEBUG_VERSION}.tar.gz"

    download_and_unpack "$CHARMDEBUG_LINK" "charmdebug" "$CHARMDEBUG_VERSION" "charmdebug"
}

obtain_projections()
{
    echo "Building Projections"
    local PROJECTIONS_VERSION=6.7.0
    local PROJECTIONS_LINK="http://charm.cs.illinois.edu/distrib/binaries/projections/projections_${PROJECTIONS_VERSION}.tar.gz"

    download_and_unpack "$PROJECTIONS_LINK" "projections" "$PROJECTIONS_VERSION" "projections_${PROJECTIONS_VERSION}"
}

obtain_tbb()
{
    echo "Building TBB"
    local TBB_VERSION=44_20160526
    local TBB_LINK="https://www.threadingbuildingblocks.org/sites/default/files/software_releases/linux/tbb${TBB_VERSION}oss_lin_1.tgz"

    download_and_unpack "$TBB_LINK" "tbb" "$TBB_VERSION" "tbb${TBB_VERSION}oss"

    cp -f -r tbb/lib/intel64/gcc4.4/* tbb/bin
    cp -f -r tbb/lib/intel64/gcc4.4/* tbb/lib
}

obtain_sparsehash()
{
    echo "Building sparsehash"
    local SPARSEHASH_VERSION=2.0.3
    local SPARSEHASH_LINK="https://github.com/sparsehash/sparsehash/archive/sparsehash-${SPARSEHASH_VERSION}.zip"

    download_and_unpack "$SPARSEHASH_LINK" "sparsehash" "$SPARSEHASH_VERSION" "sparsehash-sparsehash-${SPARSEHASH_VERSION}"

    cd sparsehash

    echo "..building"
    ./configure
    make

    cd ..
}

obtain_json()
{
    echo "Building json"
    local JSON_VERSION=1.1.0
    local JSON_LINK="https://github.com/nlohmann/json/archive/v${JSON_VERSION}.zip"

    download_and_unpack "$JSON_LINK" "json" "$JSON_VERSION"
}

obtain_flatbuffers()
{
    echo "Building FlatBuffers"
    local FLATBUFFERS_VERSION=1.3.0
    local FLATBUFFERS_LINK="https://github.com/google/flatbuffers/archive/v${FLATBUFFERS_VERSION}.zip"
    
    download_and_unpack "$FLATBUFFERS_LINK" "flatbuffers" "$FLATBUFFERS_VERSION"
   
    cd flatbuffers
    
    echo "...building"
    cmake -G "Unix Makefiles"
    make
    
    mkdir bin
    mv flat* bin
    mkdir lib
    mv libflat* lib

    cd ..
}

obtain_catch()
{
    echo "Getting Catch"
    local CATCH_VERSION=1.5.4
    local CATCH_LINK="https://github.com/philsquared/Catch/archive/v${CATCH_VERSION}.zip"

    download_and_unpack "$CATCH_LINK" "catch" "$CATCH_VERSION" "Catch-${CATCH_VERSION}"
}

download_and_unpack()
{
    local LINK="$1"
    local TARGET_DIR="$2"
    local VERSION="$3"
    local UNPACKED_DIR="$4"  # Optional.

    local ARCHIVE_NAME=$( get_archive_file_name "$LINK" "$TARGET_DIR" "$VERSION" )
    local ARCHIVE_TYPE=$( get_archive_extension "$ARCHIVE_NAME" )
    [ -n "$ARCHIVE_TYPE" ] || die "Unknown archive type '${ARCHIVE_TYPE}' or wrong archive name '${ARCHIVE_NAME}'"

    echo "...cleaning"
    rm -r -f "$TARGET_DIR"

    redownload_package "$LINK" "$ARCHIVE_NAME"

    echo "...unpacking"
    if [ "$ARCHIVE_TYPE" == 'zip' ]
    then
        unzip -q -n "$ARCHIVE_NAME"  || die
    elif [ "$ARCHIVE_TYPE" == 'tar.gz' ] || [ "$ARCHIVE_TYPE" == 'tgz' ]
    then
        tar --extract --gzip --file="$ARCHIVE_NAME"
    else
        die "Unexpected archive type: ${ARCHIVE_TYPE}."
    fi

    if [ -d "${TARGET_DIR}-${VERSION}" ] || [ -z "$UNPACKED_DIR" ]
    then
        UNPACKED_DIR="${TARGET_DIR}-${VERSION}"
    fi

    if [ "$UNPACKED_DIR" != "$TARGET_DIR" ]
    then
        mv "$UNPACKED_DIR" "$TARGET_DIR" || die
    fi

    [ -d "$TARGET_DIR" ] || die "We did not produce the target directory ${TARGET_DIR}!"
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

get_archive_file_name()
{
    local LINK="$1"
    local TARGET_DIR="$2"
    local VERSION="$3"

    local SITE=$( echo ${LINK} | cut -d / -f 3 )

    if [ ${SITE} = 'github.com' ]
    then
        echo "${TARGET_DIR}-${VERSION}.zip"
    else
        echo ${LINK} | sed -e 's|^http.*/\([^/]*\)$|\1|'
    fi
}

get_archive_extension()
{
    local FILE="$1"

    local DOUBLE_EXT=$( echo "$FILE" | rev | cut -d \. -f -2 | rev )

    if [ "$DOUBLE_EXT" == 'tar.gz' ]
    then
        echo "$DOUBLE_EXT"
        return
    fi

    local EXT=$( echo "$DOUBLE_EXT" | cut -d \. -f 2 )

    if [ "$EXT" == 'zip' ] || [ "$EXT" == 'tgz' ]
    then
        echo "$EXT"       
    fi
}

run_tests()
{
    # test get_archive_file_name
    local ARCHIVE_NAME

    ARCHIVE_NAME=$( get_archive_file_name "http://charm.cs.illinois.edu/distrib/charm-6.7.1.tar.gz" "foo" "0.1" )
    [ "$ARCHIVE_NAME" == 'charm-6.7.1.tar.gz' ] || die "Test failed, wrong ARCHIVE_NAME: ${ARCHIVE_NAME}"

    ARCHIVE_NAME=$( get_archive_file_name "https://www.threadingbuildingblocks.org/sites/default/bla_bla_bla/tbb44_20160526oss_win.zip" "foo" "0.1" )
    [ "$ARCHIVE_NAME" == 'tbb44_20160526oss_win.zip' ] || die "Test failed, wrong ARCHIVE_NAME: ${ARCHIVE_NAME}"

    ARCHIVE_NAME=$( get_archive_file_name "https://github.com/philsquared/Catch/archive/v1.5.3.zip" "catch" "1.5.3" )
    [ "$ARCHIVE_NAME" == 'catch-1.5.3.zip' ] || die "Test failed, wrong ARCHIVE_NAME: ${ARCHIVE_NAME}"

    ARCHIVE_NAME=$( get_archive_file_name  "https://github.com/sparsehash/sparsehash/archive/sparsehash-2.0.3.zip" "sparsehash" "2.0.3" )
    [ "$ARCHIVE_NAME" == 'sparsehash-2.0.3.zip' ] || die "Test failed, wrong ARCHIVE_NAME: ${ARCHIVE_NAME}"

    # test get_archive_extension
    local EXT
    EXT=$( get_archive_extension 'charm-6.7.1.tar.gz' )
    [ "$EXT" == 'tar.gz' ] || die "Test failed, wrong EXT: ${EXT}"

    EXT=$( get_archive_extension 'catch-1.5.3.zip' )
    [ "$EXT" == 'zip' ] || die "Test failed, wrong EXT: ${EXT}"

    EXT=$( get_archive_extension 'foo-1.5.3.bar' )
    [ "$EXT" == '' ] || die "Test failed, EXT should be empty: ${EXT}"
}

run_tests

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
            obtain_charm
        ;;
        charmdebug)
            obtain_charmdebug
        ;;
        projections)
            obtain_projections
        ;;
        tbb)
            obtain_tbb
        ;;
        sparsehash)
            obtain_sparsehash
        ;;
        json)
            obtain_json
        ;;
        flatbuffers)
            obtain_flatbuffers
        ;;
        catch)
            obtain_catch
        ;;
        all)
            obtain_charm
            obtain_charmdebug
            obtain_projections
            obtain_tbb
            obtain_sparsehash
            obtain_json
            obtain_flatbuffers
            obtain_catch
        ;;
        *)
            echo "Unknown option '${option}'"
            exit 1
        ;;
    esac
done

echo
echo "Dependencies have been built successfully."
