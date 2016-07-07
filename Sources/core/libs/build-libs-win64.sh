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

    cat src/arch/win64/unix2nt_cc | sed 's/\/I`cygpath -d \\"$SDK_DIR\/Include\\"`/\/I`cygpath -d \\"$SDK_DIR\/Include\/$WindowsSDKLibVersion\/shared\\"` \/I`cygpath -d \\"$SDK_DIR\/Include\/$WindowsSDKLibVersion\/um\\"`/g' > src/arch/win64/unix2nt_cc.tmp
    mv -f src/arch/win64/unix2nt_cc.tmp src/arch/win64/unix2nt_cc

    cat src/arch/win64/unix2nt_cc | sed 's/$SDK_DIR\/Lib\/x64/$SDK_DIR\/Lib\/$WindowsSDKLibVersion\/um\/x64/g' > src/arch/win64/unix2nt_cc.tmp
    mv -f src/arch/win64/unix2nt_cc.tmp src/arch/win64/unix2nt_cc
    
    cat src/arch/win64/unix2nt_cc | sed 's/\/D_WINDOWS \/DNOMINMAX/\/D_WINDOWS \/DNOMINMAX \/D_ITERATOR_DEBUG_LEVEL=0/g' > src/arch/win64/unix2nt_cc.tmp
    mv -f src/arch/win64/unix2nt_cc.tmp src/arch/win64/unix2nt_cc
    
    chmod +x src/arch/win64/unix2nt_cc
    
    cat src/scripts/Makefile | sed 's/ckloop//g' > src/scripts/Makefile.tmp
    mv -f src/scripts/Makefile.tmp src/scripts/Makefile
    
    (
    echo '#if defined(_MSC_VER)'
    echo '#define CmiMemoryAtomicIncrement(someInt)    InterlockedIncrement(&someInt)'
    echo '#define CmiMemoryAtomicDecrement(someInt)    InterlockedDecrement(&someInt)'
    echo '#define CmiMemoryAtomicFetchAndInc(input,output)   output = InterlockedIncrement(&input) - 1'
    ) > msvc_interlocked.tmp
    
    (
    echo '#if defined(_MSC_VER)'
    echo '#define CmiMemoryReadFence()                 MemoryBarrier()'
    echo '#define CmiMemoryWriteFence()                MemoryBarrier()'
    ) > msvc_barrier.tmp
    
    cat src/conv-core/converse.h | sed '/#if CMK_C_SYNC_ADD_AND_FETCH_PRIMITIVE/i \/\/MSVC Interlocked' > src/conv-core/converse.h.tmp
    mv -f src/conv-core/converse.h.tmp src/conv-core/converse.h
    cat src/conv-core/converse.h | sed '/\/\/MSVC Interlocked/r msvc_interlocked.tmp' > src/conv-core/converse.h.tmp
    mv -f src/conv-core/converse.h.tmp src/conv-core/converse.h
    cat src/conv-core/converse.h | sed 's/#if CMK_C_SYNC_ADD_AND_FETCH_PRIMITIVE/#elif CMK_C_SYNC_ADD_AND_FETCH_PRIMITIVE/g' > src/conv-core/converse.h.tmp
    mv -f src/conv-core/converse.h.tmp src/conv-core/converse.h
    
    cat src/conv-core/converse.h | sed '/#if CMK_C_SYNC_SYNCHRONIZE_PRIMITIVE/i \/\/MSVC Barrier' > src/conv-core/converse.h.tmp
    mv -f src/conv-core/converse.h.tmp src/conv-core/converse.h
    cat src/conv-core/converse.h | sed '/\/\/MSVC Barrier/r msvc_barrier.tmp' > src/conv-core/converse.h.tmp
    mv -f src/conv-core/converse.h.tmp src/conv-core/converse.h
    cat src/conv-core/converse.h | sed 's/#if CMK_C_SYNC_SYNCHRONIZE_PRIMITIVE/#elif CMK_C_SYNC_SYNCHRONIZE_PRIMITIVE/g' > src/conv-core/converse.h.tmp
    mv -f src/conv-core/converse.h.tmp src/conv-core/converse.h
    
    rm -f msvc_interlocked.tmp
    rm -f msvc_barrier.tmp
    
    cat src/conv-core/converse.h | sed 's/typedef HANDLE CmiNodeLock;/typedef CRITICAL_SECTION *CmiNodeLock;/g' > src/conv-core/converse.h.tmp
    mv -f src/conv-core/converse.h.tmp src/conv-core/converse.h
    cat src/conv-core/converse.h | sed 's/#define CmiLock(lock) (WaitForSingleObject(lock, INFINITE))/#define CmiLock(lock) (EnterCriticalSection(lock))/g' > src/conv-core/converse.h.tmp
    mv -f src/conv-core/converse.h.tmp src/conv-core/converse.h
    cat src/conv-core/converse.h | sed 's/#define CmiUnlock(lock) (ReleaseMutex(lock))/#define CmiUnlock(lock) (LeaveCriticalSection(lock))/g' > src/conv-core/converse.h.tmp
    mv -f src/conv-core/converse.h.tmp src/conv-core/converse.h
    cat src/conv-core/converse.h | sed 's/#define CmiTryLock(lock) (WaitForSingleObject(lock, 0))/#define CmiTryLock(lock) (!TryEnterCriticalSection(lock))/g' > src/conv-core/converse.h.tmp
    mv -f src/conv-core/converse.h.tmp src/conv-core/converse.h
    
    cat src/arch/util/machine-smp.c | sed 's/static HANDLE comm_mutex;/static CmiNodeLock comm_mutex;/g' > src/arch/util/machine-smp.c.tmp
    mv -f src/arch/util/machine-smp.c.tmp src/arch/util/machine-smp.c
    cat src/arch/util/machine-smp.c | sed 's/#define CmiCommLock() (WaitForSingleObject(comm_mutex, INFINITE))/#define CmiCommLock() (EnterCriticalSection(comm_mutex))/g' > src/arch/util/machine-smp.c.tmp
    mv -f src/arch/util/machine-smp.c.tmp src/arch/util/machine-smp.c
    cat src/arch/util/machine-smp.c | sed 's/#define CmiCommUnlock() (ReleaseMutex(comm_mutex))/#define CmiCommUnlock() (LeaveCriticalSection(comm_mutex))/g' > src/arch/util/machine-smp.c.tmp
    mv -f src/arch/util/machine-smp.c.tmp src/arch/util/machine-smp.c
    cat src/arch/util/machine-smp.c | sed 's/HANDLE hMutex = CreateMutex(NULL, FALSE, NULL);/CmiNodeLock lk = (CmiNodeLock)malloc(sizeof(CRITICAL_SECTION)); _MEMCHECK(lk);/g' > src/arch/util/machine-smp.c.tmp
    mv -f src/arch/util/machine-smp.c.tmp src/arch/util/machine-smp.c
    cat src/arch/util/machine-smp.c | sed 's/return hMutex;/InitializeCriticalSectionAndSpinCount(lk, 2048); return lk;/g' > src/arch/util/machine-smp.c.tmp
    mv -f src/arch/util/machine-smp.c.tmp src/arch/util/machine-smp.c
    cat src/arch/util/machine-smp.c | sed 's/CloseHandle(lk);/if (lk) { DeleteCriticalSection(lk); free(lk); }/g' > src/arch/util/machine-smp.c.tmp
    mv -f src/arch/util/machine-smp.c.tmp src/arch/util/machine-smp.c
    cat src/arch/util/machine-smp.c | sed 's/barrier_mutex = CmiCreateLock();/barrier_mutex = CreateMutex(NULL, FALSE, NULL);/g' > src/arch/util/machine-smp.c.tmp
    mv -f src/arch/util/machine-smp.c.tmp src/arch/util/machine-smp.c
    cat src/arch/util/machine-smp.c | sed 's/CloseHandle(comm_mutex);/CmiDestroyLock(comm_mutex);/g' > src/arch/util/machine-smp.c.tmp
    mv -f src/arch/util/machine-smp.c.tmp src/arch/util/machine-smp.c
    cat src/arch/util/machine-smp.c | sed 's/CloseHandle(CmiMemLock_lock);/CmiDestroyLock(CmiMemLock_lock);/g' > src/arch/util/machine-smp.c.tmp
    mv -f src/arch/util/machine-smp.c.tmp src/arch/util/machine-smp.c
    cat src/arch/util/machine-smp.c | sed 's/CloseHandle(cmiMemoryLock);/CmiDestroyLock(cmiMemoryLock);/g' > src/arch/util/machine-smp.c.tmp
    mv -f src/arch/util/machine-smp.c.tmp src/arch/util/machine-smp.c
    cat src/arch/util/machine-smp.c | sed '/barrier_mutex = CreateMutex(NULL, FALSE, NULL);/i   _smp_mutex = CmiCreateLock();' > src/arch/util/machine-smp.c.tmp
    mv -f src/arch/util/machine-smp.c.tmp src/arch/util/machine-smp.c
    cat src/arch/util/machine-smp.c | sed '/CmiDestroyLock(comm_mutex);/i   CmiDestroyLock(_smp_mutex); _smp_mutex = 0;' > src/arch/util/machine-smp.c.tmp
    mv -f src/arch/util/machine-smp.c.tmp src/arch/util/machine-smp.c
    
    ./build charm++ net-win64 smp --destination=net-debug -g -no-optimize 2>&1 | tee net-debug.log
    ./build charm++ net-win64 smp --destination=net-release --with-production -j8 | tee net-release.log

    cd ..
}

