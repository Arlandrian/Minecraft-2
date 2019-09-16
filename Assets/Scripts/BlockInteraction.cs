using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class BlockInteraction : NetworkBehaviour {

	public GameObject cam;
	Block.BlockType buildtype = Block.BlockType.STONE;
    
	// Use this for initialization
	void Start () {
        
	}

	// Update is called once per frame
	void Update () {
		
		if(Input.GetKeyDown("1"))
			buildtype = Block.BlockType.STONE;
		if(Input.GetKeyDown("2"))
			buildtype = Block.BlockType.DIAMOND;
		if(Input.GetKeyDown("3"))
			buildtype = Block.BlockType.REDSTONE;
		if(Input.GetKeyDown("4"))
			buildtype = Block.BlockType.GOLD;

		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
                       
   			//for cross hairs
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 10))
            {
   				Chunk hitc;
   				if(!World.chunks.TryGetValue(hit.collider.gameObject.name, out hitc)) return;

                string chunkName = hit.collider.gameObject.name;

                   Vector3 hitBlock;
   				if(Input.GetMouseButtonDown(0))
   				{
   					hitBlock = hit.point - hit.normal/2.0f;
   					
   				}
   				else
   				 	hitBlock = hit.point + hit.normal/2.0f;

				Block b = World.GetWorldBlock(hitBlock);
				Debug.Log(b.position);
				hitc = b.owner;
                //Update 0 ise update yapma
                //Update 1 ise update yap
                //Update 2 ise direk çık
				int update = 0;
                bool isHit = false;
                if (Input.GetMouseButtonDown(0)) {
                    //Break block
                    update = b.HitBlock(this.GetComponent<PlayerSetup>());
                    isHit = true;

                } else {
                    //Add new Block
					update = b.BuildBlock(buildtype,this.GetComponent<PlayerSetup>());
				}
                if (update == 2)
                    return;
				
				if(update == 1)
   				{
   					hitc.changed = true;
	   				List<string> updates = new List<string>();
	   				float thisChunkx = hitc.chunk.transform.position.x;
	   				float thisChunky = hitc.chunk.transform.position.y;
	   				float thisChunkz = hitc.chunk.transform.position.z;

	   				//update neighbours?
	   				if(b.position.x == 0) 
	   					updates.Add(World.BuildChunkName(new Vector3(thisChunkx-World.chunkSize,thisChunky,thisChunkz)));
					if(b.position.x == World.chunkSize - 1) 
						updates.Add(World.BuildChunkName(new Vector3(thisChunkx+World.chunkSize,thisChunky,thisChunkz)));
					if(b.position.y == 0) 
						updates.Add(World.BuildChunkName(new Vector3(thisChunkx,thisChunky-World.chunkSize,thisChunkz)));
					if(b.position.y == World.chunkSize - 1) 
						updates.Add(World.BuildChunkName(new Vector3(thisChunkx,thisChunky+World.chunkSize,thisChunkz)));
					if(b.position.z == 0) 
						updates.Add(World.BuildChunkName(new Vector3(thisChunkx,thisChunky,thisChunkz-World.chunkSize)));
					if(b.position.z == World.chunkSize - 1) 
						updates.Add(World.BuildChunkName(new Vector3(thisChunkx,thisChunky,thisChunkz+World.chunkSize)));
                    
		   			foreach(string cname in updates)
		   			{
		   				Chunk c;
						if(World.chunks.TryGetValue(cname, out c)) 
                        {
							c.Redraw();
				   		}
				   	}

                    if(isServer) {
                        //you're a server, send an RPC to other clientss
                        if (isHit) {
                            Debug.Log("1-RPC is server: " + isServer);

                            RpcSynchChunks(chunkName, b.position, Block.BlockType.AIR, updates.ToArray());

                        } else {
                            RpcSynchChunks(chunkName, b.position, buildtype, updates.ToArray());
                        }
                    } else {
                        //you're a client, send a command to the server so that other clients can get the memo
                        if (isHit) {
                            Debug.Log("2-RPC is server: " + isServer);
                            
                            CmdSynchChunks(chunkName, b.position, Block.BlockType.AIR, updates.ToArray());

                        } else {
                            CmdSynchChunks(chunkName, b.position, buildtype, updates.ToArray());
                        }
                    }


                } else {
                    if (isServer) {
                        //you're a server, send an RPC to other clientss
                        if (isHit) {
                            Debug.Log("1-RPC is server: " + isServer);

                            RpcSynchChunks(chunkName, b.position, Block.BlockType.AIR, null);

                        } else {
                            RpcSynchChunks(chunkName, b.position, buildtype, null);
                        }
                    } else {
                        //you're a client, send a command to the server so that other clients can get the memo
                        if (isHit) {
                            Debug.Log("2-RPC is server: " + isServer);

                            CmdSynchChunks(chunkName, b.position, Block.BlockType.AIR, null);

                        } else {
                            CmdSynchChunks(chunkName, b.position, buildtype, null);
                        }
                    }
                }
            }
   		}
	}
    
    [Command]
    void CmdSynchChunks(string chunkName, Vector3 blockPosition, Block.BlockType newType, string[] redrawList) {
        Debug.Log("4-RPC is server: " + isServer);
        
        RpcSynchChunks(chunkName, blockPosition, newType, redrawList);

    }

    [ClientRpc]
    void RpcSynchChunks(string chunkName, Vector3 blockPosition,Block.BlockType newType, string [] redrawList) {
        Debug.Log("3-RPC is server: " + isServer);
        Debug.Log("isLocalPlayer: " + isLocalPlayer);
        if (isLocalPlayer)
            return;

        bool hit = false;
        if(newType == Block.BlockType.AIR) {
            hit = true;
        }
        Chunk c;
        if (World.chunks.TryGetValue(chunkName, out c)) {
            Block b = c.chunkData[(int)blockPosition.x, (int)blockPosition.y, (int)blockPosition.z];
            if (hit) {
                Debug.Log("block hit");
                if (b.HitBlock(this.GetComponent<PlayerSetup>()) == 0) {
                    c.changed = true;
                }
            } else {
                b.BuildBlock(newType,GetComponent<PlayerSetup>());
                c.changed = true;
            }

        }

        if(redrawList != null)
        foreach (string cname in redrawList) {
            //Chunk c;
            if (World.chunks.TryGetValue(cname, out c)) {
                c.Redraw();
            }
        }

    }
    
}

