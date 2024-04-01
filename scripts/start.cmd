@echo off
cd ..\nats-server\
start nats-server.exe
cd ..\RankCalculator\RankCalculator\
start dotnet run

cd ..\..\Valuator\
start dotnet run --urls "http://0.0.0.0:5001"
start dotnet run --urls "http://0.0.0.0:5002"

cd D:\nginx-1.25.4
start nginx -c D:\nginx-1.25.4\conf\nginx.conf