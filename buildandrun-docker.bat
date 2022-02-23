@echo off
echo build Image.
docker build -t automation .
Call :setError
Echo %errorlevel%
Goto :eof
:setError
Exit /B 5
echo Run Container.
docker run -d -p 8090:80 --name automation automation
echo Opening in Browser to WebApp.
Start http://localhost:8090