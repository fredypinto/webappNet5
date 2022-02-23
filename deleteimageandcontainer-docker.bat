@echo off
echo delete Container.
docker rm -f automation
echo delete Image.
docker rmi -f automation