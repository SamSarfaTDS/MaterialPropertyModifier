using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tripledot.MaterialPropertyModifier.Editor
{
    [CustomEditor(typeof(GraphicMaterialPropertyModifier))]
    public class GraphicMaterialPropertyModifierEditor : UnityEditor.Editor
    {
        private static readonly string[] SlotFieldNames =
        {
            "property0", "property1", "property2", "property3",
            "property4", "property5", "property6", "property7"
        };

        private static readonly string[] ExcludedPropertyNames =
        {
            "m_Script", "property0", "property1", "property2", "property3",
            "property4", "property5", "property6", "property7"
        };

        // A slot added via "+ Add Property" but not yet given a propertyName has nothing to
        // persist, so this stays editor-instance-only: it resets (and the empty slot re-hides)
        // whenever the inspector is rebuilt, e.g. on deselect/reselect.
        private readonly HashSet<int> revealedEmptySlots = new HashSet<int>();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true)) {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            }

            DrawPropertiesExcluding(serializedObject, ExcludedPropertyNames);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Material Properties", EditorStyles.boldLabel);

            int shownCount = 0;
            for (int i = 0; i < SlotFieldNames.Length; i++) {
                SerializedProperty slot = serializedObject.FindProperty(SlotFieldNames[i]);
                if (!IsShown(slot, i)) {
                    continue;
                }

                shownCount++;
                DrawSlot(slot, i);
            }

            using (new EditorGUI.DisabledScope(shownCount >= SlotFieldNames.Length)) {
                if (GUILayout.Button("+ Add Property")) {
                    int hiddenIndex = FirstHiddenSlotIndex();
                    if (hiddenIndex >= 0) {
                        revealedEmptySlots.Add(hiddenIndex);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSlot(SerializedProperty slot, int index)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Slot {index + 1}", EditorStyles.miniBoldLabel, GUILayout.Width(50f));
            GUILayout.FlexibleSpace();
            bool removed = GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(60f));
            EditorGUILayout.EndHorizontal();

            if (removed) {
                ClearSlot(slot);
                revealedEmptySlots.Remove(index);
                return;
            }

            EditorGUILayout.PropertyField(slot, GUIContent.none, true);
        }

        private bool IsShown(SerializedProperty slot, int index)
        {
            return !string.IsNullOrEmpty(slot.FindPropertyRelative("propertyName").stringValue) || revealedEmptySlots.Contains(index);
        }

        private int FirstHiddenSlotIndex()
        {
            for (int i = 0; i < SlotFieldNames.Length; i++) {
                if (!IsShown(serializedObject.FindProperty(SlotFieldNames[i]), i)) {
                    return i;
                }
            }

            return -1;
        }

        private static void ClearSlot(SerializedProperty slot)
        {
            slot.FindPropertyRelative("propertyName").stringValue = string.Empty;
            slot.FindPropertyRelative("valueType").enumValueIndex = 0;
            slot.FindPropertyRelative("controlMode").enumValueIndex = 0;
            slot.FindPropertyRelative("floatValue").floatValue = 0f;
            slot.FindPropertyRelative("intValue").intValue = 0;
            slot.FindPropertyRelative("minValue").floatValue = 0f;
            slot.FindPropertyRelative("maxValue").floatValue = 0f;
            slot.FindPropertyRelative("boolValue").boolValue = false;
            slot.FindPropertyRelative("colorValue").colorValue = Color.white;
            slot.FindPropertyRelative("vectorValue").vector4Value = Vector4.zero;
        }
    }
}
