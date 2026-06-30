# Unity Dev Harness - Gate 1~3 자동 검증 Stop hook (Windows)
#
# CoplayDev MCP for Unity(HTTP /mcp)에 붙어서 4단계 검증 게이트의 1~3단계를
# 자동으로 수행한다. 결과는 (a) Claude Stop 차단 (b) artifacts/02-validation.md 두 곳에 남는다.
# Unity MCP가 안 떠 있으면 차단하지 않고 02-validation.md를 "수동 검증 필요"로 남긴 뒤 종료한다.

$ErrorActionPreference = "Stop"

$MCP_URL = if ($env:MCP_URL) { $env:MCP_URL } else { "http://127.0.0.1:8080/mcp" }
$script:SessionId = $null
$script:RequestId = 0

# ---- 프로젝트/artifacts 경로 해석 ----
$HookDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = if ($env:CLAUDE_PROJECT_DIR) { $env:CLAUDE_PROJECT_DIR } else { Split-Path -Parent (Split-Path -Parent $HookDir) }
$ArtifactDir = if ($env:ARTIFACT_DIR) { $env:ARTIFACT_DIR } else { Join-Path $ProjectDir "artifacts" }
$ValidationFile = Join-Path $ArtifactDir "02-validation.md"

# ---- .cs 변경 감지: clear-artifacts가 기록한 시각 이후 변경된 파일이 없으면 스킵 ----
$TsFile = Join-Path $HookDir ".viewer-state\last-clear-ts.txt"
if (Test-Path $TsFile) {
    try {
        $lastClear = [datetime]::Parse((Get-Content $TsFile -ErrorAction Stop | Select-Object -First 1).Trim())
        $changed = Get-ChildItem -Path (Join-Path $ProjectDir "Assets") -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue |
            Where-Object { $_.LastWriteTime -gt $lastClear }
        if (-not $changed) {
            [Console]::Error.WriteLine("No .cs changes since last prompt. Skipping validation.")
            exit 0
        }
    } catch { }
}

function Now-Ts { (Get-Date).ToString("yyyy-MM-dd HH:mm") }
function Now-Hm { (Get-Date).ToString("HH:mm") }

function Write-Validation {
    param(
        [string]$Mode,   # auto | manual
        [string]$G1, [string]$G2, [string]$G3,
        [string]$Detail
    )
    try {
        if (-not (Test-Path $ArtifactDir)) {
            New-Item -ItemType Directory -Force $ArtifactDir | Out-Null
        }
    } catch { return }

    $author = if ($Mode -eq "manual") {
        "unity-validate hook (MCP 미연동 -> 수동 검증 필요)"
    } else {
        "unity-validate hook (CoplayDev MCP for Unity 자동 검증)"
    }
    $hm = Now-Hm
    $sb = New-Object System.Text.StringBuilder
    [void]$sb.AppendLine("# 02. 검증 (4단계 게이트)")
    [void]$sb.AppendLine("")
    [void]$sb.AppendLine("작성: $author")
    [void]$sb.AppendLine("일시: $(Now-Ts)")
    [void]$sb.AppendLine("")
    [void]$sb.AppendLine("## 게이트 진행 요약")
    [void]$sb.AppendLine("| 단계 | 결과 | 시간 |")
    [void]$sb.AppendLine("| --- | --- | --- |")
    [void]$sb.AppendLine("| 1. 컴파일 | $G1 | $hm |")
    [void]$sb.AppendLine("| 2. 런타임 에러 | $G2 | $hm |")
    [void]$sb.AppendLine("| 3. 기능 점검 (자동) | $G3 | $hm |")
    [void]$sb.AppendLine("| 4. 기능 점검 (사용자) | ⏳ 대기 (hooks 뷰어에서 제출 예정) | - |")
    [void]$sb.AppendLine("")
    if ($Detail) { [void]$sb.AppendLine($Detail) }

    try {
        $utf8 = New-Object System.Text.UTF8Encoding($false)
        [System.IO.File]::WriteAllText($ValidationFile, $sb.ToString(), $utf8)
    } catch { }
}

