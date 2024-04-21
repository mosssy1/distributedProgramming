@echo off
taskkill /f /im valuator.exe
taskkill /f /im nginx.exe
taskkill /f /im rankCalculator.exe
taskkill /f /im EventsLogger.exe
taskkill /f /im nats-server.exe