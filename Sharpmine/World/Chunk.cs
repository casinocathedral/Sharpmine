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

        Block[,,] chunkBlocks = new Block[SIZE, HEIGHT, SIZE];

        public Chunk(Vector3 position)
        {
            this.position = position;

            chunkVerts = new List<Vector3>();
            chunkUVs = new List<Vector2>();
            chunkIndices = new List<uint>();

            float[,] heightMap = GenChunk(); 
            GenBlocks(heightMap);
            GenFaces(heightMap);
            BuildChunk();
        }

        // Generate chunk data
        public float[,] GenChunk() {
            float[,] heightMap = new float[SIZE, SIZE];

            SimplexNoise.Noise.Seed = (int)DateTime.Now.Ticks;
            for (int x = 0; x < SIZE; x++) {
                for (int z = 0; z < SIZE; z++) {
                    // Generate height using Simplex noise
                    float height = SimplexNoise.Noise.CalcPixel2D(x, z, 0.1f);
                    heightMap[x, z] = height;
                }
            }

            return heightMap;
        }

        // Generate block faces
        public void GenBlocks(float[,] heightMap) {
            for (int x = 0; x < SIZE; x++) {
                for (int z = 0; z < SIZE; z++) {
                    int columnHeight = (int)(heightMap[x, z] / 10);
                    for (int y = 0; y < HEIGHT; y++) {
                        BlockType type = BlockType.AIR;
                        if (y < columnHeight - 1)
                        {
                            type = BlockType.DIRT;
                        }
                        if (y == columnHeight - 1)
                        {
                            type = BlockType.GRASS;
                        }
                        if (y == 0)                        
                        {
                            type = BlockType.BEDROCK;
                        }
                        chunkBlocks[x, y, z] = new Block(new Vector3(x, y, z), type);
                    }
                }
            }
        }

        public void GenFaces(float[,] heightmap)
        {
            for (int x = 0; x < SIZE; x++)
            {
                for (int z = 0; z < SIZE; z++)
                {
                    int columnHeight = (int)(heightmap[x, z] / 10);
                    for (int y = 0; y < HEIGHT; y++)
                    {
                        // left faces (block to the left is empty && not farthest left in chunk)
                        int numFaces = 0;

                        if (chunkBlocks[x, y, z].type == BlockType.AIR)
                            continue; // Skip air blocks

                        if (x > 0)
                        {
                            if (chunkBlocks[x - 1, y, z].type == BlockType.AIR)
                            {
                                IntegrateFace(chunkBlocks[x, y, z], Faces.LEFT);
                                numFaces++;
                            }
                        }
                        else
                        {
                            IntegrateFace(chunkBlocks[x, y, z], Faces.LEFT);
                            numFaces++;
                        }

                        // right faces (block to the right is empty || farthest right in chunk)
                        if (x < SIZE-1)
                        {
                            if (chunkBlocks[x + 1, y, z].type == BlockType.AIR)
                            {
                                IntegrateFace(chunkBlocks[x, y, z], Faces.RIGHT);
                                numFaces++;
                            }
                        }
                        else
                        {
                            IntegrateFace(chunkBlocks[x, y, z], Faces.RIGHT);
                            numFaces++;
                        }

                        // top faces (block above is empty || farthest top in chunk)
                        if (y < HEIGHT - 1)
                        {
                            if (chunkBlocks[x, y + 1, z].type == BlockType.AIR)
                            {
                                IntegrateFace(chunkBlocks[x, y, z], Faces.TOP);
                                numFaces++;
                            }
                        }
                        else
                        {
                            IntegrateFace(chunkBlocks[x, y, z], Faces.TOP);
                            numFaces++;
                        }

                        // bottom faces (block below is empty || farthest bottom in chunk)
                        if (y > 0)
                        {
                            if (chunkBlocks[x, y - 1, z].type == BlockType.AIR)
                            {
                                IntegrateFace(chunkBlocks[x, y, z], Faces.BOTTOM);
                                numFaces++;
                            }
                        }
                        else
                        {
                            IntegrateFace(chunkBlocks[x, y, z], Faces.BOTTOM);
                            numFaces++;
                        }

                        // front faces (block in front is empty || farthest front in chunk)
                        if (z < SIZE - 1)
                        {
                            if (chunkBlocks[x, y, z + 1].type == BlockType.AIR)
                            {
                                IntegrateFace(chunkBlocks[x, y, z], Faces.FRONT);
                                numFaces++;
                            }
                        }
                        else
                        {
                            IntegrateFace(chunkBlocks[x, y, z], Faces.FRONT);
                            numFaces++;
                        }

                        // back faces (block behind is empty || farthest back in chunk)
                        if (z > 0)
                        {
                            if (chunkBlocks[x, y, z - 1].type == BlockType.AIR)
                            {
                                IntegrateFace(chunkBlocks[x, y, z], Faces.BACK);
                                numFaces++;
                            }
                        }
                        else
                        {
                            IntegrateFace(chunkBlocks[x, y, z], Faces.BACK);
                            numFaces++;
                        }

                        AddIndices(numFaces);
                    }
                }
            }
        }

        public void IntegrateFace(Block block, Faces face)
        {
            FaceData faceData = block.GetFace(face);
            chunkVerts.AddRange(faceData.vertices);
            chunkUVs.AddRange(faceData.uv);
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

            texture = new Texture("atlas.png");
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
