using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Block{

	enum Cubeside {BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK};
	public enum BlockType {GRASS, DIRT, WATER, STONE, SAND, GOLD, BEDROCK, REDSTONE, DIAMOND, NOCRACK, 
							CRACK1, CRACK2, CRACK3, CRACK4, AIR};

	public BlockType bType;
	public bool isSolid;
	public Chunk owner;
	GameObject parent;
	public Vector3 position;

	public BlockType health;
	public int currentHealth;
	int[] blockHealthMax = {3, 3, 8, 4, 2, 3, -1, 4, 4, 0, 0, 0, 0, 0, 0};

	Vector2[,] blockUVs = {// 3,6    4,6   3,7    4,7 1-5*0.0625
		/*GRASS TOP*/		{new Vector2( 0.125f, 0.375f ), new Vector2( 0.1875f, 0.375f),
								new Vector2( 0.125f, 0.4375f ),new Vector2( 0.1875f, 0.4375f )},
		/*GRASS SIDE*/		{new Vector2( 0.1875f, 0.9375f ), new Vector2( 0.25f, 0.9375f),
								new Vector2( 0.1875f, 1.0f ),new Vector2( 0.25f, 1.0f )},
		/*DIRT*/			{new Vector2( 0.125f, 0.9375f ), new Vector2( 0.1875f, 0.9375f),
								new Vector2( 0.125f, 1.0f ),new Vector2( 0.1875f, 1.0f )},
		/*WATER*/			{ new Vector2(0.875f,0.125f),  new Vector2(0.9375f,0.125f),
 								new Vector2(0.875f,0.1875f), new Vector2(0.9375f,0.1875f)},
		/*STONE*/			{new Vector2( 0, 0.875f ), new Vector2( 0.0625f, 0.875f),
								new Vector2( 0, 0.9375f ),new Vector2( 0.0625f, 0.9375f )},
		/*SAND*/			{ new Vector2(0.125f,0.875f),  new Vector2(0.1875f,0.875f),
 								new Vector2(0.125f,0.9375f), new Vector2(0.1875f,0.9375f)},
 		/*GOLD*/			{ new Vector2(0f,0.8125f),  new Vector2(0.0625f,0.8125f),
 								new Vector2(0f,0.875f), new Vector2(0.0625f,0.875f)},
		/*BEDROCK*/			{new Vector2( 0.3125f, 0.8125f ), new Vector2( 0.375f, 0.8125f),
								new Vector2( 0.3125f, 0.875f ),new Vector2( 0.375f, 0.875f )},
		/*REDSTONE*/		{new Vector2( 0.1875f, 0.75f ), new Vector2( 0.25f, 0.75f),
								new Vector2( 0.1875f, 0.8125f ),new Vector2( 0.25f, 0.8125f )},
		/*DIAMOND*/			{new Vector2( 0.125f, 0.75f ), new Vector2( 0.1875f, 0.75f),
								new Vector2( 0.125f, 0.8125f ),new Vector2( 0.1875f, 0.8125f )},
		/*NOCRACK*/			{new Vector2( 0.6875f, 0f ), new Vector2( 0.75f, 0f),
								new Vector2( 0.6875f, 0.0625f ),new Vector2( 0.75f, 0.0625f )},
		/*CRACK1*/			{ new Vector2(0f,0f),  new Vector2(0.0625f,0f),
 								new Vector2(0f,0.0625f), new Vector2(0.0625f,0.0625f)},
 		/*CRACK2*/			{ new Vector2(0.0625f,0f),  new Vector2(0.125f,0f),
 								new Vector2(0.0625f,0.0625f), new Vector2(0.125f,0.0625f)},
 		/*CRACK3*/			{ new Vector2(0.125f,0f),  new Vector2(0.1875f,0f),
 								new Vector2(0.125f,0.0625f), new Vector2(0.1875f,0.0625f)},
 		/*CRACK4*/			{ new Vector2(0.1875f,0f),  new Vector2(0.25f,0f),
 								new Vector2(0.1875f,0.0625f), new Vector2(0.25f,0.0625f)}
						}; 


	public Block(BlockType b, Vector3 pos, GameObject p, Chunk o)
	{
		bType = b;
		owner = o;
		parent = p;
		position = pos;
		SetType(bType);
	}

	public void SetType(BlockType b)
	{
		bType = b;
		if(bType == BlockType.AIR || bType == BlockType.WATER)
			isSolid = false;
		else
			isSolid = true;

		if(bType == BlockType.WATER)
		{
			parent = owner.fluid.gameObject;
		}
		else
			parent = owner.chunk.gameObject;

		health = BlockType.NOCRACK;
		currentHealth = blockHealthMax[(int)bType];
	}

	public void Reset()
	{
		health = BlockType.NOCRACK;
		currentHealth = blockHealthMax[(int)bType];
		owner.Redraw();
	}

   
    
	public int BuildBlock(BlockType b,PlayerSetup pSetup)
	{
        Vector3 localPos = this.position;
        Vector3 chunkPos = this.owner.chunk.transform.position;

        Vector3 blockWorldPos = chunkPos + localPos;
        if (pSetup.CheckFences(blockWorldPos, pSetup.username)) {
            return 2;
        }
		
        if (b == BlockType.REDSTONE) {
            if (pSetup.isServer) {
                Debug.Log("Red stone build 1");
                pSetup.RpcFenceSync(true, blockWorldPos, pSetup.username);

            } else {
                Debug.Log("Red stone build 2");
                pSetup.RpcFenceSync(false, blockWorldPos, pSetup.username);

                //pSetup.CmdFenceSync2(true, blockWorldPos, pSetup.username);
            }
        }

        SetType(b);
        owner.Redraw();
        return 1;
	}

	public int HitBlock(PlayerSetup pSetup)
	{
        Vector3 localPos = this.position;
        Vector3 chunkPos = this.owner.chunk.transform.position;

        Vector3 blockWorldPos = chunkPos + localPos;
        
        if (pSetup.CheckFences(blockWorldPos,pSetup.username)) {
            Debug.Log("INSIDE");
            return 2;//other users fences tryed to be accessed
        }
        
        if (currentHealth == -1) return 0;
		currentHealth--;
		health++;

		if(currentHealth == (blockHealthMax[(int)bType]-1))
		{
			owner.mb.StartCoroutine(owner.mb.HealBlock(position));
		}

		if(currentHealth <= 0)
		{
            if (bType == BlockType.REDSTONE) {
                if (pSetup.isServer) {
                    pSetup.RpcFenceSync(false, blockWorldPos, pSetup.username);

                } else {
                    pSetup.RpcFenceSync(false, blockWorldPos, pSetup.username);

                   // pSetup.CmdFenceSync2(false, blockWorldPos, pSetup.username);
                }
            }
            bType = BlockType.AIR;
            isSolid = false;
            health = BlockType.NOCRACK;
            owner.Redraw();
            return 1;
		}
		owner.Redraw();
		return 0;
	}

	void CreateQuad(Cubeside side)
	{
		Mesh mesh = new Mesh();
	    mesh.name = "ScriptedMesh" + side.ToString(); 

		Vector3[] vertices = new Vector3[4];
		Vector3[] normals = new Vector3[4];
		Vector2[] uvs = new Vector2[4];
		List<Vector2> suvs = new List<Vector2>();
		int[] triangles = new int[6];

		//all possible UVs
		Vector2 uv00;
		Vector2 uv10;
		Vector2 uv01;
		Vector2 uv11;

		if(bType == BlockType.GRASS && side == Cubeside.TOP)
		{
			uv00 = blockUVs[0,0];
			uv10 = blockUVs[0,1];
			uv01 = blockUVs[0,2];
			uv11 = blockUVs[0,3];
		}
		else if(bType == BlockType.GRASS && side == Cubeside.BOTTOM)
		{
			uv00 = blockUVs[(int)(BlockType.DIRT+1),0];
			uv10 = blockUVs[(int)(BlockType.DIRT+1),1];
			uv01 = blockUVs[(int)(BlockType.DIRT+1),2];
			uv11 = blockUVs[(int)(BlockType.DIRT+1),3];
		}
		else
		{
			uv00 = blockUVs[(int)(bType+1),0];
			uv10 = blockUVs[(int)(bType+1),1];
			uv01 = blockUVs[(int)(bType+1),2];
			uv11 = blockUVs[(int)(bType+1),3];
		}

		//set cracks
		suvs.Add(blockUVs[(int)(health+1),3]);
		suvs.Add(blockUVs[(int)(health+1),2]);
		suvs.Add(blockUVs[(int)(health+1),0]);
		suvs.Add(blockUVs[(int)(health+1),1]);

		//{uv11, uv01, uv00, uv10};

		//all possible vertices 
		//top vertices
		Vector3 p0 = new Vector3( -0.5f,  -0.5f,  0.5f );
		Vector3 p1 = new Vector3(  0.5f,  -0.5f,  0.5f );
		Vector3 p2 = new Vector3(  0.5f,  -0.5f, -0.5f );
		Vector3 p3 = new Vector3( -0.5f,  -0.5f, -0.5f );		 
		//bottom vertices
		Vector3 p4 = new Vector3( -0.5f,   0.5f,  0.5f );
		Vector3 p5 = new Vector3(  0.5f,   0.5f,  0.5f );
		Vector3 p6 = new Vector3(  0.5f,   0.5f, -0.5f );
		Vector3 p7 = new Vector3( -0.5f,   0.5f, -0.5f );
		
		
		switch(side)
		{
			case Cubeside.BOTTOM:
				vertices = new Vector3[] {p0, p1, p2, p3};
				normals = new Vector3[] {Vector3.down, Vector3.down, 
											Vector3.down, Vector3.down};
				uvs = new Vector2[] {uv11, uv01, uv00, uv10};
				triangles = new int[] { 3, 1, 0, 3, 2, 1};
			break;
			case Cubeside.TOP:
				vertices = new Vector3[] {p7, p6, p5, p4};
				normals = new Vector3[] {Vector3.up, Vector3.up, 
											Vector3.up, Vector3.up};
				uvs = new Vector2[] {uv11, uv01, uv00, uv10};
				triangles = new int[] {3, 1, 0, 3, 2, 1};
			break;
			case Cubeside.LEFT:
				vertices = new Vector3[] {p7, p4, p0, p3};
				normals = new Vector3[] {Vector3.left, Vector3.left, 
											Vector3.left, Vector3.left};
				uvs = new Vector2[] {uv11, uv01, uv00, uv10};
				triangles = new int[] {3, 1, 0, 3, 2, 1};
			break;
			case Cubeside.RIGHT:
				vertices = new Vector3[] {p5, p6, p2, p1};
				normals = new Vector3[] {Vector3.right, Vector3.right, 
											Vector3.right, Vector3.right};
				uvs = new Vector2[] {uv11, uv01, uv00, uv10};
				triangles = new int[] {3, 1, 0, 3, 2, 1};
			break;
			case Cubeside.FRONT:
				vertices = new Vector3[] {p4, p5, p1, p0};
				normals = new Vector3[] {Vector3.forward, Vector3.forward, 
											Vector3.forward, Vector3.forward};
				uvs = new Vector2[] {uv11, uv01, uv00, uv10};
				triangles = new int[] {3, 1, 0, 3, 2, 1};
			break;
			case Cubeside.BACK:
				vertices = new Vector3[] {p6, p7, p3, p2};
				normals = new Vector3[] {Vector3.back, Vector3.back, 
											Vector3.back, Vector3.back};
				uvs = new Vector2[] {uv11, uv01, uv00, uv10};
				triangles = new int[] {3, 1, 0, 3, 2, 1};
			break;
		}

		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.uv = uvs;
		mesh.SetUVs(1,suvs);
		mesh.triangles = triangles;
		 
		mesh.RecalculateBounds();
		
		GameObject quad = new GameObject("Quad");
		quad.transform.position = position;
	    quad.transform.parent = this.parent.transform;

     	MeshFilter meshFilter = (MeshFilter) quad.AddComponent(typeof(MeshFilter));
		meshFilter.mesh = mesh;

	}

	int ConvertBlockIndexToLocal(int i)
	{
		if(i == -1) 
			i = World.chunkSize-1; 
		else if(i == World.chunkSize) 
			i = 0;
		return i;
	}
	
	public BlockType GetBlockType(int x, int y, int z)
	{
		Block b = GetBlock(x, y, z);
		if(b == null)
			return BlockType.AIR;
		else
			return b.bType;
	}

	public Block GetBlock(int x, int y, int z)
	{
		Block[,,] chunks;

		if(x < 0 || x >= World.chunkSize || 
		   y < 0 || y >= World.chunkSize ||
		   z < 0 || z >= World.chunkSize)
		{  //block in a neighbouring chunk
			
			Vector3 neighbourChunkPos = this.parent.transform.position + 
										new Vector3((x - (int)position.x)*World.chunkSize, 
											(y - (int)position.y)*World.chunkSize, 
											(z - (int)position.z)*World.chunkSize);
			string nName = World.BuildChunkName(neighbourChunkPos);

			x = ConvertBlockIndexToLocal(x);
			y = ConvertBlockIndexToLocal(y);
			z = ConvertBlockIndexToLocal(z);
			
			Chunk nChunk;
			if(World.chunks.TryGetValue(nName, out nChunk))
			{
				chunks = nChunk.chunkData;
			}
			else
				return null;
		}  //block in this chunk
		else
			chunks = owner.chunkData;

		return chunks[x,y,z];
	}

	public bool HasSolidNeighbour(int x, int y, int z)
	{
		try
		{
			Block b = GetBlock(x,y,z);
			if(b != null)
				return (b.isSolid || b.bType == bType);
		}
		catch(System.IndexOutOfRangeException){}

		return false;
	}

	public void Draw()
	{
		if(bType == BlockType.AIR) return;
		//solid or same neighbour
		if(!HasSolidNeighbour((int)position.x,(int)position.y,(int)position.z + 1))
			CreateQuad(Cubeside.FRONT);
		if(!HasSolidNeighbour((int)position.x,(int)position.y,(int)position.z - 1))
			CreateQuad(Cubeside.BACK);
		if(!HasSolidNeighbour((int)position.x,(int)position.y + 1,(int)position.z))
			CreateQuad(Cubeside.TOP);
		if(!HasSolidNeighbour((int)position.x,(int)position.y - 1,(int)position.z))
			CreateQuad(Cubeside.BOTTOM);
		if(!HasSolidNeighbour((int)position.x - 1,(int)position.y,(int)position.z))
			CreateQuad(Cubeside.LEFT);
		if(!HasSolidNeighbour((int)position.x + 1,(int)position.y,(int)position.z))
			CreateQuad(Cubeside.RIGHT);
	}
}
