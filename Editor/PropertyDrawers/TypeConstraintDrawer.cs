using UnityEngine;
using UnityEditor;
using System;

namespace DGTools
{
    [CustomPropertyDrawer(typeof(TypeConstraintAttribute))]
    public class TypeConstraintDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                throw new Exception(string.Format("The property type ({0}) is different from reference ({1})", property.propertyType, SerializedPropertyType.ObjectReference));

            }

            GameObject obj = fieldInfo.GetValue(property.serializedObject.targetObject) as GameObject;
            if (obj != null && !(fieldInfo.GetValue(property.serializedObject.targetObject) is GameObject))
            {
                throw new Exception(string.Format("The targeted object should be a GameObject!"));
            }

            var constraint = attribute as TypeConstraintAttribute;

            if (DragAndDrop.objectReferences.Length > 0)
            {
                var draggedObject = DragAndDrop.objectReferences[0] as GameObject;

                if (draggedObject == null || (draggedObject != null && draggedObject.GetComponent(constraint.type) == null))
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }

            if (property.objectReferenceValue != null)
            {
                GameObject go = property.objectReferenceValue as GameObject;
                if (go != null && go.GetComponent(constraint.type) == null)
                {
                    property.objectReferenceValue = null;
                }
            }

            property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(GameObject), true);
        }
    }
}
