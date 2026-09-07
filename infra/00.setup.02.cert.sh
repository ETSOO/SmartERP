# 泛域名证书 DNS-01 Challenge，保存好生成的 JSON 数据
curl -X POST https://auth.acme-dns.io/register

# 域名解析
# 主机记录： _acme-challenge
# 记录类型： CNAME
# 记录值： fulldomain <从上一步生成的 JSON 数据中获取>

# 在 Kubernetes 中创建 acme-dns 凭据 Secret
apiVersion: v1
kind: Secret
metadata:
  name: acme-dns-secret
  namespace: ingress
type: Opaque
stringData:
  # 这里的 JSON 结构必须严格符合 cert-manager 的 acme-dns 格式
  acme-dns-accounts.json: |
    {
      "etsoo.cn": {
        "username": "你的 username",
        "password": "你的 password",
        "fulldomain": "你的 fulldomain",
        "subdomain": "你的 subdomain",
        "server": "https://auth.acme-dns.io"
      },
      "*.etsoo.cn": {
        "username": "你的 username",
        "password": "你的 password",
        "fulldomain": "你的 fulldomain",
        "subdomain": "你的 subdomain",
        "server": "https://auth.acme-dns.io"
      }
    }

# 创建基于 acme-dns 的 ClusterIssuer
# gatewayHTTPRoute 暂时是实验性功能，如果不支持请移除或者改成ingress方式
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-gateway
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: info@etsoo.com
    privateKeySecretRef:
      name: letsencrypt-gateway-key
    solvers:    
    - selector:
        dnsZones:
          - "etsoo.cn"
      dns01:
        acmeDNS:
          host: https://auth.acme-dns.io
          accountSecretRef:
            name: acme-dns-secret
            key: acme-dns-accounts.json
    - http01:
        gatewayHTTPRoute:
          parentRefs:
          - name: traefik-gateway
            namespace: ingress
            kind: Gateway

# 创建 Certificate 资源申请泛域名
apiVersion: cert-manager.io/v1
kind: Certificate
metadata:
  name: wildcard-etsoo-cn-cert
  namespace: ingress # 必须与 Gateway 资源处于同一个 Namespace
spec:
  secretName: wildcard-etsoo-cn-tls # 申请成功后生成的 TLS Secret 名称
  issuerRef:
    name: letsencrypt-gateway
    kind: ClusterIssuer
  dnsNames:
    - "etsoo.cn"
    - "*.etsoo.cn"

# 修改 Gateway 资源，添加 HTTPS 监听器并引用申请的泛域名证书
# microk8s kubectl edit gateway traefik-gateway -n ingress
spec:
  gatewayClassName: traefik
  listeners:
  - allowedRoutes:
      namespaces:
        from: All
    name: web
    port: 8000
    protocol: HTTP
  - allowedRoutes:
      namespaces:
        from: All
    name: websecure
    port: 8443
    protocol: HTTPS
    tls:
      certificateRefs:
      - group: ""
        kind: Secret
        name: wildcard-etsoo-cn-tls
      mode: Terminate                                                                                           2         37,9          24%