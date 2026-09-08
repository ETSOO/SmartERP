{{/*
将 ENV 格式（a=b）转换为 YAML 键值对（a: 'b'）
*/}}
{{- define "chart.envToYaml" -}}
  {{- $input := . | replace "\r" "" -}}
  {{- range $line := ($input | splitList "\n") -}}
    {{- $line := $line | trim -}}
    {{- if and $line (not (hasPrefix "#" $line)) -}}
      {{- $kv := splitn "=" 2 $line -}}
      {{- if eq (len $kv) 2 }}
{{ $kv._0 | trim }}: '{{ $kv._1 | trim | replace "'" "''" }}'
      {{- end -}}
    {{- end -}}
  {{- end -}}
{{- end -}}