using System;

namespace MaceEvolve.SilkGL
{
    public class Globals
    {
        public const int _COORD_DATALENGTH = 3;
        /// <summary>
        /// Generates triangle fan vertices for a circle.
        /// </summary>
        public static float[] GenerateCircleVertices(float centerX, float centerY, float radius, int segments = 18, int targetVertexLength = 3)
        {
            int vertexLength = 2;
            float[] vertices = new float[(segments + 2) * targetVertexLength];

            // Central vertex
            vertices[0] = centerX;
            vertices[1] = centerY;

            for (int i = vertexLength; i < targetVertexLength; i++)
            {
                vertices[i] = 0;
            }

            for (int i = 0; i < segments; i++)
            {
                float theta = 2.0f * MathF.PI * i / segments;
                float x = centerX + MathF.Cos(theta) * radius;
                float y = centerY + MathF.Sin(theta) * radius;

                int dataStartIndex = (i + 1) * targetVertexLength;
                vertices[dataStartIndex] = x;
                vertices[dataStartIndex + 1] = y;

                for (int j = vertexLength; j < targetVertexLength; j++)
                {
                    vertices[dataStartIndex + j] = 0;
                }
            }

            // Close the fan by repeating the first perimeter vertex at the end
            vertices[(segments + 1) * _COORD_DATALENGTH] = vertices[_COORD_DATALENGTH];
            vertices[(segments + 1) * _COORD_DATALENGTH + 1] = vertices[_COORD_DATALENGTH + 1];
            vertices[(segments + 1) * _COORD_DATALENGTH + 2] = vertices[_COORD_DATALENGTH + 2];

            return vertices;
        }
    }
}