obtain_tbb()
{
    echo "Building TBB"
    local TBB_VERSION=44_20160526
    local TBB_LINK="https://www.threadingbuildingblocks.org/sites/default/files/software_releases/windows/tbb${TBB_VERSION}oss_win.zip"

    download_and_unpack "$TBB_LINK" "tbb" "$TBB_VERSION" "tbb${TBB_VERSION}oss"

    cp -f -r tbb/bin/intel64/vc14/* tbb/bin
    cp -f -r tbb/lib/intel64/vc14/* tbb/lib
}

obtain_sparsehash()
{
    echo "Building sparsehash"
    local SPARSEHASH_VERSION=2.0.3
    local SPARSEHASH_LINK="https://github.com/sparsehash/sparsehash/archive/sparsehash-${SPARSEHASH_VERSION}.zip"

    download_and_unpack "$SPARSEHASH_LINK" "sparsehash" "$SPARSEHASH_VERSION" "sparsehash-sparsehash-${SPARSEHASH_VERSION}"

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

    [ -n "$UNPACKED_DIR" ] || UNPACKED_DIR="${TARGET_DIR}-${VERSION}"

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
    elif [ "$ARCHIVE_TYPE" == 'tar.gz' ]
    then
        tar --extract --gzip --file="$ARCHIVE_NAME"
    else
        die "Unexpected archive type: ${ARCHIVE_TYPE}."
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

    if [ "$EXT" == 'zip' ]
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
