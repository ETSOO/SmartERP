# Debug
# 在 helm 目录下调试模板
microk8s helm3 template . --debug

# Remove previous release done by kustomization
# Example, when current location is /infra/overlays/cn/
microk8s kubectl delete -k admin

# Remove previous release directory, especially during debugging when other users may have created files
# 移除之前的发布目录，特别是调试阶段，有其他用户创建了文件
rm -rf /home/***/deploy/releases/1.0.0