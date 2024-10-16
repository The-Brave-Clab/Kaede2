using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class UGUILineRenderer : Graphic
    {
        public Vector2[] points;

        public float thickness = 10f;
        public bool center = true;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (points == null || points.Length < 2)
                return;

            Rect rect = rectTransform.rect;

            // Convert UV points to local positions
            Vector2[] localPoints = new Vector2[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                localPoints[i] = new Vector2(
                    Mathf.Lerp(rect.xMin, rect.xMax, points[i].x),
                    Mathf.Lerp(rect.yMin, rect.yMax, points[i].y)
                );
            }

            List<UIVertex> vertices = new List<UIVertex>();
            List<int> indices = new List<int>();

            float halfThickness = thickness * 0.5f;

            for (int i = 0; i < localPoints.Length; i++)
            {
                Vector2 point = localPoints[i];

                // Determine the directions of the previous and next segments
                Vector2 prevDir = Vector2.zero;
                if (i > 0)
                    prevDir = (point - localPoints[i - 1]).normalized;
                else if (i < localPoints.Length - 1)
                    prevDir = (localPoints[i + 1] - point).normalized;

                Vector2 nextDir = Vector2.zero;
                if (i < localPoints.Length - 1)
                    nextDir = (localPoints[i + 1] - point).normalized;
                else if (i > 0)
                    nextDir = (point - localPoints[i - 1]).normalized;

                // Calculate miter vector and length
                Vector2 miter;
                float miterLength;
                CalculateMiter(prevDir, nextDir, halfThickness, out miter, out miterLength);

                // Scale the miter vector
                miter *= miterLength;

                // Calculate the two vertices at this point
                Vector2 v1 = point + miter;
                Vector2 v2 = point - miter;

                // Add the vertices
                UIVertex vertex = UIVertex.simpleVert;
                vertex.color = color;

                vertex.position = v1;
                vertices.Add(vertex);

                vertex.position = v2;
                vertices.Add(vertex);

                // Add triangles
                if (i < localPoints.Length - 1)
                {
                    int index = i * 2;

                    indices.Add(index);
                    indices.Add(index + 2);
                    indices.Add(index + 1);

                    indices.Add(index + 1);
                    indices.Add(index + 2);
                    indices.Add(index + 3);
                }
            }

            vh.AddUIVertexStream(vertices, indices);
        }

        private void CalculateMiter(Vector2 prevDir, Vector2 nextDir, float halfThickness, out Vector2 miter,
            out float miterLength)
        {
            Vector2 tangent = (prevDir + nextDir).normalized;
            miter = new Vector2(-tangent.y, tangent.x);

            float dot = Vector2.Dot(prevDir, tangent);

            // Clamp the dot product to prevent extreme miter lengths
            const float minDot = 0.1f; // Adjust this value as needed
            dot = Mathf.Clamp(dot, minDot, 1f);

            miterLength = halfThickness / dot;
        }

        /// <summary>
        /// Gets the angle that a vertex needs to rotate to face target vertex
        /// </summary>
        /// <param name="vertex">The vertex being rotated</param>
        /// <param name="target">The vertex to rotate towards</param>
        /// <returns>The angle required to rotate vertex towards target</returns>
        private float RotatePointTowards(Vector2 vertex, Vector2 target)
        {
            return (float)(Mathf.Atan2(target.y - vertex.y, target.x - vertex.x) * (180 / Mathf.PI));
        }
    }
}