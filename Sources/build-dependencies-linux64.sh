#!/bin/sh

build_corelibs()
{
    echo "Building core libs"
    
    cd core/libs
    
    sh build-libs-linux64.sh all
    
    cd ../..
}

for option
do
    case $option in
        corelibs)
            build_corelibs
        ;;
        all)
            build_corelibs
        ;;
        *)
            echo "Unknown target: '$option'"
            exit 1
        ;;
    esac
done

echo
echo "Dependencies have been built successfully"
