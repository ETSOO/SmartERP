#!/bin/bash

# Avoid running multiple times
# When the certificates are expired, you can remove the files and run this script again
# export the 
if [ ! -e "/var/etsoo/certs/dev.pem" ]; then
    # Install mkcert
    apt update
    apt install -y libnss3-tools
    apt install -y mkcert

    # Run mkcert
    # Certificates will be stored in /var/etsoo/certs, mapped to "certs" volume in docker-compose.
    # The named "certs" volume is also mapped to /etc/nginx/ssl for Nginx to use.
    # Before open the URL in Chrome, import the rootCA.pem to "Trusted Root Certification Authorities" / "受信任的根证书颁发机构" with 管理计算机证书（certlm.msc / certmgr.msc） in your Windows.
    # Bypass the proxy for '*.app.local' in your Windows proxy settings when defined. / 如果使用VPN，添加路由规则：DOMAIN-SUFFIX,app.local,DIRECT
    CAROOT=/var/etsoo/certs mkcert -key-file /var/etsoo/certs/dev.key -cert-file /var/etsoo/certs/dev.pem localhost app.local *.app.local

    echo "Certs created"
else
    echo "Certs already exist"
fi