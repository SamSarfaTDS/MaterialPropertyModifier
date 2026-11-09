using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Tripledot.MaterialPropertyModifier.Editor
{
    [CustomPropertyDrawer(typeof(MaterialPropertyBinding))]
    public class MaterialPropertyBindingDrawer : PropertyDrawer
    {
        private const float ColumnGap = 6f;
        private const float BoxPadding = 5f;
        private const float ElementGap = 8f;
        private const float PickerButtonWidth = 22f;

        private static readonly GUIContent PickerButtonContent = new GUIContent("▾", "Pick from the attached Graphic's shader properties");

        private static readonly string[] NumericControlModeNames = { "Number", "Slider" };
        private static readonly int[] NumericControlModeValues =
        {
            (int)MaterialPropertyControlMode.Number,
            (int)MaterialPropertyControlMode.Slider
        };

        private static readonly string[] VectorControlModeNames = { "Vector2", "Vector3", "Vector4" };
        private static readonly int[] VectorControlModeValues =
        {
            (int)MaterialPropertyControlMode.Vector2,
            (int)MaterialPropertyControlMode.Vector3,
            (int)MaterialPropertyControlMode.Vector4
        };

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            if (IsEmpty(property)) {
                return lineHeight + BoxPadding * 2f + ElementGap;
            }

            int rowCount = RowCount(property);
            float contentHeight = rowCount * lineHeight + (rowCount - 1) * spacing;
            return contentHeight + BoxPadding * 2f + ElementGap;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty propertyName = property.FindPropertyRelative("propertyName");

            Rect boxRect = new Rect(position.x, position.y, position.width, position.height - ElementGap);
            GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float previousLabelWidth = EditorGUIUtility.labelWidth;

            if (string.IsNullOrEmpty(propertyName.stringValue)) {
                Rect emptyRow = new Rect(boxRect.x + BoxPadding, boxRect.y + BoxPadding, boxRect.width - BoxPadding * 2f, lineHeight);
                (Rect emptyField, Rect emptyButton) = SplitWithButton(emptyRow);
                EditorGUIUtility.labelWidth = ScaledLabelWidth(emptyField.width, 90f);
                EditorGUI.PropertyField(emptyField, propertyName, new GUIContent("Property (empty)"));
                if (GUI.Button(emptyButton, PickerButtonContent)) {
                    ShowPropertyPicker(emptyButton, property);
                }
                EditorGUIUtility.labelWidth = previousLabelWidth;
                EditorGUI.EndProperty();
                return;
            }

            SerializedProperty valueType = property.FindPropertyRelative("valueType");
            SerializedProperty controlMode = property.FindPropertyRelative("controlMode");
            SerializedProperty floatValue = property.FindPropertyRelative("floatValue");
            SerializedProperty intValue = property.FindPropertyRelative("intValue");
            SerializedProperty minValue = property.FindPropertyRelative("minValue");
            SerializedProperty maxValue = property.FindPropertyRelative("maxValue");
            SerializedProperty boolValue = property.FindPropertyRelative("boolValue");
            SerializedProperty colorValue = property.FindPropertyRelative("colorValue");
            SerializedProperty vectorValue = property.FindPropertyRelative("vectorValue");

            float spacing = EditorGUIUtility.standardVerticalSpacing;

            Rect row = new Rect(boxRect.x + BoxPadding, boxRect.y + BoxPadding, boxRect.width - BoxPadding * 2f, lineHeight);
            (Rect propertyField, Rect propertyButton) = SplitWithButton(row);
            EditorGUIUtility.labelWidth = ScaledLabelWidth(propertyField.width, 90f);
            EditorGUI.PropertyField(propertyField, propertyName, new GUIContent("Property"));
            if (GUI.Button(propertyButton, PickerButtonContent)) {
                ShowPropertyPicker(propertyButton, property);
            }

            row.y += lineHeight + spacing;
            (Rect left, Rect right) = SplitRow(row);
            EditorGUIUtility.labelWidth = ScaledLabelWidth(left.width, 36f);
            EditorGUI.PropertyField(left, valueType, new GUIContent("Type"));

            var type = (MaterialPropertyValueType)valueType.enumValueIndex;
            MaterialPropertyControlMode? lockedMode = type switch {
                MaterialPropertyValueType.Color => MaterialPropertyControlMode.ColorWheel,
                MaterialPropertyValueType.Bool => MaterialPropertyControlMode.Toggle,
                _ => null
            };

            EditorGUIUtility.labelWidth = ScaledLabelWidth(right.width, 44f);
            if (lockedMode.HasValue) {
                controlMode.enumValueIndex = (int)lockedMode.Value;
                using (new EditorGUI.DisabledScope(true)) {
                    EditorGUI.PropertyField(right, controlMode, new GUIContent("Control"));
                }
            } else if (type == MaterialPropertyValueType.Vector) {
                int currentIndex = System.Array.IndexOf(VectorControlModeValues, controlMode.enumValueIndex);
                if (currentIndex < 0) {
                    currentIndex = VectorControlModeValues.Length - 1; // default to Vector4
                }
                int selectedIndex = EditorGUI.Popup(right, "Control", currentIndex, VectorControlModeNames);
                controlMode.enumValueIndex = VectorControlModeValues[selectedIndex];
            } else {
                int currentIndex = System.Array.IndexOf(NumericControlModeValues, controlMode.enumValueIndex);
                if (currentIndex < 0) {
                    currentIndex = 0;
                }
                int selectedIndex = EditorGUI.Popup(right, "Control", currentIndex, NumericControlModeNames);
                controlMode.enumValueIndex = NumericControlModeValues[selectedIndex];
            }

            var mode = (MaterialPropertyControlMode)controlMode.enumValueIndex;

            if (mode == MaterialPropertyControlMode.Slider) {
                row.y += lineHeight + spacing;
                (left, right) = SplitRow(row);
                EditorGUIUtility.labelWidth = ScaledLabelWidth(left.width, 30f);
                EditorGUI.PropertyField(left, minValue, new GUIContent("Min"));
                EditorGUIUtility.labelWidth = ScaledLabelWidth(right.width, 30f);
                EditorGUI.PropertyField(right, maxValue, new GUIContent("Max"));
            }

            row.y += lineHeight + spacing;
            EditorGUIUtility.labelWidth = ScaledLabelWidth(row.width, 44f);
            DrawValueField(row, mode, type, floatValue, intValue, boolValue, colorValue, vectorValue, minValue.floatValue, maxValue.floatValue);

            EditorGUIUtility.labelWidth = previousLabelWidth;

            EditorGUI.EndProperty();
        }

        // Every branch here binds directly to a SerializedProperty (rather than reading/writing
        // floatValue/intValue by hand) so Unity's Animation window record mode can key these fields.
        private static void DrawValueField(Rect rect, MaterialPropertyControlMode mode, MaterialPropertyValueType type, SerializedProperty floatValue, SerializedProperty intValue, SerializedProperty boolValue, SerializedProperty colorValue, SerializedProperty vectorValue, float min, float max)
        {
            switch (type) {
                case MaterialPropertyValueType.Color:
                    EditorGUI.PropertyField(rect, colorValue, new GUIContent("Value"));
                    return;
                case MaterialPropertyValueType.Vector:
                    DrawVectorField(rect, mode, vectorValue);
                    return;
                case MaterialPropertyValueType.Bool:
                    EditorGUI.PropertyField(rect, boolValue, new GUIContent("Value"));
                    return;
                case MaterialPropertyValueType.Int:
                    if (mode == MaterialPropertyControlMode.Slider) {
                        EditorGUI.IntSlider(rect, intValue, Mathf.RoundToInt(min), Mathf.RoundToInt(max), new GUIContent("Value"));
                    } else {
                        EditorGUI.PropertyField(rect, intValue, new GUIContent("Value"));
                    }
                    return;
                default:
                    if (mode == MaterialPropertyControlMode.Slider) {
                        EditorGUI.Slider(rect, floatValue, min, max, new GUIContent("Value"));
                    } else {
                        EditorGUI.PropertyField(rect, floatValue, new GUIContent("Value"));
                    }
                    return;
            }
        }

        private static readonly string[] VectorComponentNames = { "x", "y", "z", "w" };
        private static readonly string[] VectorComponentLabels = { "X", "Y", "Z", "W" };

        // Vector4's default EditorGUI.PropertyField draws a foldout with X/Y/Z/W on separate rows
        // (unlike Vector2/Vector3, which get a compact single-line field) — binding the components
        // as their own SerializedProperty sub-fields keeps this on one row while staying keyable
        // per-component in the Animation window, same as every other branch here. Trailing
        // components (e.g. z/w for a Vector2) are left untouched in the struct and still get sent
        // to the shader via SetVector — harmless, since a shader declaring float2 simply ignores them.
        private static void DrawVectorField(Rect rect, MaterialPropertyControlMode mode, SerializedProperty vectorValue)
        {
            int componentCount = mode switch {
                MaterialPropertyControlMode.Vector2 => 2,
                MaterialPropertyControlMode.Vector3 => 3,
                _ => 4
            };

            Rect fieldsRect = EditorGUI.PrefixLabel(rect, new GUIContent("Value"));

            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 14f;

            float segmentWidth = (fieldsRect.width - ColumnGap * (componentCount - 1)) / componentCount;
            float x = fieldsRect.x;
            for (int i = 0; i < componentCount; i++) {
                Rect fieldRect = new Rect(x, fieldsRect.y, segmentWidth, fieldsRect.height);
                EditorGUI.PropertyField(fieldRect, vectorValue.FindPropertyRelative(VectorComponentNames[i]), new GUIContent(VectorComponentLabels[i]));
                x = fieldRect.xMax + ColumnGap;
            }

            EditorGUIUtility.labelWidth = previousLabelWidth;
        }

        private static (Rect left, Rect right) SplitRow(Rect row)
        {
            float halfWidth = (row.width - ColumnGap) * 0.5f;
            Rect left = new Rect(row.x, row.y, halfWidth, row.height);
            Rect right = new Rect(row.x + halfWidth + ColumnGap, row.y, halfWidth, row.height);
            return (left, right);
        }

        private static (Rect field, Rect button) SplitWithButton(Rect row)
        {
            Rect button = new Rect(row.xMax - PickerButtonWidth, row.y, PickerButtonWidth, row.height);
            Rect field = new Rect(row.x, row.y, row.width - PickerButtonWidth - ColumnGap, row.height);
            return (field, button);
        }

        // Reads the attached Graphic's current shader properties and offers them as a one-click
        // autofill for propertyName/valueType/controlMode/min/max. Purely an authoring convenience —
        // the manual fields stay editable before and after a pick (e.g. to force Bool on a 0/1 Float,
        // since shaders have no native bool property type).
        private static void ShowPropertyPicker(Rect buttonRect, SerializedProperty bindingProperty)
        {
            MaterialProperty[] properties = GetMaterialProperties(bindingProperty);

            var menu = new GenericMenu();
            int addedCount = 0;
            foreach (MaterialProperty materialProperty in properties) {
                MaterialPropertyValueType? mappedType = MapType(materialProperty.type);
                if (!mappedType.HasValue) {
                    continue;
                }

                addedCount++;
                string label = $"{materialProperty.name} ({materialProperty.type})";
                menu.AddItem(new GUIContent(label), false, () => ApplyPickedProperty(bindingProperty, materialProperty, mappedType.Value));
            }

            if (addedCount == 0) {
                menu.AddDisabledItem(new GUIContent(properties.Length == 0 ? "No material found on this Graphic" : "No supported properties on this shader"));
            }

            menu.DropDown(buttonRect);
        }

        private static MaterialProperty[] GetMaterialProperties(SerializedProperty bindingProperty)
        {
            var component = bindingProperty.serializedObject.targetObject as Component;
            Graphic graphic = component != null ? component.GetComponent<Graphic>() : null;
            Material material = graphic != null ? graphic.material : null;
            return material != null ? MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { material }) : Array.Empty<MaterialProperty>();
        }

        private static MaterialPropertyValueType? MapType(MaterialProperty.PropType type)
        {
            switch (type) {
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    return MaterialPropertyValueType.Float;
                case MaterialProperty.PropType.Color:
                    return MaterialPropertyValueType.Color;
                case MaterialProperty.PropType.Vector:
                    return MaterialPropertyValueType.Vector;
                default:
                    return null;
            }
        }

        private static void ApplyPickedProperty(SerializedProperty bindingProperty, MaterialProperty materialProperty, MaterialPropertyValueType mappedType)
        {
            bindingProperty.serializedObject.Update();

            bindingProperty.FindPropertyRelative("propertyName").stringValue = materialProperty.name;
            bindingProperty.FindPropertyRelative("valueType").enumValueIndex = (int)mappedType;

            SerializedProperty controlMode = bindingProperty.FindPropertyRelative("controlMode");
            if (materialProperty.type == MaterialProperty.PropType.Range) {
                controlMode.enumValueIndex = (int)MaterialPropertyControlMode.Slider;
                bindingProperty.FindPropertyRelative("minValue").floatValue = materialProperty.rangeLimits.x;
                bindingProperty.FindPropertyRelative("maxValue").floatValue = materialProperty.rangeLimits.y;
            } else if (materialProperty.type == MaterialProperty.PropType.Color) {
                controlMode.enumValueIndex = (int)MaterialPropertyControlMode.ColorWheel;
            } else if (materialProperty.type == MaterialProperty.PropType.Vector) {
                controlMode.enumValueIndex = (int)MaterialPropertyControlMode.Vector4;
            } else {
                controlMode.enumValueIndex = (int)MaterialPropertyControlMode.Number;
            }

            bindingProperty.serializedObject.ApplyModifiedProperties();
        }

        private static float ScaledLabelWidth(float availableWidth, float preferred)
        {
            return Mathf.Clamp(preferred, 24f, availableWidth * 0.5f);
        }

        private static bool IsEmpty(SerializedProperty property)
        {
            return string.IsNullOrEmpty(property.FindPropertyRelative("propertyName").stringValue);
        }

        private static int RowCount(SerializedProperty property)
        {
            SerializedProperty valueType = property.FindPropertyRelative("valueType");
            var type = (MaterialPropertyValueType)valueType.enumValueIndex;
            if (type == MaterialPropertyValueType.Color || type == MaterialPropertyValueType.Bool || type == MaterialPropertyValueType.Vector) {
                return 3;
            }

            SerializedProperty controlMode = property.FindPropertyRelative("controlMode");
            var mode = (MaterialPropertyControlMode)controlMode.enumValueIndex;
            return mode == MaterialPropertyControlMode.Slider ? 4 : 3;
        }
    }
}
