cd .\Results
mkdir checkpoint_new
mkdir checkpoint_old
xcopy /S /Y checkpoint checkpoint_old
cd .\checkpoint
set /A end = %3 - 1
for /L %%a in (0,1,%end%) do (
  condor_transfer_data -name %1 %2.%%a
  for %%b in (*) do fc %%b ..\checkpoint_old\%%b >NUL && echo "skipping" || copy /Y %%b ..\checkpoint_new\%%b )
cd ..
xcopy /S /Y checkpoint_old checkpoint
xcopy /S /Y checkpoint_new checkpoint
rmdir /s /q checkpoint_old
rmdir /s /q checkpoint_new
cd ..
