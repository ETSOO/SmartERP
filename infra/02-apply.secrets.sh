# Create necessary directories for base and overlays/cn
mkdir -p infra/base/platform infra/overlays/cn/platform secrets

touch secrets/platform.env.secrets

touch infra/base/platform/kustomization.yaml \
      infra/base/platform/deployment.yaml \
      infra/base/platform/service.yaml \
      infra/base/platform/route.yaml

touch infra/overlays/cn/platform/kustomization.yaml

# Move the secrets file to the appropriate location for the CN platform overlay
cp -f secrets/platform.env.secrets infra/overlays/cn/platform/

# View the rendered Kustomize configuration for the CN platform overlay
microk8s kubectl kustomize infra/overlays/cn/platform

# Apply the CN platform overlay configuration to the cluster
microk8s kubectl apply -k infra/overlays/cn/platform

# View the logs for pod failure
microk8s kubectl logs -n smarterp smarterp-platform-7656999848-p42xz