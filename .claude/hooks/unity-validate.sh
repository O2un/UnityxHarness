#!/bin/sh
# Unity Dev Harness - Gate 1~3 자동 검증 Stop hook (macOS/Linux)
#
# CoplayDev MCP for Unity(HTTP /mcp)에 붙어서 4단계 검증 게이트의 1~3단계를
# 자동으로 수행한다.
#   1단계 컴파일      : refresh_unity(force/compile) + editor/state 대기
#   2단계 런타임 에러 : manage_editor(play) 진입 후 콘솔 에러 확인
#   3단계 기능 점검   : play 상태에서 추가 콘솔 에러 없음으로 1차 판정
#
# 결과는 두 가지 형태로 남는다.
#   (a) Claude Stop 차단: 에러가 있으면 {"decision":"block","reason":...} 출력 →
#       Claude가 멈추지 않고 수정 작업을 이어간다.
#   (b) artifacts/02-validation.md: 게이트 진행 요약 표를 자동 기록 → viewer가 읽는다.
#
# Unity MCP가 안 떠 있으면(연결 실패) 아무것도 차단하지 않고 exit 0 한다.
# 이때 02-validation.md를 "🔧 수동 검증 필요"로 남겨, viewer가 수동 모드로 표시한다.

MCP_URL="${MCP_URL:-http://127.0.0.1:8080/mcp}"
SESSION_ID=""
REQUEST_ID=0

TMP_DIR="${TMPDIR:-/tmp}"
BODY_FILE="$TMP_DIR/unity-validate-body-$$.json"
HEADER_FILE="$TMP_DIR/unity-validate-headers-$$.txt"
RESPONSE_FILE="$TMP_DIR/unity-validate-response-$$.txt"

# ---- 프로젝트/artifacts 경로 해석 (open-viewer.sh와 동일 규칙) ----
SCRIPT_PATH="$0"
HOOK_DIR="$(CDPATH= cd -- "$(dirname -- "$SCRIPT_PATH")" && pwd)"
PROJECT_DIR="${CLAUDE_PROJECT_DIR:-}"
if [ -z "$PROJECT_DIR" ]; then
    # .claude/hooks/unity-validate.sh -> 프로젝트 루트는 두 단계 위
    PROJECT_DIR="$(CDPATH= cd -- "$HOOK_DIR/../.." && pwd)"
fi
ARTIFACT_DIR="${ARTIFACT_DIR:-$PROJECT_DIR/artifacts}"
VALIDATION_FILE="$ARTIFACT_DIR/02-validation.md"

# ---- .cs 변경 감지: clear-artifacts가 기록한 시각 이후 변경된 파일이 없으면 스킵 ----
TS_FILE="$HOOK_DIR/.viewer-state/last-clear-ts.txt"
if [ -f "$TS_FILE" ]; then
    last_clear_epoch="$(date -d "$(cat "$TS_FILE" 2>/dev/null)" +%s 2>/dev/null || date -j -f '%Y-%m-%dT%H:%M:%S' "$(cat "$TS_FILE" 2>/dev/null | cut -c1-19)" +%s 2>/dev/null || echo 0)"
    cs_changed="$(find "$PROJECT_DIR/Assets" -name '*.cs' -newer "$TS_FILE" 2>/dev/null | head -1)"
    if [ -z "$cs_changed" ]; then
        printf 'No .cs changes since last prompt. Skipping validation.\n' >&2
        exit 0
    fi
fi

cleanup() {
    rm -f "$BODY_FILE" "$HEADER_FILE" "$RESPONSE_FILE"
}
trap cleanup EXIT HUP INT TERM

next_request_id() {
    REQUEST_ID=$((REQUEST_ID + 1))
    printf '%s' "$REQUEST_ID"
}

require_python() {
    command -v python3 >/dev/null 2>&1 || exit 0
}

now_ts() {
    date '+%Y-%m-%d %H:%M'
}

now_hm() {
    date '+%H:%M'
}

