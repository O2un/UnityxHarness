# EnemySpawner & WaveModule 스펙

## WaveModule (순수 C# Module)
- 데이터를 들고 있음: 어드레서블 Key, 스폰 시각, 스폰 개수, 스폰 위치
- 현재 시간을 받아 **지금 스폰해야 할 몬스터 목록**을 리턴

## EnemySpawnManager
- `Start`에서 WaveModule이 참조하는 **모든 프리팹을 프리로드**
- WaveModule을 소유
- EntryPoint로 **Tick을 돌면서** 현재 시간을 WaveModule에 넘기고, 리턴된 목록대로 **스폰 처리**
