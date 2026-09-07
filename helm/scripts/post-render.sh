#!/bin/bash
set -e

# Get the path of the current script directory (scripts)
# 获取当前脚本所在目录的绝对路径 (scripts)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Receive Helm rendered output and save it
# 接收 Helm 渲染传过来的全部 YAML 内容
cat <&0 > "$SCRIPT_DIR/helm-output.yaml"

# Use microk8s kustomize to build the final Kubernetes resource manifest
# 使用 microk8s 的 kustomize 构建最终的 Kubernetes 资源清单
microk8s kustomize build "$SCRIPT_DIR"