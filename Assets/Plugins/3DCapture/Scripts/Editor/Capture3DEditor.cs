using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace NKStudio
{
    [CustomEditor(typeof(Capture3D))]
    public class Capture3DEditor : Editor
    {
        [SerializeField] private VisualTreeAsset uxml;

        private VisualElement _root;

        private Label _titleLabel;
        private TextField _fileNameTextField;
        private Label _filePathLabel;
        private Button _filePathButton;
        private Button _captureButton;
        private Button _openButton;
        private EnumField _outputResolutionField;
        private Toggle _useHDRToggle;
        private Capture3D _capture3D;
        
        private SerializedProperty _useHDRProperty;
        private SerializedProperty _outputResolutionProperty;
        private SerializedProperty _fileNameProperty;
        private SerializedProperty _filePathProperty;
        

        private string _originPath;
        private string _versionName;

        private string DefaultPath => $"{Application.dataPath}/";

        private void InitElement()
        {
            if (uxml != null)
                _root = uxml.CloneTree();
            else
                _root = new VisualElement();
        }

        private void FindProperty()
        {
            _useHDRProperty = serializedObject.FindProperty("useHDR");
            _outputResolutionProperty = serializedObject.FindProperty("outputResolution");
            _filePathProperty = serializedObject.FindProperty("filePath");
            _fileNameProperty = serializedObject.FindProperty("fileName");
        }

        private void Bind()
        {
            // Find
            _titleLabel = _root.Q<Label>("title-label");
            _fileNameTextField = _root.Q<TextField>("fileName-field");
            _filePathLabel = _root.Q<Label>("filePath-Label");
            _filePathButton = _root.Q<Button>("filePath-button");
            _captureButton = _root.Q<Button>("capture-button");
            _openButton = _root.Q<Button>("showFolder-button");
            _outputResolutionField = _root.Q<EnumField>("outputResolution-field");
            _useHDRToggle = _root.Q<Toggle>("useHDR-toggle");

            _filePathButton.tooltip = "이미지를 저장할 경로를 지정합니다.";
            _captureButton.tooltip = "스크린샷을 찍습니다.";

            // Bind
            ModifyUserName();
            
            _filePathProperty.stringValue = EditorPrefs.GetString("Capture3D_Path", Application.dataPath);
            _originPath = _filePathProperty.stringValue;

            ModifyUserPath();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _capture3D = target as Capture3D;
        }

        public override VisualElement CreateInspectorGUI()
        {
            InitIcon();
            FindProperty();
            InitElement();
            Bind();

            _outputResolutionField.RegisterValueChangedCallback(evt =>
            {
                int resolution = _outputResolutionProperty.enumValueIndex;

                switch (resolution)
                {
                    case 0:
                        GameViewUtils.SetCustomSize(1280, 720);
                        break;
                    case 1:
                        GameViewUtils.SetCustomSize(1920, 1080);
                        break;
                    case 2:
                        GameViewUtils.SetCustomSize(2560, 1440);
                        break;
                    case 3:
                        GameViewUtils.SetCustomSize(3840, 2160);
                        break;
                }
            });

            _fileNameTextField.RegisterValueChangedCallback(evt =>
            {
                _fileNameProperty.stringValue = evt.newValue;

                ModifyUserName();
                ModifyUserPath();

                serializedObject.ApplyModifiedProperties();
            });

            _filePathButton.clicked += () =>
            {
                string path = EditorUtility.OpenFolderPanel("Save Image", DefaultPath, "");
                _filePathProperty.stringValue = path;
                _originPath = path;

                ModifyUserPath();

                // Save
                EditorPrefs.SetString("Capture3D_Path", path);
                serializedObject.ApplyModifiedProperties();
            };

            _captureButton.clicked += () =>
            {
                var path = _capture3D.ScreenShotClick();

                // 파인더 열기
                EditorUtility.RevealInFinder(path);
            };
            _openButton.clicked += () => EditorUtility.RevealInFinder(_filePathProperty.stringValue);

            _titleLabel.RegisterCallback<ClickEvent>(_ => OpenBehaviour(_capture3D));
            
            _useHDRToggle.RegisterValueChangedCallback(evt => ModifyUserPath());

            return _root;
        }

        /// <summary>
        /// 사용자가 보기 쉽도록 하는 형태로 파일 이름을 수정합니다.
        /// </summary>
        private void ModifyUserName()
        {
            int count = 0;
            _versionName = $"{_fileNameProperty.stringValue}-{count}";

            string extension = _useHDRProperty.boolValue ? "exr" : "png";
            
            // 해당 경로에 이미 존재한다면 카운트를 적용한다.
            while (File.Exists($"{_filePathProperty.stringValue}/{_versionName}.{extension}"))
            {
                _versionName = $"{_fileNameProperty.stringValue}-{count}";
                count += 1;
            }

            _fileNameTextField.SetValueWithoutNotify(_versionName);
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 사용자가 보기 쉽도록 하는 형태로 경로를 수정합니다.
        /// </summary>
        private void ModifyUserPath()
        {
            var targetDirPath = _filePathProperty.stringValue;

            // 폴더가 유효한지 확인
            if (!Directory.Exists(targetDirPath))
            {
                _filePathProperty.stringValue = DefaultPath;
                _originPath = DefaultPath;
            }

            string extension = _useHDRProperty.boolValue ? "exr" : "png";
            string modifiedPath = $"{_originPath}/{_versionName}.{extension}";
            _filePathLabel.text = modifiedPath;
        }

        /// <summary>
        /// 아이콘을 지정합니다.
        /// </summary>
        /// <param name="iconPath">아이콘 경로</param>
        /// <param name="targetObject">대상 오브젝트</param>
        private static void ApplyIcon(string iconPath, Object targetObject)
        {
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);

            if (!icon)
            {
                EditorGUIUtility.SetIconForObject(targetObject, null);
                return;
            }

            EditorGUIUtility.SetIconForObject(targetObject, icon);
        }

        private static void OpenBehaviour(MonoBehaviour targetBehaviour)
        {
            MonoScript scriptAsset = MonoScript.FromMonoBehaviour(targetBehaviour);
            string path = AssetDatabase.GetAssetPath(scriptAsset);

            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            AssetDatabase.OpenAsset(textAsset);
        }

        /// <summary>
        /// 아이콘을 설정합니다.
        /// </summary>
        private void InitIcon()
        {
            // 아이콘 폴더 경로
            string iconDirectory = AssetDatabase.GUIDToAssetPath("fbde2d2737dd04f3ea74e62196b451d2");
            string iconPath = $"{iconDirectory}/{(EditorGUIUtility.isProSkin ? "d_" : "")}Camera_Icon.png";
            ApplyIcon(iconPath, _capture3D);
        }
    }
}