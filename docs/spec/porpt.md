@docs/conventions/convention.md

macOS 빌드를 명령행에서 실행할 Editor 빌드 스크립트를 만들어줘.
- 위치: Assets/Editor/Build/BuildScript.cs
- 정적 메서드 BuildMac() 하나
- 씬 목록은 EditorBuildSettings.scenes에서 활성 씬만 사용
- locationPathName = "Builds/macOS/HarnessProject.app"
- target = BuildTarget.StandaloneOSX, options = BuildOptions.None
- BuildPipeline.BuildPlayer의 BuildReport로 성공 여부 판정
- 실패하면 0이 아닌 종료 코드로 프로세스 종료