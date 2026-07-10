# .claude/hooks/agent-router.ps1
$ErrorActionPreference = "Stop"

# 한글 reason이 깨지지 않도록 stdout을 UTF-8로 고정
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# stdin(JSON)을 읽는다
# 주의: $input 은 PowerShell 예약 자동 변수(파이프라인 enumerator)라 여기 쓰면 값이 소실된다.
$raw = [Console]::In.ReadToEnd()
$data = $raw | ConvertFrom-Json

$subagent = $data.tool_input.subagent_type
if ([string]::IsNullOrEmpty($subagent)) {
    exit 0   # Task 호출이 아니거나 subagent_type이 없음 -> 통과
}

$configPath = Join-Path $env:CLAUDE_PROJECT_DIR ".claude\harness.config.json"
if (-not (Test-Path $configPath)) {
    exit 0   # config 없으면 전부 internal 취급
}

$config = Get-Content $configPath -Raw -Encoding UTF8 | ConvertFrom-Json

$agentConfig = $config.agents.$subagent
$backend = if ($null -ne $agentConfig -and $agentConfig.backend) { $agentConfig.backend } else { "internal" }

if ($backend -eq "internal") {
    exit 0
}

$reason = if ($agentConfig.reason) {
    $agentConfig.reason
} else {
    "이 에이전트는 외부 백엔드로 전환되었습니다."
}

$result = @{
    hookSpecificOutput = @{
        hookEventName            = "PreToolUse"
        permissionDecision       = "deny"
        permissionDecisionReason = $reason
    }
} | ConvertTo-Json -Compress

Write-Output $result
exit 0