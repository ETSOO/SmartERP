# Create user smarterp and its shell(-s), home directory(-m)
sudo useradd -m -s /bin/bash smarterp

# Add the current user to the microk8s group for permissions, -aG "append to group"
# 将当前用户添加到 microk8s 组以获取权限
sudo usermod -aG microk8s smarterp

# Create the home and secrets directories for the smarterp user
sudo mkdir -p /home/smarterp/secrets /home/smarterp/deploy

# Copy all secrets to the smarterp user's secrets directory
sudo cp -r secrets/* /home/smarterp/secrets/

# 将 deploy 目录及其子文件的属主改为 smarterp:smarterp
sudo chown -R smarterp:smarterp /home/smarterp/deploy

# 确保 smarterp 拥有可读可写可执行权限 (755)
sudo chmod -R 755 /home/smarterp/deploy

# Create SSH key pair for authentication, user(-u), -C "comment", -f "file", -N "passphrase"
# 创建 SSH 密钥对用于认证
sudo -u smarterp ssh-keygen -t ed25519 -C "smarterp-deploy-key" -f /home/smarterp/.ssh/id_ed25519 -N ""

# 打印 SSH 私钥内容并存入 GitHub Secrets 或其他安全存储
# CN_SSH_HOST, CN_SSH_PORT, CN_SSH_USER, CN_SSH_KEY
sudo cat /home/smarterp/.ssh/id_ed25519

# Add the public key to the authorized_keys file for passwordless SSH login
sudo ssh-keygen -y -f /home/smarterp/.ssh/id_ed25519 \
  | sudo tee -a /home/smarterp/.ssh/authorized_keys > /dev/null

# to verify the added public key
sudo cat /home/smarterp/.ssh/authorized_keys

# 阿里云创建服务器密匙对，核心作用就是为你的Linux云服务器（ECS）提供一种比常规密码更安全、更便捷的登录认证方式
# 在文件中找到 #Port 22 这一行，去掉开头的 # 注释，并将数字改为你想要的新端口，比如 31382，并修改防火墙规则
# 在测试新端口成功前，建议保留 Port 22 这一行，即同时监听2个端口，然后重启服务器
sudo vim /etc/ssh/sshd_config

# 仅限证书登录
# PubkeyAuthentication yes
# PasswordAuthentication no

sudo reboot

# Remove the smarterp user and its home directory
sudo pkill -u smarterp
sudo userdel -r smarterp