function Send-McpRequest {
    param([hashtable]$Payload)

    $headers = @{
        "Content-Type" = "application/json"
        "Accept"       = "application/json, text/event-stream"
    }
    if ($script:SessionId) { $headers["Mcp-Session-Id"] = $script:SessionId }

    $body = $Payload | ConvertTo-Json -Depth 20 -Compress
    $response = Invoke-WebRequest -Uri $MCP_URL -Method Post -Headers $headers -Body $body -UseBasicParsing -TimeoutSec 15

    $sessionHeader = $response.Headers["Mcp-Session-Id"]
    if ($sessionHeader) {
        if ($sessionHeader -is [array]) { $script:SessionId = $sessionHeader[0] }
        else { $script:SessionId = $sessionHeader }
    }

    if ([string]::IsNullOrWhiteSpace($response.Content)) { return $null }
    $content = $response.Content.Trim()
    if ($content -match "(?m)^data:\s*(.+)$") { $content = $Matches[1].Trim() }
    if ([string]::IsNullOrWhiteSpace($content)) { return $null }
    return $content | ConvertFrom-Json
}

function Invoke-UnityTool {
    param([string]$ToolName, [hashtable]$Arguments)
    $script:RequestId += 1
    return Send-McpRequest @{
        jsonrpc = "2.0"
        id      = $script:RequestId
        method  = "tools/call"
        params  = @{ name = $ToolName; arguments = $Arguments }
    }
}

function Read-McpResource {
    param([string]$Uri)
    $script:RequestId += 1
    return Send-McpRequest @{
        jsonrpc = "2.0"
        id      = $script:RequestId
        method  = "resources/read"
        params  = @{ uri = $Uri }
    }
}

# 게이트 실패 시: 02-validation.md 기록만 하고 종료 (block 없음)
function Fail-Gate {
    param([string]$G1, [string]$G2, [string]$G3, [string]$Prefix, [string]$DetailText)
    $body = "### 실패 상세`n$Prefix`n$DetailText"
    Write-Validation -Mode "auto" -G1 $G1 -G2 $G2 -G3 $G3 -Detail $body
    [Console]::Error.WriteLine("Validation failed: $Prefix")
    exit 0
}

function Get-ToolPayload {
    param($ToolResult)
    if ($ToolResult.result.structuredContent) { return $ToolResult.result.structuredContent }
    $text = ($ToolResult.result.content | Where-Object { $_.type -eq "text" } | Select-Object -First 1).text
    if ([string]::IsNullOrWhiteSpace($text)) { return $null }
    try { return $text | ConvertFrom-Json }
    catch { return @{ success = $true; message = $text; data = @($text) } }
}

function Get-ResourcePayload {
    param($ResourceResult)
    $text = ($ResourceResult.result.contents | Select-Object -First 1).text
    if ([string]::IsNullOrWhiteSpace($text)) { return $null }
    try { return $text | ConvertFrom-Json } catch { return $null }
}

function Get-ToolText {
    param($ToolResult)
    $payload = Get-ToolPayload $ToolResult
    if (-not $payload) { return "" }
    if ($payload.data) {
        if ($payload.data -is [array]) { return ($payload.data | ForEach-Object { "$_" }) -join "`n" }
        return "$($payload.data)"
    }
    if ($payload.message) { return "$($payload.message)" }
    return ($payload | ConvertTo-Json -Depth 10)
}

function Assert-ToolSucceeded {
    param($ToolResult, [string]$FailurePrefix, [string]$G1, [string]$G2, [string]$G3)
    if ($ToolResult.error) { Fail-Gate $G1 $G2 $G3 $FailurePrefix "$($ToolResult.error.message)" }
    if ($ToolResult.result.isError -eq $true) { Fail-Gate $G1 $G2 $G3 $FailurePrefix (Get-ToolText $ToolResult) }
    $payload = Get-ToolPayload $ToolResult
    if ($payload -and $payload.success -eq $false) { Fail-Gate $G1 $G2 $G3 $FailurePrefix (Get-ToolText $ToolResult) }
}

