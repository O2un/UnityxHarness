#!/usr/bin/env bash
set -euo pipefail

CONFIG="$CLAUDE_PROJECT_DIR/.claude/harness.config.json"
input=$(cat)

subagent=$(echo "$input" | jq -r '.tool_input.subagent_type // empty')
[[ -z "$subagent" ]] && exit 0          # Task 호출이 아니거나 subagent_type 없음 → 통과
[[ -f "$CONFIG" ]] || exit 0            # config 파일 없으면 전부 internal 취급

backend=$(jq -r --arg a "$subagent" '.agents[$a].backend // "internal"' "$CONFIG")
[[ "$backend" == "internal" ]] && exit 0

reason=$(jq -r --arg a "$subagent" '.agents[$a].reason // "이 에이전트는 외부 백엔드로 전환되었습니다."' "$CONFIG")

jq -n --arg r "$reason" '{hookSpecificOutput:{hookEventName:"PreToolUse",permissionDecision:"deny",permissionDecisionReason:$r}}'