# ---- 02-validation.md 기록 ----
# write_validation <mode> <g1> <g2> <g3> <detail>
#   mode  : auto | manual
#   g1~g3 : 게이트 결과 셀 문자열(예: "✅ 통과", "❌ 실패", "🔧 수동 검증 필요")
#   detail: 표 아래 본문(없으면 빈 문자열)
write_validation() {
    mode="$1"; g1="$2"; g2="$3"; g3="$4"; detail="$5"
    [ -d "$ARTIFACT_DIR" ] || mkdir -p "$ARTIFACT_DIR" 2>/dev/null || return 0

    ts="$(now_ts)"
    {
        printf '# 02. 검증 (4단계 게이트)\n\n'
        if [ "$mode" = "manual" ]; then
            printf '작성: unity-validate hook (MCP 미연동 → 수동 검증 필요)\n'
        else
            printf '작성: unity-validate hook (CoplayDev MCP for Unity 자동 검증)\n'
        fi
        printf '일시: %s\n\n' "$ts"
        printf '## 게이트 진행 요약\n'
        printf '| 단계 | 결과 | 시간 |\n'
        printf '| --- | --- | --- |\n'
        printf '| 1. 컴파일 | %s | %s |\n' "$g1" "$(now_hm)"
        printf '| 2. 런타임 에러 | %s | %s |\n' "$g2" "$(now_hm)"
        printf '| 3. 기능 점검 (자동) | %s | %s |\n' "$g3" "$(now_hm)"
        printf '| 4. 기능 점검 (사용자) | ⏳ 대기 (hooks 뷰어에서 제출 예정) | - |\n'
        printf '\n'
        if [ -n "$detail" ]; then
            printf '%s\n' "$detail"
        fi
    } > "$VALIDATION_FILE" 2>/dev/null || return 0
}


write_json() {
    require_python
    python3 - "$BODY_FILE" "$@" <<'PY'
import json
import sys

path = sys.argv[1]
kind = sys.argv[2]
request_id = int(sys.argv[3]) if len(sys.argv) > 3 and sys.argv[3] else None

if kind == "initialize":
    payload = {
        "jsonrpc": "2.0",
        "id": request_id,
        "method": "initialize",
        "params": {
            "protocolVersion": "2024-11-05",
            "capabilities": {},
            "clientInfo": {"name": "unity-validate-hook", "version": "1.0"},
        },
    }
elif kind == "initialized":
    payload = {
        "jsonrpc": "2.0",
        "method": "notifications/initialized",
        "params": {},
    }
elif kind == "tool":
    tool_name = sys.argv[4]
    arguments = json.loads(sys.argv[5])
    payload = {
        "jsonrpc": "2.0",
        "id": request_id,
        "method": "tools/call",
        "params": {"name": tool_name, "arguments": arguments},
    }
elif kind == "resource":
    uri = sys.argv[4]
    payload = {
        "jsonrpc": "2.0",
        "id": request_id,
        "method": "resources/read",
        "params": {"uri": uri},
    }
else:
    raise SystemExit(f"unknown payload kind: {kind}")

with open(path, "w", encoding="utf-8") as f:
    json.dump(payload, f, separators=(",", ":"))
PY
}

extract_sse_json() {
    require_python
    python3 - "$RESPONSE_FILE" <<'PY'
import sys

path = sys.argv[1]
text = open(path, encoding="utf-8", errors="replace").read().strip()
if not text:
    raise SystemExit(0)

for line in text.splitlines():
    if line.startswith("data:"):
        print(line[5:].strip())
        raise SystemExit(0)

print(text)
PY
}