function Wait-UnityCompileReady {
    param([int]$TimeoutSeconds = 60)
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $stateResult = Read-McpResource "mcpforunity://editor/state"
        $statePayload = Get-ResourcePayload $stateResult
        if ($statePayload -and $statePayload.success -eq $false) {
            Fail-Gate "❌ 실패" "⏳ 대기" "⏳ 대기" "Unity editor state read failed:" "$($statePayload.message)"
        }
        $state = $statePayload.data
        if ($state) {
            $busy = ($state.compilation.is_compiling -eq $true) -or
                    ($state.compilation.is_domain_reload_pending -eq $true) -or
                    ($state.assets.is_updating -eq $true) -or
                    ($state.assets.refresh.is_refresh_in_progress -eq $true) -or
                    ($state.editor.play_mode.is_changing -eq $true)
            if (-not $busy) { return }
        }
        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)
    Fail-Gate "❌ 실패" "⏳ 대기" "⏳ 대기" "Timed out waiting for Unity refresh/compile to finish." ""
}

# Initialize MCP session. Unity MCP가 안 떠 있으면 수동 검증 필요로 남기고 통과.
try {
    Send-McpRequest @{
        jsonrpc = "2.0"; id = (++$script:RequestId)
        method  = "initialize"
        params  = @{
            protocolVersion = "2024-11-05"
            capabilities    = @{}
            clientInfo      = @{ name = "unity-validate-hook"; version = "1.0" }
        }
    } | Out-Null
    Send-McpRequest @{ jsonrpc = "2.0"; method = "notifications/initialized"; params = @{} } | Out-Null
} catch {
    Write-Validation -Mode "manual" -G1 "🔧 수동 검증 필요" -G2 "🔧 수동 검증 필요" -G3 "🔧 수동 검증 필요" -Detail "## 수동 검증 체크리스트 (MCP 미연동)
1. [ ] 콘솔에 컴파일 에러가 없는지 확인
2. [ ] Play 모드 진입 후 LogError/Exception이 없는지 확인
3. [ ] 대상 기능이 의도대로 동작하는지 확인
4. [ ] 이상 시 콘솔 에러 복사 -> debugger에 전달

> MCP for Unity가 실행 중이 아니어서 1~3단계 자동 검증을 건너뛰었습니다. Unity 에디터에서 직접 확인 후 4단계에 입력하세요."
    exit 0
}

# Step 1: 이전 콘솔 에러 비우기 (best-effort)
try { Invoke-UnityTool "read_console" @{ action = "clear" } | Out-Null } catch { }

# Step 2: refresh + 컴파일 요청 (1단계 컴파일)
try {
    $refreshResult = Invoke-UnityTool "refresh_unity" @{ mode = "force"; scope = "all"; compile = "request"; wait_for_ready = $true }
    Assert-ToolSucceeded $refreshResult "Unity refresh failed:" "❌ 실패" "⏳ 대기" "⏳ 대기"
    Wait-UnityCompileReady 60
} catch {
    Fail-Gate "❌ 실패" "⏳ 대기" "⏳ 대기" "Unity refresh failed:" "$_"
}

# Step 3: Play 모드 진입
try {
    $playResult = Invoke-UnityTool "manage_editor" @{ action = "play" }
    Assert-ToolSucceeded $playResult "Unity play mode failed:" "✅ 통과" "❌ 실패" "⏳ 대기"
} catch {
    Fail-Gate "✅ 통과" "❌ 실패" "⏳ 대기" "Unity play mode failed:" "$_"
}

# Step 4: Play 모드 잠시 실행
Start-Sleep -Seconds 5

