#!/bin/sh

build_flatc()
{
    cd network

    echo "Building FlatBuffers compiler"
    FLATBUFFERS_VERSION=1.3.0
    FLATBUFFERS_LINK=https://github.com/google/flatbuffers/archive/v$FLATBUFFERS_VERSION.zip
    FLATBUFFERS_ARCHIVE=flatbuffers-$FLATBUFFERS_VERSION.zip
    FLATBUFFERS_SOLUTION=build/VS2010/FlatBuffers.sln
    
    echo "...cleaning"
    rm -r -f flatbuffers
    rm -r -f bin

    if [ `ls -1 $FLATBUFFERS_ARCHIVE 2>/dev/null | wc -l` -le 0 ]
    then
        wget --no-check-certificate --output-document=$FLATBUFFERS_ARCHIVE $FLATBUFFERS_LINK
    fi

    echo "...unpacking"
    unzip -q -n $FLATBUFFERS_ARCHIVE
    mv flatbuffers-$FLATBUFFERS_VERSION flatbuffers
    
    cd flatbuffers
    
    echo "...building"
    
    devenv.exe /Upgrade $FLATBUFFERS_SOLUTION
    msbuild.exe $FLATBUFFERS_SOLUTION /property:Configuration=Release /property:Platform=x64
    
    mkdir bin
    mv build/VS2010/x64/Release/* bin/

    cd ../..
}

copy_headers()
{
    cd network/flatbuffers
    
    rm -r -f ../../core/core/flatbuffers
    mkdir ../../core/core/flatbuffers
    cp include/flatbuffers/flatbuffers.h ../../core/core/flatbuffers/
    
    cd ../..
}

build_corelibs()
{
    echo "Building core libs"
    
    cd core/libs
    
    ./build-libs-win64.sh
    
    cd ../..
}

build_messages()
{
    echo "Building messages"
    
    cd network
    
    flatbuffers/bin/flatc.exe --csharp --gen-onefile -o ../UI/ArnoldUI/Network/Messages/ messages/requests.fbs
    mv ../UI/ArnoldUI/Network/Messages/requests.cs ../UI/ArnoldUI/Network/Messages/Requests.cs
    
    flatbuffers/bin/flatc.exe --csharp --gen-onefile -o ../UI/ArnoldUI/Network/Messages/ messages/responses.fbs
    mv ../UI/ArnoldUI/Network/Messages/responses.cs ../UI/ArnoldUI/Network/Messages/Responses.cs
    
    flatbuffers/bin/flatc.exe --cpp -o ../core/core/ messages/requests.fbs
    flatbuffers/bin/flatc.exe --cpp -o ../core/core/ messages/responses.fbs
    
    cd ..
}

for option
do
    case $option in
        flatc)
            build_flatc
            copy_headers
        ;;
        copy-flat-headers)
            copy_headers
        ;;
        corelibs)
            build_corelibs
        ;;
        messages)
            build_messages
        ;;
        all)
            build_flatc
            build_messages
            build_corelibs
        ;;
    esac
done

echo
echo "Dependencies have been built successfully"