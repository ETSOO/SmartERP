# Create necessary directories for base and overlays/cn
mkdir -p infra/base/shared infra/base/platform infra/overlays/cn/platform secrets

touch secrets/shared.env.secrets \
      secrets/platform.env.secrets

touch infra/base/shared/kustomization.yaml \
      infra/base/platform/kustomization.yaml \
      infra/base/platform/kustomization.yaml \
      infra/base/platform/deployment.yaml \
      infra/base/platform/service.yaml \
      infra/base/platform/route.yaml

touch infra/overlays/cn/platform/kustomization.yaml

# Move the secrets file to the appropriate location for the CN platform overlay
cp -f secrets/shared.env.secrets infra/base/shared/
cp -f secrets/platform.env.secrets infra/overlays/cn/platform/

# View the rendered Kustomize configuration for the CN platform overlay
microk8s kubectl kustomize infra/overlays/cn/platform

# Apply the CN platform overlay configuration to the cluster
microk8s kubectl apply -k infra/overlays/cn/platform

# View the logs for pod failure
microk8s kubectl logs -n smarterp smarterp-platform-***

# =================================================================
# Add other appplications as needed
# coreapp as an example
# =================================================================
mkdir -p infra/base/coreapp infra/overlays/cn/coreapp

# Download the files to the base/coreapp
curl -k -sSL -o infra/base/coreapp/kustomization.yaml https://ghfast.top/https://raw.githubusercontent.com/etsoo/SmartERP/main/infra/base/coreapp/kustomization.yaml
curl -k -sSL -o infra/base/coreapp/deployment.yaml https://ghfast.top/https://raw.githubusercontent.com/etsoo/SmartERP/main/infra/base/coreapp/deployment.yaml
curl -k -sSL -o infra/base/coreapp/service.yaml https://ghfast.top/https://raw.githubusercontent.com/etsoo/SmartERP/main/infra/base/coreapp/service.yaml
curl -k -sSL -o infra/base/coreapp/route.yaml https://ghfast.top/https://raw.githubusercontent.com/etsoo/SmartERP/main/infra/base/coreapp/route.yaml
curl -k -sSL -o infra/overlays/cn/coreapp/kustomization.yaml https://ghfast.top/https://raw.githubusercontent.com/etsoo/SmartERP/main/infra/overlays/cn/coreapp/kustomization.yaml

touch secrets/coreapp.env.secrets
vi secrets/coreapp.env.secrets

cp -f secrets/coreapp.env.secrets infra/overlays/cn/coreapp/

microk8s kubectl kustomize infra/overlays/cn/coreapp
microk8s kubectl apply -k infra/overlays/cn/coreapp

# =================================================================
# Initialize program data
# 初始化程序数据
# 这里的app_secret，从项目配置文件中的 AppSecret，“Token” + AppId + (中心平台Platform.Server的)PrivateKey 作为密匙加密而来。
# =================================================================
INSERT INTO public.core_app (id, name, identity_type, app_secret, is_public, enabled, urls)
VALUES (1,'司友云ERP管理中心', 1,
'***',
false, true, '[
  {
    "api": "https://coreapp.etsoo.cn/api/",
    "web": "https://coreapp.etsoo.cn/"
  }
]')

INSERT INTO public.core_app (id, name, identity_type, app_secret, is_public, enabled, urls)
VALUES (2,'司友云ERP运维中心', 1,
'***',
false, true, '[
  {
    "api": "https://admin.etsoo.cn/api/",
    "web": "https://admin.etsoo.cn/"
  }
]')

INSERT INTO public.core_app (id, name, identity_type, app_secret, is_public, enabled, urls)
VALUES (3,'司友云ERP业务管理', 1,
'***',
true, true, '[
  {
    "api": "https://crm.etsoo.cn/api/",
    "web": "https://crm.etsoo.cn/"
  }
]')
