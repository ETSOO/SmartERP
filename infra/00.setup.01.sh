# Update and upgrade system packages
sudo apt update
sudo apt upgrade -y


# Install Microk8s
sudo snap install microk8s --classic

# Add current user to microk8s group
# sudo usermod -a -G microk8s $USER
# sudo chown -f -R $USER ~/.kube

# Remove Microk8s
# sudo snap remove microk8s

# Check Microk8s status
microk8s status --wait-ready

# Check logs
sudo journalctl -u snap.microk8s.daemon-kubelite -n 100 --no-pager

# registry.k8s.io mirror setup
: << 'BLOCK'
sudo mkdir -p /var/snap/microk8s/current/args/certs.d/registry.k8s.io
sudo vi /var/snap/microk8s/current/args/certs.d/registry.k8s.io/hosts.toml

server = "https://registry.k8s.io"

[host."https://registry.cn-hangzhou.aliyuncs.com/google_containers"]
  capabilities = ["pull", "resolve"]
BLOCK

# docker.io mirror setup
: << 'BLOCK'
sudo mkdir -p /var/snap/microk8s/current/args/certs.d/docker.io
sudo vi /var/snap/microk8s/current/args/certs.d/docker.io/hosts.toml

server = "https://docker.io"

[host."https://docker.1ms.run"]
  capabilities = ["pull", "resolve"]
BLOCK

# docker registry secret
: << 'BLOCK'
microk8s kubectl create secret docker-registry aliyun-acr-secret \
  --namespace=smarterp \
  --docker-server=etsoo-registry.cn-qingdao.cr.aliyuncs.com \
  --docker-username="in**@etsoo.com" \
  --docker-password="你的阿里云访问凭证密码"
BLOCK

# Simplify access to Microk8s commands
which microk8s
sudo snap alias microk8s.kubectl kubectl

# sudo snap unalias kubectl

# Enable add-ons
microk8s enable cert-manager

# traefik + Gateway API CRDs
# Ports: 9100/TCP (metrics), 8080/TCP (traefik), 8000/TCP (web), 8443/TCP (websecure)
# microk8s kubectl describe ds traefik -n ingress
microk8s enable ingress

# TargetPort（目标端口）—— 容器真正监听的“房门”
# Port（服务端口）—— 集群内部访问的“单元门”，这是 Service（服务） 对象本身暴露在 Kubernetes 集群内部网络（ClusterIP）上的端口。
# NodePort（节点端口）—— 外部访问的“小区大门”，这是 物理节点（Node/宿主机） 上开放的端口，用于将服务暴露给集群外部的用户访问。
# sudo iptables-legacy -t nat -L -n -v | grep -E "dports 80|dports 443" -A 2 -B 2
# sudo iptables-legacy -t nat -L CNI-DN-b019f25aa8bce06bae720 -n -v
# 所有发往宿主机 80 端口的流量，已被自动转发到 Traefik Pod 的 8000 端口
# 所有发往宿主机 443 端口的流量，已被自动转发到 Traefik Pod 的 8443 端口
# 外部用户 --> NAT || (Node IP : NodePort) --> Service ClusterIP : Port --> Pod IP : TargetPort --> 容器应用

# Install PostgreSQL
# https://www.postgresql.org/download/linux/ubuntu/
sudo apt install curl ca-certificates
sudo install -d /usr/share/postgresql-common/pgdg
sudo curl -o /usr/share/postgresql-common/pgdg/apt.postgresql.org.asc --fail https://www.postgresql.org/media/keys/ACCC4CF8.asc 

sudo apt install postgresql-18

psql --version

# Edit PostgreSQL configuration
vi /etc/postgresql/18/main/postgresql.conf
listen_addresses = 'localhost, 10.1.1.1'

vi /etc/postgresql/18/main/pg_hba.conf


# Restart PostgreSQL to apply changes
sudo systemctl restart postgresql

# Allow any IP in the 10.1.0.0/16 subnet and any user to access all databases
# 允许 10.1.0.0/16 网段下的任意 IP、任意用户访问所有数据库
host    all             all             10.1.0.0/16             scram-sha-256

# Change user postgres password
sudo -u postgres psql
ALTER USER postgres WITH PASSWORD '***';
\q

# Create two users & databases, smarterp, smarterp_log
# Backup from existing PostgreSQL databases with pgAdmin:
# 1. General, Format: "Plain", Encoding: "UTF8"
# 2. Data Options, Unselect "Blobs", "Only schemas"
# 3. Options, Unselect "Verbose messages"
# Tools -> Storage Manager -> Download