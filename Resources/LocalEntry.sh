#!/bin/bash

# Copy the certs to /var/etsoo/hostcerts, mapped to ./data/certs for Windows to use
mkdir -p /var/etsoo/hostcerts

cp /var/etsoo/certs/dev.key /var/etsoo/hostcerts/ || {
    echo "Failed to copy key.pem to host" >&2
    exit 1
}

cp /var/etsoo/certs/dev.pem /var/etsoo/hostcerts/ || {
    echo "Failed to copy cert.pem to host" >&2
    exit 1
}

echo "Certs copied to the host"