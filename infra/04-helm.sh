# Debug
# 在 helm 目录下调试模板
microk8s helm3 template . \
  -n smarterp \
  -f "./config/values-cn.yaml" \
  --set-file secrets.shared=/home/smarterp/secrets/shared.env.secrets \
  --set-file secrets.admin=/home/smarterp/secrets/admin.env.secrets \
  --set-file secrets.coreapp=/home/smarterp/secrets/coreapp.env.secrets \
  --set-file secrets.crm=/home/smarterp/secrets/crm.env.secrets \
  --set-file secrets.platform=/home/smarterp/secrets/platform.env.secrets \
  --set-file secrets.workercenter=/home/smarterp/secrets/workercenter.env.secrets \
  --set-file secrets.workercms=/home/smarterp/secrets/workercms.env.secrets \
  --debug

# Remove previous release done by kustomization
# Example, when current location is /infra/overlays/cn/
microk8s kubectl delete -k admin

# Remove previous release directory, especially during debugging when other users may have created files
# 移除之前的发布目录，特别是调试阶段，有其他用户创建了文件
rm -rf /home/***/deploy/releases/1.0.0

# Remove previous release done by Helm
microk8s helm3 uninstall smarterp -n smarterp

# Query last 5 history
microk8s helm3 history smarterp -n smarterp --max=5