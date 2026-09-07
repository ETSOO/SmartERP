
# Add the current user to the microk8s group for permissions
# 将当前用户添加到 microk8s 组以获取权限
sudo usermod -aG microk8s $USER

# Create SSH key pair for authentication
# 创建 SSH 密钥对用于认证
ssh-keygen -t ed25519 -C "github-actions-deploy" -f ~/.ssh/id_ed25519_deploy -N ""

# Set execute permissions for the current directory
# 为当前目录设置执行权限
chmod +x ./