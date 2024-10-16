using System;
using UnityEngine;
using UnityEditor;
using Kaede2.UI;

namespace Kaede2.Editor.Inspectors
{
    [CustomEditor(typeof(UGUILineRenderer))]
    public class UGUILineRendererEditor : UnityEditor.Editor
    {
        private UGUILineRenderer lineRenderer;

        // Snapping threshold in local space units
        private const float SnappingThreshold = 10f;

        private void OnEnable()
        {
            lineRenderer = (UGUILineRenderer)target;
        }

        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();

            // Add buttons to add or remove points
            EditorGUILayout.Space();

            if (GUILayout.Button("Add Point"))
            {
                Undo.RecordObject(lineRenderer, "Add Point");
                AddPoint();
                EditorUtility.SetDirty(lineRenderer);
            }

            if (GUILayout.Button("Remove Last Point"))
            {
                Undo.RecordObject(lineRenderer, "Remove Point");
                RemoveLastPoint();
                EditorUtility.SetDirty(lineRenderer);
            }
        }

        private void OnSceneGUI()
        {
            if (lineRenderer == null || lineRenderer.points == null)
                return;

            RectTransform rectTransform = lineRenderer.rectTransform;
            Rect rect = rectTransform.rect;

            Handles.color = Color.red;

            for (int i = 0; i < lineRenderer.points.Length; i++)
            {
                // Convert UV to world space
                Vector2 localPoint = new Vector2(
                    Mathf.Lerp(rect.xMin, rect.xMax, lineRenderer.points[i].x),
                    Mathf.Lerp(rect.yMin, rect.yMax, lineRenderer.points[i].y)
                );
                Vector3 worldPoint = rectTransform.TransformPoint(localPoint);

                EditorGUI.BeginChangeCheck();

                float handleSize = HandleUtility.GetHandleSize(worldPoint) * 0.05f;

                Vector3 newWorldPoint = Handles.FreeMoveHandle(
                    worldPoint,
                    handleSize,
                    Vector3.zero,
                    Handles.DotHandleCap
                );

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(lineRenderer, "Move Point");

                    // Convert world space back to local space
                    Vector2 newLocalPoint = rectTransform.InverseTransformPoint(newWorldPoint);

                    // Convert local space to UV coordinates
                    Vector2 newUVPoint = new Vector2(
                        Mathf.InverseLerp(rect.xMin, rect.xMax, newLocalPoint.x),
                        Mathf.InverseLerp(rect.yMin, rect.yMax, newLocalPoint.y)
                    );

                    newUVPoint = ApplySnapping(newUVPoint, i);

                    lineRenderer.points[i] = newUVPoint;
                    EditorUtility.SetDirty(lineRenderer);
                    lineRenderer.SetVerticesDirty();
                }

                // Draw labels
                Handles.Label(worldPoint + Vector3.up * (handleSize * 2), $"{i}");

                // Draw lines between points
                if (i < lineRenderer.points.Length - 1)
                {
                    Vector2 nextLocalPoint = new Vector2(
                        Mathf.Lerp(rect.xMin, rect.xMax, lineRenderer.points[i + 1].x),
                        Mathf.Lerp(rect.yMin, rect.yMax, lineRenderer.points[i + 1].y)
                    );
                    Vector3 nextWorldPoint = rectTransform.TransformPoint(nextLocalPoint);

                    Handles.DrawDottedLine(worldPoint, nextWorldPoint, 5f);
                }
            }
        }

        private Vector2 ApplySnapping(Vector2 currentUV, int currentIndex)
        {
            Vector2 snappedUV = currentUV;

            for (int j = 0; j < lineRenderer.points.Length; j++)
            {
                if (j == currentIndex)
                    continue;

                Vector2 otherUV = lineRenderer.points[j];

                // Define a snapping threshold in UV space (e.g., 0.02 for 2%)
                const float snappingThresholdUV = 0.02f;

                // Snap UV coordinates
                if (Mathf.Abs(otherUV.x - currentUV.x) < snappingThresholdUV)
                    snappedUV.x = otherUV.x;

                if (Mathf.Abs(otherUV.y - currentUV.y) < snappingThresholdUV)
                    snappedUV.y = otherUV.y;
            }

            return snappedUV;
        }


        private void AddPoint()
        {
            Vector2[] points = lineRenderer.points ?? Array.Empty<Vector2>();

            Array.Resize(ref points, points.Length + 1);

            if (points.Length > 1)
            {
                // Add new point at a relative UV position
                Vector2 lastPoint = points[^2];
                points[^1] = lastPoint + new Vector2(0.1f, 0); // Move 10% to the right
            }
            else
            {
                points[^1] = Vector2.zero; // Start at bottom-left corner
            }

            lineRenderer.points = points;
        }


        private void RemoveLastPoint()
        {
            if (lineRenderer.points is not { Length: > 0 }) return;
            Vector2[] points = lineRenderer.points;
            Array.Resize(ref points, points.Length - 1);
            lineRenderer.points = points;
        }
    }
}