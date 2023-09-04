#!/bin/bash
redis-cli KEYS "message:*" | xargs redis-cli DEL
redis-cli KEYS "groups:*" | xargs redis-cli DEL
redis-cli KEYS "user:*" | xargs redis-cli DEL