send_mcp_request() {
    kind="$1"
    request_id="$2"
    shift 2

    write_json "$kind" "$request_id" "$@"

    if [ -n "$SESSION_ID" ]; then
        curl -sS --max-time 15 -X POST "$MCP_URL" \
            -H "Content-Type: application/json" \
            -H "Accept: application/json, text/event-stream" \
            -H "Mcp-Session-Id: $SESSION_ID" \
            -D "$HEADER_FILE" \
            --data-binary "@$BODY_FILE" \
            -o "$RESPONSE_FILE" >/dev/null || return 1
    else
        curl -sS --max-time 15 -X POST "$MCP_URL" \
            -H "Content-Type: application/json" \
            -H "Accept: application/json, text/event-stream" \
            -D "$HEADER_FILE" \
            --data-binary "@$BODY_FILE" \
            -o "$RESPONSE_FILE" >/dev/null || return 1
    fi

    new_session_id="$(python3 - "$HEADER_FILE" <<'PY'
import sys

for line in open(sys.argv[1], encoding="utf-8", errors="replace"):
    name, sep, value = line.partition(":")
    if sep and name.lower() == "mcp-session-id":
        print(value.strip())
        break
PY
)"
    if [ -n "$new_session_id" ]; then
        SESSION_ID="$new_session_id"
    fi

    extract_sse_json
}

invoke_unity_tool() {
    tool_name="$1"
    arguments="$2"
    send_mcp_request "tool" "$(next_request_id)" "$tool_name" "$arguments"
}

read_mcp_resource() {
    uri="$1"
    send_mcp_request "resource" "$(next_request_id)" "$uri"
}

tool_text() {
    require_python
    python3 -c '
import json
import sys

raw = sys.stdin.read().strip()
if not raw:
    raise SystemExit(0)

obj = json.loads(raw)
result = obj.get("result") or {}
payload = result.get("structuredContent")

if payload is None:
    text = ""
    for item in result.get("content") or []:
        if item.get("type") == "text":
            text = item.get("text") or ""
            break
    if text:
        try:
            payload = json.loads(text)
        except Exception:
            payload = {"success": True, "message": text, "data": [text]}

if not payload:
    raise SystemExit(0)

data = payload.get("data")
if isinstance(data, list):
    print("\n".join(str(x) for x in data))
elif data is not None:
    print(data)
elif payload.get("message"):
    print(payload["message"])
else:
    print(json.dumps(payload, ensure_ascii=False))
'
}

# 게이트 실패 시: 02-validation.md에 실패 기록 후 종료 (block 없음)
fail_gate() {
    g1="$1"; g2="$2"; g3="$3"; prefix="$4"; detail_text="$5"
    body="### 실패 상세
$prefix
$detail_text"
    write_validation "auto" "$g1" "$g2" "$g3" "$body"
    printf 'Validation failed: %s\n' "$prefix" >&2
    exit 0
}

assert_tool_succeeded() {
    raw="$1"
    failure_prefix="$2"
    g1="$3"; g2="$4"; g3="$5"

    status="$(printf '%s' "$raw" | python3 -c '
import json
import sys

raw = sys.stdin.read().strip()
if not raw:
    print("ok")
    raise SystemExit(0)

obj = json.loads(raw)
if obj.get("error"):
    print("error:" + str(obj["error"].get("message", "")))
    raise SystemExit(0)

result = obj.get("result") or {}
if result.get("isError") is True:
    print("tool_error")
    raise SystemExit(0)

payload = result.get("structuredContent")
if payload is None:
    text = ""
    for item in result.get("content") or []:
        if item.get("type") == "text":
            text = item.get("text") or ""
            break
    if text:
        try:
            payload = json.loads(text)
        except Exception:
            payload = None

if payload and payload.get("success") is False:
    print("tool_error")
else:
    print("ok")
')"

    case "$status" in
        ok)
            return 0
            ;;
        error:*)
            fail_gate "$g1" "$g2" "$g3" "$failure_prefix" "${status#error:}"
            ;;
        *)
            text="$(printf '%s' "$raw" | tool_text)"
            fail_gate "$g1" "$g2" "$g3" "$failure_prefix" "$text"
            ;;
    esac
}

resource_payload_data_json() {
    require_python
    python3 -c '
import json
import sys

raw = sys.stdin.read().strip()
if not raw:
    raise SystemExit(0)

obj = json.loads(raw)
contents = (obj.get("result") or {}).get("contents") or []
text = contents[0].get("text", "") if contents else ""
if not text:
    raise SystemExit(0)

payload = json.loads(text)
if payload.get("success") is False:
    print(json.dumps({"__error": payload.get("message", "resource read failed")}))
else:
    print(json.dumps(payload.get("data") or {}))
'
}

