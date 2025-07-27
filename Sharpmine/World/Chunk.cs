using Sharpmine.Graphics;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpmine.World
{
    internal class Chunk
    {
        public List<Vector3> chunkVerts;
        public List<Vector2> chunkUVs;
        public List<uint> chunkIndices;

        const int SIZE = 16;
        const int HEIGHT = 32;
        public Vector3 position;

        private uint indexCount;

        VAO chunkVAO;
        VBO chunkVertexVBO;
        VBO chunkUVVBO;
        IBO chunkIBO;

        Texture texture;

        public Chunk(Vector3 position)
        {
            this.position = position;

            chunkVerts = new List<Vector3>();
            chunkUVs = new List<Vector2>();
            chunkIndices = new List<uint>();

            GenBlocks();
            BuildChunk();
        }

        // Generate chunk data
        public void GenChunk() { }

        // Generate block faces
        public void GenBlocks() {
            for (int i = 0; i < 3; i++)
            {
                Block block = new Block(new Vector3(i, 0, 0));
                var frontFaceData = block.GetFace(Faces.FRONT);
                chunkVerts.AddRange(frontFaceData.vertices);
                chunkUVs.AddRange(frontFaceData.uv);
                AddIndices(1);
            }
        }

        public void AddIndices(int amountFaces)
        {
            for (int i= 0; i < amountFaces; i++)
            {
                chunkIndices.Add(0 + indexCount);
                chunkIndices.Add(1 + indexCount);
                chunkIndices.Add(2 + indexCount);
                chunkIndices.Add(2 + indexCount);
                chunkIndices.Add(3 + indexCount);
                chunkIndices.Add(0 + indexCount);

                indexCount += 4;
            }
        }

        // Process the chunk for rendering
        public void BuildChunk() {
            chunkVAO = new VAO();
            chunkVAO.Bind();

            chunkVertexVBO = new VBO(chunkVerts);
            chunkVertexVBO.Bind();
            chunkVAO.LinkToVAO(0, 3, chunkVertexVBO); // Link vertex positions to VAO at location 0

            chunkUVVBO = new VBO(chunkUVs);
            chunkUVVBO.Bind();
            chunkVAO.LinkToVAO(1, 2, chunkUVVBO); // Link texture coordinates to VAO at location 1

            chunkIBO = new IBO(chunkIndices);

            texture = new Texture("dirt.png");
        }

        // Render the chunk
        public void RenderChunk(ShaderProgram program)
        {
            program.Bind();
            chunkVAO.Bind();
            chunkIBO.Bind();
            texture.Bind();
            GL.DrawElements(PrimitiveType.Triangles, chunkIndices.Count, DrawElementsType.UnsignedInt, 0);
        }

        public void DeleteChunk()
        {
            chunkVAO.Delete();
            chunkVertexVBO.Delete();
            chunkUVVBO.Delete();
            chunkIBO.Delete();
            texture.Delete();
        }
    }
}
