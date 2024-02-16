#!/usr/bin/env bash
docker build -t prakashpatel107/resourceservice .
docker run -it -p 7001:7001 --env-file .env prakashpatel107/resourceservice