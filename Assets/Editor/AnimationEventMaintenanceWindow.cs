#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public sealed class AnimationEventMaintenanceWindow : EditorWindow
{
    private enum ParameterKind
    {
        Unknown,
        None,
        Float,
        Int,
        String,
        Object,
        AnimationEvent
    }

    [Serializable]
    private sealed class EventRow
    {
        public int Frame;
        public string FunctionName = string.Empty;
        public float FloatParameter;
        public int IntParameter;
        public string StringParameter = string.Empty;
        public UnityEngine.Object ObjectReferenceParameter;
        public SendMessageOptions MessageOptions = SendMessageOptions.RequireReceiver;
        public ParameterKind ParameterKind;
        public bool Expanded = true;

        public EventRow Clone()
        {
            return new EventRow
            {
                Frame = Frame,
                FunctionName = FunctionName,
                FloatParameter = FloatParameter,
                IntParameter = IntParameter,
                StringParameter = StringParameter,
                ObjectReferenceParameter = ObjectReferenceParameter,
                MessageOptions = MessageOptions,
                ParameterKind = ParameterKind,
                Expanded = Expanded
            };
        }
    }

    private sealed class ReceiverMethod
    {
        public string FunctionName;
        public string DisplayName;
        public ParameterKind ParameterKind;
        public Type ObjectParameterType;
        public Type ComponentType;
    }

    private Animator _animator;
    private GameObject _receiver;
    private AnimationClip _clip;
    private AnimationClip[] _controllerClips = Array.Empty<AnimationClip>();
    private string[] _controllerClipNames = Array.Empty<string>();
    private int _controllerClipIndex;
    private readonly List<EventRow> _eventRows = new();
    private readonly List<ReceiverMethod> _receiverMethods = new();
    private Vector2 _scrollPosition;
    private AnimationClip _copySourceClip;
    private int _previewFrame;
    private int _shiftFrameCount;
    private bool _autoPreview = true;
    private bool _isDirty;

    [MenuItem("Tools/Animation/Animation Event Maintenance")]
    private static void Open()
    {
        var window = GetWindow<AnimationEventMaintenanceWindow>();
        window.titleContent = new GUIContent("Animation Events");
        window.minSize = new Vector2(760f, 520f);
        window.Show();
    }

    private void OnEnable()
    {
        Undo.undoRedoPerformed += HandleUndoRedo;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= HandleUndoRedo;
        CommitPendingChanges();
        StopPreview();
    }

    private void OnGUI()
    {
        DrawTargetSection();

        if(_clip == null)
        {
            EditorGUILayout.HelpBox("AnimationClip을 지정하십시오.", MessageType.Info);
            return;
        }

        var editable = IsEditableClip(_clip);
        DrawClipInformation(editable);
        DrawPreviewSection();
        DrawBatchTools(editable);
        DrawValidationSummary();
        DrawEventList(editable);
    }

    private void DrawTargetSection()
    {
        EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);

        using(new EditorGUILayout.HorizontalScope())
        {
            EditorGUI.BeginChangeCheck();
            var newAnimator = (Animator)EditorGUILayout.ObjectField("Animator", _animator, typeof(Animator), true);
            if(EditorGUI.EndChangeCheck())
            {
                ChangeAnimator(newAnimator);
            }

            if(GUILayout.Button("Use Selection", GUILayout.Width(110f)))
            {
                var selectedAnimator = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<Animator>() : null;
                ChangeAnimator(selectedAnimator);
            }
        }

        EditorGUI.BeginChangeCheck();
        var newReceiver = (GameObject)EditorGUILayout.ObjectField("Event Receiver", _receiver, typeof(GameObject), true);
        if(EditorGUI.EndChangeCheck())
        {
            _receiver = newReceiver;
            RefreshReceiverMethods();
        }

        if(_controllerClips.Length > 0)
        {
            EditorGUI.BeginChangeCheck();
            var newIndex = EditorGUILayout.Popup("Controller Clips", _controllerClipIndex, _controllerClipNames);
            if(EditorGUI.EndChangeCheck())
            {
                _controllerClipIndex = newIndex;
                ChangeClip(_controllerClips[_controllerClipIndex]);
            }
        }

        EditorGUI.BeginChangeCheck();
        var newClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", _clip, typeof(AnimationClip), false);
        if(EditorGUI.EndChangeCheck())
        {
            ChangeClip(newClip);
        }

        EditorGUILayout.Space(4f);
    }

    private void DrawClipInformation(bool editable)
    {
        var frameRate = Mathf.Max(1f, _clip.frameRate);
        var frameCount = GetFrameCount(_clip);
        var path = AssetDatabase.GetAssetPath(_clip);

        using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField(_clip.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Frame Rate: {frameRate:0.###} / Frames: {frameCount} / Events: {_eventRows.Count}");
            EditorGUILayout.LabelField($"Asset: {(string.IsNullOrWhiteSpace(path) ? "(Memory)" : path)}");

            if(!editable)
            {
                EditorGUILayout.HelpBox("FBX 등 서브 에셋 클립은 직접 수정하지 않습니다. .anim 파일로 복제한 뒤 편집하십시오.", MessageType.Warning);

                if(GUILayout.Button("Duplicate As Editable .anim"))
                {
                    DuplicateAsEditableClip();
                }
            }
        }
    }

    private void DrawPreviewSection()
    {
        using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Frame Preview", EditorStyles.boldLabel);

            var maxFrame = GetMaxFrame(_clip);
            EditorGUI.BeginChangeCheck();
            _previewFrame = EditorGUILayout.IntSlider("Frame", Mathf.Clamp(_previewFrame, 0, maxFrame), 0, maxFrame);
            if(EditorGUI.EndChangeCheck() && _autoPreview)
            {
                PreviewFrame(_previewFrame);
            }

            using(new EditorGUILayout.HorizontalScope())
            {
                _autoPreview = EditorGUILayout.ToggleLeft("Auto Preview", _autoPreview, GUILayout.Width(110f));

                using(new EditorGUI.DisabledScope(_animator == null))
                {
                    if(GUILayout.Button("Preview"))
                    {
                        PreviewFrame(_previewFrame);
                    }

                    if(GUILayout.Button("Stop"))
                    {
                        StopPreview();
                    }
                }
            }

            if(_animator == null)
            {
                EditorGUILayout.HelpBox("프레임 미리보기에는 Animator가 필요합니다.", MessageType.None);
            }
        }
    }

    private void DrawBatchTools(bool editable)
    {
        using(new EditorGUI.DisabledScope(!editable))
        {
            using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Batch Tools", EditorStyles.boldLabel);

                using(new EditorGUILayout.HorizontalScope())
                {
                    if(GUILayout.Button("Add Event"))
                    {
                        AddEvent(_previewFrame);
                    }

                    using(new EditorGUI.DisabledScope(!_isDirty))
                    {
                        if(GUILayout.Button("Apply"))
                        {
                            ApplyChanges();
                        }

                        if(GUILayout.Button("Revert"))
                        {
                            ReloadEvents();
                        }
                    }

                    using(new EditorGUI.DisabledScope(_eventRows.Count == 0))
                    {
                        if(GUILayout.Button("Clear All"))
                        {
                            if(EditorUtility.DisplayDialog("Clear Animation Events", "현재 클립의 모든 AnimationEvent를 삭제합니다.", "Clear", "Cancel"))
                            {
                                _eventRows.Clear();
                                MarkDirty();
                            }
                        }
                    }
                }

                EditorGUILayout.Space(3f);

                using(new EditorGUILayout.HorizontalScope())
                {
                    _copySourceClip = (AnimationClip)EditorGUILayout.ObjectField("Copy From", _copySourceClip, typeof(AnimationClip), false);

                    using(new EditorGUI.DisabledScope(_copySourceClip == null))
                    {
                        if(GUILayout.Button("Replace", GUILayout.Width(80f)))
                        {
                            CopyEventsFromClip(false);
                        }

                        if(GUILayout.Button("Append", GUILayout.Width(80f)))
                        {
                            CopyEventsFromClip(true);
                        }
                    }
                }

                using(new EditorGUILayout.HorizontalScope())
                {
                    _shiftFrameCount = EditorGUILayout.IntField("Shift Frames", _shiftFrameCount);

                    using(new EditorGUI.DisabledScope(_eventRows.Count == 0 || _shiftFrameCount == 0))
                    {
                        if(GUILayout.Button("Shift", GUILayout.Width(80f)))
                        {
                            ShiftAllEvents(_shiftFrameCount);
                        }
                    }
                }
            }
        }
    }

    private void DrawValidationSummary()
    {
        if(_receiver == null)
        {
            EditorGUILayout.HelpBox("Event Receiver가 없으므로 함수 유효성을 검사할 수 없습니다.", MessageType.Info);
            return;
        }

        var missingCount = _eventRows.Count(row => !string.IsNullOrWhiteSpace(row.FunctionName) && !HasCompatibleMethod(row));
        var emptyCount = _eventRows.Count(row => string.IsNullOrWhiteSpace(row.FunctionName));
        var duplicateCount = _eventRows
            .GroupBy(row => (row.Frame, row.FunctionName))
            .Where(group => !string.IsNullOrWhiteSpace(group.Key.FunctionName) && group.Count() > 1)
            .Sum(group => group.Count());

        if(missingCount == 0 && emptyCount == 0 && duplicateCount == 0)
        {
            EditorGUILayout.HelpBox($"Validation OK · Receiver methods: {_receiverMethods.Count}", MessageType.Info);
            return;
        }

        EditorGUILayout.HelpBox($"Validation · Missing: {missingCount} / Empty: {emptyCount} / Duplicated: {duplicateCount}", MessageType.Warning);
    }

    private void DrawEventList(bool editable)
    {
        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("Animation Events", EditorStyles.boldLabel);

        if(_eventRows.Count == 0)
        {
            EditorGUILayout.HelpBox("등록된 AnimationEvent가 없습니다.", MessageType.None);
            return;
        }

        var changed = false;
        var removeIndex = -1;
        EventRow duplicateRow = null;

        using(new EditorGUI.DisabledScope(!editable))
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            for(var index = 0; index < _eventRows.Count; index++)
            {
                var row = _eventRows[index];

                using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using(new EditorGUILayout.HorizontalScope())
                    {
                        row.Expanded = EditorGUILayout.Foldout(row.Expanded, $"#{index + 1}", true);

                        EditorGUI.BeginChangeCheck();
                        row.Frame = EditorGUILayout.IntField("Frame", row.Frame, GUILayout.Width(150f));
                        row.Frame = Mathf.Clamp(row.Frame, 0, GetMaxFrame(_clip));
                        if(EditorGUI.EndChangeCheck())
                        {
                            changed = true;

                            if(_autoPreview)
                            {
                                PreviewFrame(row.Frame);
                            }
                        }

                        GUILayout.Label($"{row.Frame / Mathf.Max(1f, _clip.frameRate):0.###}s", GUILayout.Width(58f));

                        if(GUILayout.Button("▶", GUILayout.Width(28f)))
                        {
                            _previewFrame = row.Frame;
                            PreviewFrame(row.Frame);
                        }

                        if(GUILayout.Button("＋", GUILayout.Width(28f)))
                        {
                            duplicateRow = row.Clone();
                        }

                        if(GUILayout.Button("×", GUILayout.Width(28f)))
                        {
                            removeIndex = index;
                        }
                    }

                    if(row.Expanded)
                    {
                        EditorGUI.indentLevel++;

                        if(DrawFunctionPopup(row))
                        {
                            changed = true;
                        }

                        if(DrawParameterField(row))
                        {
                            changed = true;
                        }

                        EditorGUI.BeginChangeCheck();
                        row.MessageOptions = (SendMessageOptions)EditorGUILayout.EnumPopup("Message Options", row.MessageOptions);
                        if(EditorGUI.EndChangeCheck())
                        {
                            changed = true;
                        }

                        DrawRowValidation(row);
                        EditorGUI.indentLevel--;
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        if(removeIndex >= 0)
        {
            _eventRows.RemoveAt(removeIndex);
            changed = true;
        }

        if(duplicateRow != null)
        {
            duplicateRow.Frame = Mathf.Clamp(duplicateRow.Frame + 1, 0, GetMaxFrame(_clip));
            _eventRows.Add(duplicateRow);
            changed = true;
        }

        if(changed)
        {
            SortRows();
            MarkDirty();
        }
    }

    private bool DrawFunctionPopup(EventRow row)
    {
        var methods = BuildMethodOptions(row);
        var labels = methods.Select(method => method.DisplayName).ToArray();
        var currentIndex = FindMethodOptionIndex(methods, row);

        EditorGUI.BeginChangeCheck();
        var selectedIndex = EditorGUILayout.Popup("Function", currentIndex, labels);
        if(!EditorGUI.EndChangeCheck())
        {
            return false;
        }

        var selectedMethod = methods[selectedIndex];
        row.FunctionName = selectedMethod.FunctionName;
        row.ParameterKind = selectedMethod.ParameterKind;

        if(selectedMethod.ParameterKind != ParameterKind.Object)
        {
            row.ObjectReferenceParameter = null;
        }

        return true;
    }

    private bool DrawParameterField(EventRow row)
    {
        EditorGUI.BeginChangeCheck();

        switch(row.ParameterKind)
        {
            case ParameterKind.None:
                break;

            case ParameterKind.Float:
                row.FloatParameter = EditorGUILayout.FloatField("Float Parameter", row.FloatParameter);
                break;

            case ParameterKind.Int:
                row.IntParameter = EditorGUILayout.IntField("Int Parameter", row.IntParameter);
                break;

            case ParameterKind.String:
                row.StringParameter = EditorGUILayout.TextField("String Parameter", row.StringParameter);
                break;

            case ParameterKind.Object:
                var objectType = GetObjectParameterType(row);
                row.ObjectReferenceParameter = EditorGUILayout.ObjectField("Object Parameter", row.ObjectReferenceParameter, objectType, false);
                break;

            case ParameterKind.AnimationEvent:
                EditorGUILayout.HelpBox("AnimationEvent 전체 객체가 함수 인자로 전달됩니다.", MessageType.None);
                break;

            default:
                row.FloatParameter = EditorGUILayout.FloatField("Float Parameter", row.FloatParameter);
                row.IntParameter = EditorGUILayout.IntField("Int Parameter", row.IntParameter);
                row.StringParameter = EditorGUILayout.TextField("String Parameter", row.StringParameter);
                row.ObjectReferenceParameter = EditorGUILayout.ObjectField("Object Parameter", row.ObjectReferenceParameter, typeof(UnityEngine.Object), false);
                break;
        }

        return EditorGUI.EndChangeCheck();
    }

    private void DrawRowValidation(EventRow row)
    {
        if(string.IsNullOrWhiteSpace(row.FunctionName))
        {
            EditorGUILayout.HelpBox("함수명이 비어 있습니다.", MessageType.Error);
            return;
        }

        if(_receiver == null)
        {
            return;
        }

        var compatibleMethods = _receiverMethods.Where(method => method.FunctionName == row.FunctionName).ToArray();

        if(compatibleMethods.Length == 0)
        {
            EditorGUILayout.HelpBox($"'{row.FunctionName}' 함수를 Event Receiver에서 찾지 못했습니다.", MessageType.Error);
            return;
        }

        if(!compatibleMethods.Any(method => IsParameterKindCompatible(method.ParameterKind, row.ParameterKind)))
        {
            EditorGUILayout.HelpBox($"'{row.FunctionName}' 함수는 존재하지만 파라미터 형식이 맞지 않습니다.", MessageType.Error);
            return;
        }

        var componentCount = compatibleMethods.Select(method => method.ComponentType).Distinct().Count();
        if(componentCount > 1)
        {
            EditorGUILayout.HelpBox($"같은 함수명이 {componentCount}개 컴포넌트에 존재합니다. Unity가 여러 컴포넌트에 전달할 수 있습니다.", MessageType.Warning);
        }

        var duplicated = _eventRows.Count(other => other != row && other.Frame == row.Frame && other.FunctionName == row.FunctionName) > 0;
        if(duplicated)
        {
            EditorGUILayout.HelpBox("같은 프레임에 동일한 함수 이벤트가 중복되어 있습니다.", MessageType.Warning);
        }
    }

    private void ChangeAnimator(Animator animator)
    {
        CommitPendingChanges();
        StopPreview();

        _animator = animator;
        _receiver = animator != null ? animator.gameObject : null;
        RefreshControllerClips();
        RefreshReceiverMethods();

        if(_clip == null && _controllerClips.Length > 0)
        {
            ChangeClip(_controllerClips[0]);
        }

        Repaint();
    }

    private void ChangeClip(AnimationClip clip)
    {
        if(_clip == clip)
        {
            return;
        }

        CommitPendingChanges();
        StopPreview();

        _clip = clip;
        _previewFrame = 0;
        SyncControllerClipIndex();
        ReloadEvents();
    }

    private void RefreshControllerClips()
    {
        if(_animator == null || _animator.runtimeAnimatorController == null)
        {
            _controllerClips = Array.Empty<AnimationClip>();
            _controllerClipNames = Array.Empty<string>();
            _controllerClipIndex = 0;
            return;
        }

        _controllerClips = _animator.runtimeAnimatorController.animationClips
            .Where(clip => clip != null)
            .GroupBy(clip => clip.GetInstanceID())
            .Select(group => group.First())
            .OrderBy(clip => clip.name)
            .ToArray();

        _controllerClipNames = _controllerClips.Select(clip => clip.name).ToArray();
        SyncControllerClipIndex();
    }

    private void SyncControllerClipIndex()
    {
        if(_clip == null || _controllerClips.Length == 0)
        {
            _controllerClipIndex = 0;
            return;
        }

        var index = Array.IndexOf(_controllerClips, _clip);
        _controllerClipIndex = Mathf.Max(0, index);
    }

    private void RefreshReceiverMethods()
    {
        _receiverMethods.Clear();

        if(_receiver == null)
        {
            return;
        }

        var components = _receiver.GetComponents<MonoBehaviour>();

        foreach(var component in components)
        {
            if(component == null)
            {
                continue;
            }

            var componentType = component.GetType();

            for(var type = componentType; type != null && type != typeof(MonoBehaviour); type = type.BaseType)
            {
                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                foreach(var method in methods)
                {
                    if(!TryCreateReceiverMethod(componentType, method, out var receiverMethod))
                    {
                        continue;
                    }

                    _receiverMethods.Add(receiverMethod);
                }
            }
        }

        _receiverMethods.Sort((left, right) =>
        {
            var functionCompare = string.Compare(left.FunctionName, right.FunctionName, StringComparison.Ordinal);
            if(functionCompare != 0)
            {
                return functionCompare;
            }

            return string.Compare(left.DisplayName, right.DisplayName, StringComparison.Ordinal);
        });
    }

    private static bool TryCreateReceiverMethod(Type componentType, MethodInfo method, out ReceiverMethod receiverMethod)
    {
        receiverMethod = null;

        if(method.IsStatic || method.IsAbstract || method.IsGenericMethod || method.IsSpecialName || method.ReturnType != typeof(void))
        {
            return false;
        }

        if(method.Name.Contains("."))
        {
            return false;
        }

        var parameters = method.GetParameters();

        if(parameters.Length > 1)
        {
            return false;
        }

        var parameterKind = ParameterKind.None;
        var objectParameterType = typeof(UnityEngine.Object);
        var parameterText = string.Empty;

        if(parameters.Length == 1)
        {
            var parameterType = parameters[0].ParameterType;

            if(parameterType == typeof(float))
            {
                parameterKind = ParameterKind.Float;
            }
            else if(parameterType == typeof(int))
            {
                parameterKind = ParameterKind.Int;
            }
            else if(parameterType == typeof(string))
            {
                parameterKind = ParameterKind.String;
            }
            else if(parameterType == typeof(AnimationEvent))
            {
                parameterKind = ParameterKind.AnimationEvent;
            }
            else if(typeof(UnityEngine.Object).IsAssignableFrom(parameterType))
            {
                parameterKind = ParameterKind.Object;
                objectParameterType = parameterType;
            }
            else
            {
                return false;
            }

            parameterText = parameterType.Name;
        }

        receiverMethod = new ReceiverMethod
        {
            FunctionName = method.Name,
            DisplayName = parameters.Length == 0
                ? $"{componentType.Name}.{method.Name}()"
                : $"{componentType.Name}.{method.Name}({parameterText})",
            ParameterKind = parameterKind,
            ObjectParameterType = objectParameterType,
            ComponentType = componentType
        };

        return true;
    }

    private List<ReceiverMethod> BuildMethodOptions(EventRow row)
    {
        var options = new List<ReceiverMethod>
        {
            new ReceiverMethod
            {
                FunctionName = string.Empty,
                DisplayName = "<None>",
                ParameterKind = ParameterKind.None,
                ObjectParameterType = typeof(UnityEngine.Object),
                ComponentType = null
            }
        };

        options.AddRange(_receiverMethods);

        if(!string.IsNullOrWhiteSpace(row.FunctionName) && !_receiverMethods.Any(method => method.FunctionName == row.FunctionName))
        {
            options.Add(new ReceiverMethod
            {
                FunctionName = row.FunctionName,
                DisplayName = $"<Missing> {row.FunctionName}",
                ParameterKind = ParameterKind.Unknown,
                ObjectParameterType = typeof(UnityEngine.Object),
                ComponentType = null
            });
        }

        return options;
    }

    private static int FindMethodOptionIndex(IReadOnlyList<ReceiverMethod> methods, EventRow row)
    {
        for(var index = 0; index < methods.Count; index++)
        {
            var method = methods[index];

            if(method.FunctionName == row.FunctionName && IsParameterKindCompatible(method.ParameterKind, row.ParameterKind))
            {
                return index;
            }
        }

        for(var index = 0; index < methods.Count; index++)
        {
            if(methods[index].FunctionName == row.FunctionName)
            {
                return index;
            }
        }

        return 0;
    }

    private Type GetObjectParameterType(EventRow row)
    {
        var method = _receiverMethods.FirstOrDefault(candidate =>
            candidate.FunctionName == row.FunctionName &&
            candidate.ParameterKind == ParameterKind.Object);

        return method?.ObjectParameterType ?? typeof(UnityEngine.Object);
    }

    private bool HasCompatibleMethod(EventRow row)
    {
        return _receiverMethods.Any(method =>
            method.FunctionName == row.FunctionName &&
            IsParameterKindCompatible(method.ParameterKind, row.ParameterKind));
    }

    private static bool IsParameterKindCompatible(ParameterKind methodKind, ParameterKind rowKind)
    {
        if(rowKind == ParameterKind.Unknown)
        {
            return true;
        }

        return methodKind == rowKind;
    }

    private void ReloadEvents()
    {
        _eventRows.Clear();
        _isDirty = false;

        if(_clip == null)
        {
            Repaint();
            return;
        }

        var events = AnimationUtility.GetAnimationEvents(_clip);
        var maxFrame = GetMaxFrame(_clip);

        foreach(var animationEvent in events)
        {
            var frame = Mathf.Clamp(Mathf.RoundToInt(animationEvent.time * Mathf.Max(1f, _clip.frameRate)), 0, maxFrame);
            var parameterKind = ResolveParameterKind(animationEvent);

            _eventRows.Add(new EventRow
            {
                Frame = frame,
                FunctionName = animationEvent.functionName ?? string.Empty,
                FloatParameter = animationEvent.floatParameter,
                IntParameter = animationEvent.intParameter,
                StringParameter = animationEvent.stringParameter ?? string.Empty,
                ObjectReferenceParameter = animationEvent.objectReferenceParameter,
                MessageOptions = animationEvent.messageOptions,
                ParameterKind = parameterKind
            });
        }

        SortRows();
        Repaint();
    }

    private ParameterKind ResolveParameterKind(AnimationEvent animationEvent)
    {
        var candidates = _receiverMethods
            .Where(method => method.FunctionName == animationEvent.functionName)
            .ToArray();

        if(candidates.Length == 1)
        {
            return candidates[0].ParameterKind;
        }

        if(animationEvent.objectReferenceParameter != null && candidates.Any(method => method.ParameterKind == ParameterKind.Object))
        {
            return ParameterKind.Object;
        }

        if(!string.IsNullOrEmpty(animationEvent.stringParameter) && candidates.Any(method => method.ParameterKind == ParameterKind.String))
        {
            return ParameterKind.String;
        }

        if(animationEvent.intParameter != 0 && candidates.Any(method => method.ParameterKind == ParameterKind.Int))
        {
            return ParameterKind.Int;
        }

        if(Math.Abs(animationEvent.floatParameter) > float.Epsilon && candidates.Any(method => method.ParameterKind == ParameterKind.Float))
        {
            return ParameterKind.Float;
        }

        if(candidates.Any(method => method.ParameterKind == ParameterKind.None))
        {
            return ParameterKind.None;
        }

        return candidates.FirstOrDefault()?.ParameterKind ?? ParameterKind.Unknown;
    }

    private void AddEvent(int frame)
    {
        _eventRows.Add(new EventRow
        {
            Frame = Mathf.Clamp(frame, 0, GetMaxFrame(_clip)),
            FunctionName = string.Empty,
            ParameterKind = ParameterKind.None
        });

        SortRows();
        MarkDirty();
    }

    private void CopyEventsFromClip(bool append)
    {
        if(_copySourceClip == null || _clip == null)
        {
            return;
        }

        if(!append)
        {
            _eventRows.Clear();
        }

        var sourceFrameRate = Mathf.Max(1f, _copySourceClip.frameRate);
        var targetMaxFrame = GetMaxFrame(_clip);

        foreach(var sourceEvent in AnimationUtility.GetAnimationEvents(_copySourceClip))
        {
            var sourceFrame = Mathf.RoundToInt(sourceEvent.time * sourceFrameRate);

            _eventRows.Add(new EventRow
            {
                Frame = Mathf.Clamp(sourceFrame, 0, targetMaxFrame),
                FunctionName = sourceEvent.functionName ?? string.Empty,
                FloatParameter = sourceEvent.floatParameter,
                IntParameter = sourceEvent.intParameter,
                StringParameter = sourceEvent.stringParameter ?? string.Empty,
                ObjectReferenceParameter = sourceEvent.objectReferenceParameter,
                MessageOptions = sourceEvent.messageOptions,
                ParameterKind = ResolveParameterKind(sourceEvent)
            });
        }

        SortRows();
        MarkDirty();
    }

    private void ShiftAllEvents(int frameOffset)
    {
        var maxFrame = GetMaxFrame(_clip);

        foreach(var row in _eventRows)
        {
            row.Frame = Mathf.Clamp(row.Frame + frameOffset, 0, maxFrame);
        }

        SortRows();
        MarkDirty();
    }

    private void CommitPendingChanges()
    {
        if(!_isDirty || _clip == null || !IsEditableClip(_clip))
        {
            return;
        }

        ApplyChanges();
    }

    private void ApplyChanges()
    {
        if(_clip == null || !IsEditableClip(_clip))
        {
            return;
        }

        SortRows();
        Undo.RegisterCompleteObjectUndo(_clip, "Edit Animation Events");

        var frameRate = Mathf.Max(1f, _clip.frameRate);
        var events = _eventRows.Select(row => new AnimationEvent
        {
            time = Mathf.Clamp(row.Frame / frameRate, 0f, _clip.length),
            functionName = row.FunctionName ?? string.Empty,
            floatParameter = row.FloatParameter,
            intParameter = row.IntParameter,
            stringParameter = row.StringParameter ?? string.Empty,
            objectReferenceParameter = row.ObjectReferenceParameter,
            messageOptions = row.MessageOptions
        }).ToArray();

        AnimationUtility.SetAnimationEvents(_clip, events);
        EditorUtility.SetDirty(_clip);
        AssetDatabase.SaveAssetIfDirty(_clip);
        _isDirty = false;
        Repaint();
    }

    private void DuplicateAsEditableClip()
    {
        if(_clip == null)
        {
            return;
        }

        var sourcePath = AssetDatabase.GetAssetPath(_clip);
        var defaultDirectory = string.IsNullOrWhiteSpace(sourcePath) ? "Assets" : Path.GetDirectoryName(sourcePath)?.Replace("\\", "/");
        var savePath = EditorUtility.SaveFilePanelInProject("Duplicate Animation Clip", $"{_clip.name}_Editable", "anim", "편집 가능한 .anim 파일로 저장하십시오.", defaultDirectory);

        if(string.IsNullOrWhiteSpace(savePath))
        {
            return;
        }

        var duplicatedClip = Instantiate(_clip);
        duplicatedClip.name = Path.GetFileNameWithoutExtension(savePath);
        AssetDatabase.CreateAsset(duplicatedClip, savePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ChangeClip(duplicatedClip);
        Selection.activeObject = duplicatedClip;
    }

    private void PreviewFrame(int frame)
    {
        if(_animator == null || _clip == null)
        {
            return;
        }

        _previewFrame = Mathf.Clamp(frame, 0, GetMaxFrame(_clip));

        if(!AnimationMode.InAnimationMode())
        {
            AnimationMode.StartAnimationMode();
        }

        AnimationMode.BeginSampling();
        AnimationMode.SampleAnimationClip(_animator.gameObject, _clip, _previewFrame / Mathf.Max(1f, _clip.frameRate));
        AnimationMode.EndSampling();

        SceneView.RepaintAll();
        Repaint();
    }

    private static void StopPreview()
    {
        if(AnimationMode.InAnimationMode())
        {
            AnimationMode.StopAnimationMode();
        }

        SceneView.RepaintAll();
    }

    private static bool IsEditableClip(AnimationClip clip)
    {
        if(clip == null)
        {
            return false;
        }

        var path = AssetDatabase.GetAssetPath(clip);

        if(string.IsNullOrWhiteSpace(path))
        {
            return true;
        }

        return !AssetDatabase.IsSubAsset(clip) && string.Equals(Path.GetExtension(path), ".anim", StringComparison.OrdinalIgnoreCase);
    }

    private static int GetFrameCount(AnimationClip clip)
    {
        if(clip == null)
        {
            return 0;
        }

        return Mathf.Max(1, Mathf.CeilToInt(clip.length * Mathf.Max(1f, clip.frameRate)));
    }

    private static int GetMaxFrame(AnimationClip clip)
    {
        return Mathf.Max(0, GetFrameCount(clip) - 1);
    }

    private void SortRows()
    {
        _eventRows.Sort((left, right) =>
        {
            var frameCompare = left.Frame.CompareTo(right.Frame);
            if(frameCompare != 0)
            {
                return frameCompare;
            }

            return string.Compare(left.FunctionName, right.FunctionName, StringComparison.Ordinal);
        });
    }

    private void MarkDirty()
    {
        _isDirty = true;
        Repaint();
    }

    private void HandleUndoRedo()
    {
        ReloadEvents();
    }
}
#endif
