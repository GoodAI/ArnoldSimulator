@echo off
rmdir /s /q C:\arnold
mkdir C:\arnold
xcopy /S /Y . C:\arnold
rmdir /s /q checkpoint
mkdir C:\arnold\checkpoint
mklink /J checkpoint C:\arnold\checkpoint
C:\arnold\charmd_faceless.exe
