# Create user smarterp and its shell(-s), home directory(-m)
sudo useradd -m -s /bin/bash smarterp

# Add the current user to the microk8s group for permissions, -aG "append to group"
# 将当前用户添加到 microk8s 组以获取权限
sudo usermod -aG microk8s smarterp

# Create SSH key pair for authentication, user(-u), -C "comment", -f "file", -N "passphrase"
# 创建 SSH 密钥对用于认证
sudo -u smarterp ssh-keygen -t ed25519 -C "smarterp-deploy-key" -f /home/smarterp/.ssh/id_ed25519 -N ""

# Set execute permissions for the deploy directory
# 为部署目录设置执行权限
sudo -u smarterp mkdir -p /home/smarterp/deploy
sudo chmod 755 /home/smarterp/deploy
sudo chown -R smarterp:smarterp /home/smarterp/deploy

# 打印 SSH 私钥内容并存入 GitHub Secrets 或其他安全存储
# CN_SSH_HOST, CN_SSH_PORT, CN_SSH_USER, CN_SSH_KEY
sudo cat /home/smarterp/.ssh/id_ed25519

# Add the public key to the authorized_keys file for passwordless SSH login
sudo ssh-keygen -y -f /home/smarterp/.ssh/id_ed25519 \
  | sudo tee -a /home/smarterp/.ssh/authorized_keys > /dev/null

# to verify the added public key
sudo cat /home/smarterp/.ssh/authorized_keys