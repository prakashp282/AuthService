#!/usr/bin/env bash
docker build -t prakashpatel107/identityserver .
docker run -it -p 5007:5007 --env-file .env prakashpatel107/identityserver