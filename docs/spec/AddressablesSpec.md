| 질문 | 답변 |
|---|---|
| 무엇을 감싸는가 | 어드레서블의 `LoadAssetAsync<T>`와 핸들 관리 |
| 외부 노출 API | `LoadAsync<T>(string key)`, `Release(string key)` — string 키만 파라미터로 받는다 |
| 캐싱 정책 | 내부 `Dictionary<string, AsyncOperationHandle>`로 캐싱. 비동기 로드가 끝나기 전에 동일 키 요청이 다시 들어와도 재로드하지 않음 |
| 비동기 처리 | UniTask 결합, `AsyncOperationHandle.ToUniTask()` |
| 등록 방식 | VContainer에 Singleton Service로 등록 |
| 실패/해제 정책 | 로드 실패 시 캐시에서 제거. 이번 단계에서는 AssetService가 핸들 생명주기를 중앙에서 관리 |