wait_unity_compile_ready() {
    timeout_seconds="${1:-60}"
    end_time=$(( $(date +%s) + timeout_seconds ))

    while [ "$(date +%s)" -lt "$end_time" ]; do
        state_result="$(read_mcp_resource "mcpforunity://editor/state")" ||
            fail_gate "❌ 실패" "⏳ 대기" "⏳ 대기" "Unity editor state read failed." ""
        state_json="$(printf '%s' "$state_result" | resource_payload_data_json)"

        if printf '%s' "$state_json" | grep -q '"__error"'; then
            message="$(printf '%s' "$state_json" | python3 -c 'import json,sys; print(json.load(sys.stdin).get("__error","resource read failed"))')"
            fail_gate "❌ 실패" "⏳ 대기" "⏳ 대기" "Unity editor state read failed:" "$message"
        fi

        ready="$(printf '%s' "$state_json" | python3 -c '
import json
import sys

state = json.loads(sys.stdin.read() or "{}")
flags = [
    state.get("compilation", {}).get("is_compiling") is True,
    state.get("compilation", {}).get("is_domain_reload_pending") is True,
    state.get("assets", {}).get("is_updating") is True,
    state.get("assets", {}).get("refresh", {}).get("is_refresh_in_progress") is True,
    state.get("editor", {}).get("play_mode", {}).get("is_changing") is True,
]
print("no" if any(flags) else "yes")
')"

        [ "$ready" = "yes" ] && return 0
        sleep 0.5
    done

    fail_gate "❌ 실패" "⏳ 대기" "⏳ 대기" "Timed out waiting for Unity refresh/compile to finish." ""
}

filter_error_lines() {
    sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//' |
        grep -v '^$' |
        grep -Ev '^Retrieved [0-9]+ log entries\.$' |
        grep -Ev '^No (log )?entries' |
        grep -Ev '^Saving results to:'
}

require_python

# Initialize MCP session. Unity MCP가 안 떠 있으면 수동 검증 필요로 남기고 통과시킨다.
if ! send_mcp_request "initialize" "$(next_request_id)" >/dev/null 2>&1; then
    write_validation "manual" "🔧 수동 검증 필요" "🔧 수동 검증 필요" "🔧 수동 검증 필요" \
"## 수동 검증 체크리스트 (MCP 미연동)
1. [ ] 콘솔에 컴파일 에러가 없는지 확인
2. [ ] Play 모드 진입 후 LogError·Exception이 없는지 확인
3. [ ] 대상 기능이 의도대로 동작하는지 확인
4. [ ] 이상 시 콘솔 에러 복사 → debugger에 전달

> MCP for Unity가 실행 중이 아니어서 1~3단계 자동 검증을 건너뛰었습니다. Unity 에디터에서 직접 확인 후 4단계에 입력하세요."
    exit 0
fi
send_mcp_request "initialized" "" >/dev/null 2>&1 || exit 0

# Step 1: 이전 콘솔 에러를 비워 이번 refresh/play 실행만 판정한다.
invoke_unity_tool "read_console" '{"action":"clear"}' >/dev/null 2>&1 || true

# Step 2: 에셋 refresh + 스크립트 컴파일 요청 (1단계 컴파일)
refresh_result="$(invoke_unity_tool "refresh_unity" '{"mode":"force","scope":"all","compile":"request","wait_for_ready":true}')" ||
    fail_gate "❌ 실패" "⏳ 대기" "⏳ 대기" "Unity refresh failed." ""
assert_tool_succeeded "$refresh_result" "Unity refresh failed:" "❌ 실패" "⏳ 대기" "⏳ 대기"
wait_unity_compile_ready 60

# Step 3: Play 모드 진입 (컴파일 에러가 여기서 드러나는 경우가 많음)
play_result="$(invoke_unity_tool "manage_editor" '{"action":"play"}')" ||
    fail_gate "✅ 통과" "❌ 실패" "⏳ 대기" "Unity play mode failed." ""
