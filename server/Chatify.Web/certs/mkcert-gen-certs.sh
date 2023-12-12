#!/bin/bash

cert_name="myapp"

mkcert -pkcs12 $cert_name localhost 192.168.1.111 ::1
mv "$cert_name+3.p12" "$cert_name.pfx"