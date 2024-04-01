@echo off
taskkill /F /IM "Valuator.exe" /T
taskkill /F /IM "nginx.exe" /T
taskkill /F /IM "RankCalculator.exe" /T
taskkill /F /IM "nats-server.exe" /T