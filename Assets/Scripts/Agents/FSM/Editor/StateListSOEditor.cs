using System.IO;
using System.Linq;
using CoreSystem;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Agents.FSM.Editor
{
    [CustomEditor(typeof(StateListSO))]
    public class StateListSOEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset editorView = default;

        private Button _folderBtn;
        private Button _generateBtn;
        private Label _folderPathLabel;

        private string _folderPath;
        private StateListSO _targetData;
        
        public override VisualElement CreateInspectorGUI()
        {
            _targetData = target as StateListSO;
            
            VisualElement root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            editorView.CloneTree(root);

            _folderBtn = root.Q<Button>("FolderBtn");
            _generateBtn = root.Q<Button>("GenerateBtn");
            _folderPathLabel = root.Q<Label>("SelectedFolderLabel");
            _folderPathLabel.text = "No folder selected";

            _folderBtn.clicked += HandleFolderBtnClick;
            _generateBtn.clicked += HandleGenerateBtnClick;

            if (_targetData != null && !string.IsNullOrEmpty(_targetData.generatePath))
            {
                _folderPath = _targetData.generatePath;
                _folderPathLabel.text = FileUtil.GetProjectRelativePath(_targetData.generatePath);
            }
            
            return root;
        }

        private void HandleGenerateBtnClick()
        {
            if (string.IsNullOrEmpty(_folderPath) || !Directory.Exists(_folderPath))
            {
                EditorUtility.DisplayDialog("Error", "폴더를 선택하지 않았거나 올바르지 않습니다.", "OK");
                return;
            }

            
            int index = 0;
            string enumString = string.Join(",", _targetData.states.Select(so =>
            {
                so.assetIndex = index;
                EditorUtility.SetDirty(so);
                return $"{so.stateName} = {index++}";
            }));
            string ns = FileUtil.GetProjectRelativePath(_folderPath).Substring("Assets/".Length);
            if (ns.StartsWith("Scripts/"))
            {
                ns = ns.Substring("Scripts/".Length);
            }

            ns = ns.Replace("/", "."); // 슬래시를 .으로 변경한다.
            string code = string.Format(CodeFormat.EnumFormat, ns, _targetData.enumName, enumString);
            
            File.WriteAllText($"{_folderPath}/{_targetData.enumName}.cs", code );
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void HandleFolderBtnClick()
        {
            //타이틀, 처음으로 열릴 경로(없으면 기본 프로젝트 경로), 선택하지 않았을 때 값
            _folderPath = EditorUtility.OpenFolderPanel("폴더를 선택하세요", _folderPath, "");

            if (!string.IsNullOrEmpty(_folderPath))
            {
                Debug.Log(_folderPath);
                _targetData.generatePath = _folderPath;
                _folderPathLabel.text = FileUtil.GetProjectRelativePath(_folderPath);
                
                EditorUtility.SetDirty(_targetData);
                AssetDatabase.SaveAssets(); //모든 에셋을 검사해서 DirtyFlag가 생긴녀석을 저장한다.
            }
        }
    }
}