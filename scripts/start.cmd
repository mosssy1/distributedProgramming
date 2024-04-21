cd ..\RankCalculator\
start dotnet run

cd ..\nats-server\
start nats-server.exe

cd ..\EventsLogger\
start dotnet run
start dotnet run

cd ..\Valuator\
start dotnet run --urls "http://localhost:5001"
start dotnet run --urls "http://localhost:5002"

cd D:\nginx-1.25.4
start nginx -c D:\nginx-1.25.4\conf\nginx.conf