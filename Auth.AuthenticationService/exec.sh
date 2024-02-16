#!/usr/bin/env bash
docker build -t prakashpatel107/authbackend .
docker run -it -p 5001:5001 --env-file .env prakashpatel107/authbackend