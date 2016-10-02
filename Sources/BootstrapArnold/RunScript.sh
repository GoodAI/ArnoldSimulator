#!/bin/sh
rm -r -f /tmp/arnold
mkdir /tmp/arnold
cp -r -f . /tmp/arnold
SCRATCH_DIR=`pwd`
mkdir -p ./checkpoint
rm -r -f /tmp/arnold/checkpoint
(cd /tmp/arnold && ln -s $SCRATCH_DIR/checkpoint checkpoint)
/tmp/arnold/charmd
