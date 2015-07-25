@echo OFF
pushd "%~dp0.."

rmdir /S /Q "Source\ScpControl\obj"
rmdir /S /Q "Source\ScpInstaller\bin"
rmdir /S /Q "Source\ScpInstaller\obj"
rmdir /S /Q "Source\ScpMonitor\bin"
rmdir /S /Q "Source\ScpMonitor\obj"
rmdir /S /Q "Source\ScpPair\bin"
rmdir /S /Q "Source\ScpPair\obj"
rmdir /S /Q "Source\ScpServer\bin"
rmdir /S /Q "Source\ScpServer\obj"
rmdir /S /Q "Source\ScpService\bin"
rmdir /S /Q "Source\ScpService\obj"
rmdir /S /Q "Source\ScpUser\Build"
rmdir /S /Q "Source\XInput_Scp\Build"
rmdir /S /Q "Source\ipch"

rmdir /S /Q "Sample\D3Mapper\bin"
rmdir /S /Q "Sample\D3Mapper\obj"

rmdir /S /Q "Sample\DskMapper\bin"
rmdir /S /Q "Sample\DskMapper\obj"

rmdir /S /Q "Sample\GtaMapper\bin"
rmdir /S /Q "Sample\GtaMapper\obj"

rmdir /S /Q "Source\ScpBus\bus\objchk_win7_amd64"
rmdir /S /Q "Source\ScpBus\bus\objchk_win7_x86"
rmdir /S /Q "Source\ScpBus\bus\objfre_win7_amd64"
rmdir /S /Q "Source\ScpBus\bus\objfre_win7_x86"

del /S /Q "*.sdf"
del /S /Q "*.user"
del /S /Q "*.log"
del /S /Q "*.aps"
del /S /Q /A:H "*.suo"

del /S /Q "CommonInfo.cs"
del /S /Q "Scp_All.ico"

pushd bin
del /S /Q "*.pdb"
del /S /Q "*.lib"
del /S /Q "*.exp"
del /S /Q "*.exe.metagen"
del /S /Q "*.vshost.exe"
del /S /Q "*.vshost.exe.config"
del /S /Q "*.vshost.exe.manifest"
popd
popd
