#!/bin/sh
cd Results
mkdir checkpoint_new
mkdir checkpoint_old
cp -r -f checkpoint/. checkpoint_old
cd checkpoint
end=`expr $3 - 1`
for a in $(seq 0 1 $end)
do
  condor_transfer_data -name $1 $2.$a
  for b in *
  do
    if cmp -s $b ../checkpoint_old/$b
    then
      echo "skipping"
    else
      cp -f $b ../checkpoint_new/$b
    fi
  done
done
cd ..
cp -r -f checkpoint_old/. checkpoint
cp -r -f checkpoint_new/. checkpoint
rm -r -f checkpoint_old
rm -r -f checkpoint_new
cd ..