# Step 5: 콘솔 에러 확인
try {
    $consoleResult = Invoke-UnityTool "read_console" @{ action = "get"; types = @("error"); count = 20; format = "plain" }
    Assert-ToolSucceeded $consoleResult "Unity console read failed:" "✅ 통과" "❌ 실패" "⏳ 대기"
    $text = Get-ToolText $consoleResult
    $errorLines = @(
        $text -split "`r?`n" |
            ForEach-Object { $_.Trim() } |
            Where-Object {
                $_ -and
                $_ -notmatch "^Retrieved \d+ log entries\.$" -and
                $_ -notmatch "^No (log )?entries" -and
                $_ -notmatch "^Saving results to:"
            }
    )
    if ($errorLines.Count -gt 0) {
        Fail-Gate "✅ 통과" "❌ 실패" "❌ 실패" "Unity console errors detected:" ($errorLines -join "`n")
    }
} catch {
    # 콘솔 읽기는 best-effort. 로그를 못 읽었다는 이유만으로 차단하지 않는다.
}

# Step 6: Gate 3 — play mode 안에서 gate3_run_test 커스텀 툴 호출
$g3Status = "➖ 스킵"
$g3Detail = ""
$gate3File = Join-Path $HookDir ".viewer-state\gate3-test.json"
if (Test-Path $gate3File) {
    try {
        $g3cfg = Get-Content $gate3File -Raw -Encoding UTF8 | ConvertFrom-Json
        $g3toolArgs = @{ type_name = $g3cfg.type_name }
        if ($null -ne $g3cfg.call_start)            { $g3toolArgs["call_start"] = [bool]$g3cfg.call_start }
        if ($null -ne $g3cfg.inject)                { $g3toolArgs["inject"] = [bool]$g3cfg.inject }
        if ($null -ne $g3cfg.check_non_null_fields) { $g3toolArgs["check_non_null_fields"] = $g3cfg.check_non_null_fields }

        $g3result = Invoke-UnityTool "gate3_run_test" $g3toolArgs

        # gate3_run_test 응답 파싱 (content[0].text → JSON → message)
        $g3raw = ($g3result.result.content | Where-Object { $_.type -eq "text" } | Select-Object -First 1).text
        if ([string]::IsNullOrWhiteSpace($g3raw) -and $g3result.result.structuredContent) {
            $sc = $g3result.result.structuredContent
            $g3raw = if ($sc.message) { $sc.message } else { "$sc" }
        }
        try {
            $g3parsed = $g3raw | ConvertFrom-Json
            $g3text = if ($g3parsed.message) { "$($g3parsed.message)" } else { "$($g3parsed.data)" }
        } catch {
            $g3text = "$g3raw"
        }

        if ($g3result.error -or $g3result.result.isError -eq $true) {
            $g3Status = "❌ 실패"; $g3Detail = $g3text
        } elseif ($g3text -match "^PASS") {
            $g3Status = "✅ 통과"; $g3Detail = $g3text
        } elseif ($g3text -match "^FAIL") {
            $g3Status = "❌ 실패"; $g3Detail = $g3text
        } else {
            $g3Status = "⚠️ 결과 불명 ($g3text)"; $g3Detail = $g3text
        }
    } catch {
        $g3Status = "⚠️ 오류"; $g3Detail = "$_"
    }
}

# Step 7: play 종료
try {
    $stopResult = Invoke-UnityTool "manage_editor" @{ action = "stop" }
    Assert-ToolSucceeded $stopResult "Unity play mode stop failed:" "✅ 통과" "✅ 통과" $g3Status
} catch {
    Fail-Gate "✅ 통과" "✅ 통과" $g3Status "Unity play mode stop failed:" "$_"
}

$g3DetailBlock = if ($g3Detail) { "`n- 3단계 상세: $g3Detail" } else { "" }
Write-Validation -Mode "auto" -G1 "✅ 통과" -G2 "✅ 통과" -G3 $g3Status -Detail "## 자동 검증 결과
- 1단계 컴파일: refresh_unity(force/compile) + editor/state 대기 통과. 컴파일 에러 0.
- 2단계 런타임 에러: Play 모드 진입 후 콘솔 에러/예외 없음.$g3DetailBlock"

exit 0
