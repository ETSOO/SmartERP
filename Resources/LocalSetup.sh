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
    # Certificates will be stored in /var/etsoo/certs, mapped to "certs" volume in docker-compose.
    # The "certs" volume is also mapped to /etc/nginx/ssl for Nginx to use.
    # Before open the URL in Chrome, import the rootCA.pem to "Trusted Root Certification Authorities" / "受信任的根证书颁发机构" with certmgr.msc in your Windows.
    CAROOT=/var/etsoo/certs mkcert -key-file /var/etsoo/certs/key.pem -cert-file /var/etsoo/certs/cert.pem app.local *.app.local
else
    echo "Certs already exist"
fi