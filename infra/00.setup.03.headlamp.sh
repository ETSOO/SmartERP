# 安装 Headlamp
microk8s helm repo add headlamp https://kubernetes-sigs.github.io/headlamp/
microk8s helm repo update
microk8s helm upgrade --install headlamp headlamp/headlamp \
  --namespace headlamp \
  --create-namespace

# Route
# Get parentRefs (traefik-gateway) info:
# microk8s kubectl get gateway -A
# Get backendRefs (headlamp) info:
# microk8s kubectl get svc -A
apiVersion: gateway.networking.k8s.io/v1
kind: HTTPRoute
metadata:
  name: headlamp
  namespace: headlamp

spec:
  parentRefs:
    - name: traefik-gateway
      namespace: ingress
      kind: Gateway

  hostnames:
    - headlamp.etsoo.cn

  rules:
    - matches:
        - path:
            type: PathPrefix
            value: /

      backendRefs:
        - name: headlamp
          namespace: headlamp
          port: 80

# 创建 ServiceAccount
microk8s kubectl create serviceaccount headlamp-admin -n headlamp

# 创建登录 Token
microk8s kubectl create token headlamp-admin -n headlamp