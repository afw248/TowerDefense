using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Agents.FSM.Editor
{
    [CustomEditor(typeof(StateSO))]
    public class StateSOEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset editorView = default;

        private StateSO _targetData;
        
        public override VisualElement CreateInspectorGUI()
        {
            _targetData = target as StateSO;
            
            VisualElement root = new VisualElement();

            editorView.CloneTree(root);

            FillDropdownField(root);
            
            return root;
        }

        private void FillDropdownField(VisualElement root)
        {
            DropdownField field = root.Q<DropdownField>("ClassNameDropdown");
            
            //StateSO 클래스가 속해있는 어셈블리를 가져온다.(모든 어쎔을 가져올 수도 있지만, 그럼 너무 느리다)
            Assembly stateAssembly = Assembly.GetAssembly(typeof(StateSO));
            
            IEnumerable<string> choices = stateAssembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(AgentState)))
                .Select(type => type.FullName);
            
            field.choices.AddRange(choices);

            // if (_targetData != null && !string.IsNullOrEmpty(_targetData.className)
            //                         && field.choices.Contains(_targetData.className))
            // {
            //     field.value = _targetData.className;
            // }
            
            if (_targetData != null && field.choices.Count > 0 && string.IsNullOrEmpty(_targetData.className))
            {
                _targetData.className = field.choices.First();
                EditorUtility.SetDirty(_targetData);
            }
            
            AssetDatabase.SaveAssetIfDirty(_targetData);
        }
    }
}