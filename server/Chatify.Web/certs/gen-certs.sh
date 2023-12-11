#!/bin/bash

# Set certificate file names
cert_file="my_certificate.crt"
key_file="my_certificate.key"

# Generate a new private key without a passphrase
openssl genpkey -algorithm RSA -out "$key_file"

mkcert -pkcs12 trtest localhost 192.168.1.111 ::1

# Generate a self-signed certificate using the private key
openssl req -new -x509 -key "$key_file" -out "$cert_file" -days 365 -subj "/CN=MyCertificate/O=MyOrganization/C=US"

# Provide information about the certificate
echo "Certificate generated:"
openssl x509 -noout -text -in "$cert_file"

echo "Private key saved as: $key_file"
echo "Certificate saved as: $cert_file"

openssl pkcs12 -export -out myapp