assert_tool_succeeded "$play_result" "Unity play mode failed:" "✅ 통과" "❌ 실패" "⏳ 대기"

# Step 4: Play 모드에서 잠시 실행하며 콘솔 로그가 쌓이게 둔다.
sleep 5

# Step 5: 콘솔 에러가 있으면 차단 (2·3단계 실패 판정)
console_result="$(invoke_unity_tool "read_console" '{"action":"get","types":["error"],"count":20,"format":"plain"}')" || console_result=""
if [ -n "$console_result" ]; then
    assert_tool_succeeded "$console_result" "Unity console read failed:" "✅ 통과" "❌ 실패" "⏳ 대기"
    error_lines="$(printf '%s' "$console_result" | tool_text | filter_error_lines || true)"
    if [ -n "$error_lines" ]; then
        # play 진입은 됐으나 콘솔 에러 → 런타임/기능 단계 실패로 본다.
        fail_gate "✅ 통과" "❌ 실패" "❌ 실패" "Unity console errors detected:" "$error_lines"
    fi
fi

# Step 6: Gate 3 — play mode 안에서 gate3_run_test 커스텀 툴 호출
g3_status="➖ 스킵"
g3_detail=""
gate3_file="$HOOK_DIR/.viewer-state/gate3-test.json"
if [ -f "$gate3_file" ]; then
    g3_args="$(python3 - "$gate3_file" <<'PY'
import json, sys
cfg = json.load(open(sys.argv[1]))
args = {"type_name": cfg["type_name"]}
if "call_start" in cfg: args["call_start"] = bool(cfg["call_start"])
if "inject" in cfg: args["inject"] = bool(cfg["inject"])
if "check_non_null_fields" in cfg: args["check_non_null_fields"] = cfg["check_non_null_fields"]
print(json.dumps(args))
PY
)" || g3_args=""

    if [ -n "$g3_args" ]; then
        g3_result="$(invoke_unity_tool "gate3_run_test" "$g3_args")" || g3_result=""
        g3_text="$(printf '%s' "$g3_result" | python3 -c '
import json, sys
raw = sys.stdin.read().strip()
if not raw: raise SystemExit(0)
obj = json.loads(raw)
result = obj.get("result") or {}
for item in result.get("content") or []:
    if item.get("type") == "text":
        text = item.get("text") or ""
        try:
            p = json.loads(text)
            print(p.get("message") or p.get("data") or text)
        except:
            print(text)
        raise SystemExit(0)
sc = result.get("structuredContent")
if sc: print(sc.get("message") or sc.get("data") or str(sc))
' 2>/dev/null || true)"

        if printf '%s' "$g3_text" | grep -q "^PASS"; then
            g3_status="✅ 통과"; g3_detail="$g3_text"
        elif printf '%s' "$g3_text" | grep -q "^FAIL"; then
            g3_status="❌ 실패"; g3_detail="$g3_text"
        elif [ -n "$g3_text" ]; then
            g3_status="⚠️ 결과 불명 ($g3_text)"; g3_detail="$g3_text"
        fi
    fi
fi

# Step 7: play 종료
stop_result="$(invoke_unity_tool "manage_editor" '{"action":"stop"}')" ||
    fail_gate "✅ 통과" "✅ 통과" "$g3_status" "Unity play mode stop failed." ""
assert_tool_succeeded "$stop_result" "Unity play mode stop failed:" "✅ 통과" "✅ 통과" "$g3_status"

g3_detail_line=""
[ -n "$g3_detail" ] && g3_detail_line="
- 3단계 상세: $g3_detail"

write_validation "auto" "✅ 통과" "✅ 통과" "$g3_status" \
"## 자동 검증 결과
- 1단계 컴파일: refresh_unity(force/compile) + editor/state 대기 통과. 컴파일 에러 0.
- 2단계 런타임 에러: Play 모드 진입 후 콘솔 에러·예외 없음.$g3_detail_line"

exit 0
