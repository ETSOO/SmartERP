{{/*
将 ENV 格式的字符串（a=b）转换为 YAML 格式（a: 'b'）
同时自动过滤空行和 # 开头的注释，并将 key 后面的值默认加单引号
Usage:
  {{ include "chart.envToYaml" .Values.secrets.shared | indent 2 }}
*/}}
{{- define "chart.envToYaml" -}}
  {{- $input := . -}}
  {{- range $line := ($input | splitList "\n") -}}
    {{- $line := $line | trim -}}
    {{- if and $line (not (hasPrefix "#" $line)) -}}
      {{- $kv := splitn "=" 2 $line -}}
      {{- if eq (len $kv) 2 -}}
{{ $kv._0 }}: '{{ $kv._1 | replace "'" "''" }}'
      {{ end -}}
    {{- end -}}
  {{- end -}}
{{- end -}}