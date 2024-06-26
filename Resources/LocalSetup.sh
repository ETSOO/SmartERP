#!/bin/bash

# Avoid running multiple times
# When the certificates are expired, you can remove the files and run this script again
# export the 
if [ ! -e "/var/etsoo/certs/key.pem" ]; then
    # Install mkcert
    apt update
    apt install -y libnss3-tools
    apt install -y mkcert

    # Run mkcert
    CAROOT=/var/etsoo/certs mkcert -key-file /var/etsoo/certs/key.pem -cert-file /var/etsoo/certs/cert.pem app.local *.app.local
else
    echo "Certs already exist"
fi