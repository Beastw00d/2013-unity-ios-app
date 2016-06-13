using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeManager : MonoBehaviour 
{
	// script in charge of storing, placing, and randomizing the tree sections of the level

	// the mobile trans is used to position tree rows in relation to
	public Transform mobileTrans;

	// parent transforms for each tree row and index to keep track of currently watched transform and the offset amount between rows vertically
	public Transform[] parentRowArr;
	private int curIndex = 0; 
	public float treeOffset;

	// parent pool transform to hold all unused tree pieces
	public Transform poolParent;

	// pools of the different kinds of tree pieces
	public Transform[] treeBlockArr = new Transform[32], treeOpenLArr = new Transform[32], treeOpenRArr = new Transform[32], 
		treeClosedLArr = new Transform[32], treeClosedRArr = new Transform[32], treeClosedVArr = new Transform[32], treeOpenVArr = new Transform[32];

	private List<Transform> treeBlockPool = new List<Transform>(), treeOpenLPool = new List<Transform>(), treeOpenRPool = new List<Transform>(), 
		treeClosedLPool = new List<Transform>(), treeClosedRPool = new List<Transform>(), treeClosedVPool = new List<Transform>(), treeOpenVPool = new List<Transform>();
	
	// arrays to store the most recent tree point data: 0 empty, -1 open left, -2 open right, 0 closed V, 1 closed left, 2 closed right, 3 block, 4 open V
	public int[] treeArrCur = new int[4]{0,0,0,0};

	// ints to store path locations.  pathC is only used during the fade out
	int pathA, pathB, pathC = -1;

	// int for whether the paths are split (negative) or merged (positive) with magnitude representing number of rows spent in that state
	int splitInt = 0;

	// ------------------------------------------------------------------------------------------------------


	// populate the tree pool lists
	void Start()
	{
		for (int i = 0 ; i < treeBlockArr.Length ; i++)
		{
			treeBlockPool.Add(treeBlockArr[i]);
			treeOpenLPool.Add(treeOpenLArr[i]);
			treeOpenRPool.Add(treeOpenRArr[i]);
			treeClosedLPool.Add(treeClosedLArr[i]);
			treeClosedRPool.Add(treeClosedRArr[i]);
		}

		for (int i = 0 ; i < treeClosedVArr.Length ; i++)
		{
			treeClosedVPool.Add(treeClosedVArr[i]);
			treeOpenVPool.Add(treeOpenVArr[i]);
		}
	}


	// intitialize a new tree area and starts the routine to spawn the given number of tree rows
	public void StartTreeArea()
	{
		pathC = -1;

		if (Random.Range(0,2) == 1)
		{
			treeArrCur[0] = treeArrCur[3] = -3;
			treeArrCur[1] = 2;
			treeArrCur[2] = 1;

			splitInt = -2;
			pathA = 0;
			pathB = 3;
		}
		else
		{
			treeArrCur[1] = treeArrCur[2] = -3;
			treeArrCur[0] = 1;
			treeArrCur[3] = 2;

			splitInt = -2;
			pathA = 1;
			pathB = 2;
		}
	
		// reset the current index and position all tree rows above the mobile transform
		Vector3 basePos = mobileTrans.position + new Vector3(0,250,0);
		curIndex = 0;

		for (int i = 0 ; i < parentRowArr.Length ; i++)
		{
			parentRowArr[i].position = new Vector3( basePos.x, basePos.y + i * (treeOffset), basePos.z);
		}

		// start the coroutine to spawn the trees over time 
		StartCoroutine("SpawnTreesOverTime");
	}


	// spawn the given number of tree rows over time
	IEnumerator SpawnTreesOverTime()
	{
		// spawn initial row manually
		SpawnTreeRow(0, 48);

		// this int is used to index all other tree rows and assign the appropriate sorting order
		int treeIndex = 1;

		// this int remains 3 until it is time to end the tree area, at which point it is decreased to 2 and decrements to 0 for a smooth tree fade out
		int remainingRows = 3;

		// spawn all initial rows with appropriate sprite sorting order int
		while ( treeIndex < parentRowArr.Length )
		{
			GetTreeRow(treeIndex, 48 - treeIndex, remainingRows);
			treeIndex++;
		}

		curIndex = 0;

		// continue spawning rows over time
		while(true)
		{
			if (LevelController.gameTime >= LevelController.advanceTime && remainingRows > 2)
				remainingRows = 2;

			// if this row is below the bottom limit, then move it to the top and spawn a row of trees with the appropirate sprite sorting order int
			if ( parentRowArr[curIndex].position.y < mobileTrans.position.y )
			{
				int oldIndex = curIndex;
				curIndex++;
				if (curIndex == 10) curIndex = 0;	
				parentRowArr[oldIndex].position = new Vector3( 0,  parentRowArr[oldIndex].position.y + 280, 0);

				GetTreeRow(oldIndex, 48 - treeIndex, remainingRows);

				if (remainingRows < 3) remainingRows--;
				if (remainingRows < 0) break;
				treeIndex++;
			}

			yield return null;
		}
	}


	// randomize a row of trees based on the previous row's tree placement
	void GetTreeRow(int thisRowIndex, int sortNum, int remainingRows)
	{
		// 'reset' the current array of tree ints
		for (int i = 0 ; i < 4 ; i++)
		{
			treeArrCur[i] = 3;
		}

		// bool is set to true if we are fading out the tree rows manually instead of generating random geometry
		bool isFadingOut = false;

		// based on remaining rows, find out if we need to start fading away the trees
		if (remainingRows == 2 && pathB < 0)
		{
			//Bebug.Log ("2 rows with path A: " + pathA.ToString() + " , and pathB: " + pathB.ToString());

			if (pathA == 0)
			{
				//Bebug.Log ("2 B");
				treeArrCur[0] = -3;
				treeArrCur[1] = -2;

				pathB = 1;
				isFadingOut = true;
			}
			else if (pathA == 3)
			{
				//Bebug.Log ("2 C");
				treeArrCur[2] = -1;
				treeArrCur[3] = -3;

				pathA = 2;
				pathB = 3;
				isFadingOut = true;
			}
		}
		else if (remainingRows == 1)
		{
			//Bebug.Log ("1 row with path A: " + pathA.ToString() + " , and pathB: " + pathB.ToString());

			if (pathB < 0)
			{
				//Bebug.Log ("1 B");

				if (pathA == 1)
				{
					//Bebug.Log ("1 C");

					treeArrCur[0] = -1;
					treeArrCur[1] = -3;
					treeArrCur[2] = -2;

					pathA = 0;
					pathB = 1;
					pathC = 2;
					isFadingOut = true;
				}
				else if (pathA == 2)
				{
					//Bebug.Log ("1 D");

					treeArrCur[1] = -1;
					treeArrCur[2] = -3;
					treeArrCur[3] = -2;

					pathA = 1;
					pathB = 2;
					pathC = 3;
					isFadingOut = true;
				}
			}
			else if (pathB == 1 && pathA == 0)
			{
				//Bebug.Log ("1 E");

				treeArrCur[0] = treeArrCur[1] = -3;
				treeArrCur[2] = -2;

				pathC = 2;
				isFadingOut = true;
			}
			else if (pathB == 3 && pathA == 2)
			{
				//Bebug.Log ("1 F");

				treeArrCur[1] = -1;
				treeArrCur[2] = treeArrCur[3] = -3;

				pathA = 1;
				pathB = 2; 
				pathC = 3;
				isFadingOut = true;
			}
		}
		else if (remainingRows == 0)
		{
			//Bebug.Log ("0 rows with path A: " + pathA.ToString() + " , and pathB: " + pathB.ToString());

			if (pathC < 0)
			{
				//Bebug.Log ("0 rows with path C less than 0");

				if (pathA == 0 && pathB == 3)
				{
					//Bebug.Log ("0 C");
					treeArrCur[0] = treeArrCur[3] = -3;
					treeArrCur[1] = -2;
					treeArrCur[2] = -1;

					isFadingOut = true;
				}
				else if (pathA == 0 && pathB == 2)
				{
					//Bebug.Log ("0 D");
					treeArrCur[0] = treeArrCur[2] = -3;
					treeArrCur[1] = 4;
					treeArrCur[3] = -2;
					
					isFadingOut = true;
				}
				else if (pathA == 1 && pathB == 3)
				{
					//Bebug.Log ("0 E");
					treeArrCur[1] = treeArrCur[3] = -3;
					treeArrCur[0] = -1;
					treeArrCur[2] = 4;
					
					isFadingOut = true;
				}
				else if (pathA == 1 && pathB == 2)
				{
					//Bebug.Log ("0 F");
					treeArrCur[1] = treeArrCur[2] = -3;
					treeArrCur[0] = -1;
					treeArrCur[3] = -2;
					
					isFadingOut = true;
				}
				else
					 Debug.Log ("0 G ERROR: pathA: " + pathA.ToString() + " , pathB: " + pathB.ToString());
			}
			else if (pathC == 3)
			{
				//Bebug.Log ("0 rows (H) with pathC: " + pathC.ToString());
				treeArrCur[1] = treeArrCur[2] = treeArrCur[3] = -3;
				treeArrCur[0] = -1;

				isFadingOut = true;
			}
			else if (pathC == 2)
			{
				//Bebug.Log ("0 rows (I) with pathC: " + pathC.ToString());
				treeArrCur[0] = treeArrCur[1] = treeArrCur[2] = -3;
				treeArrCur[3] = -2;
				
				isFadingOut = true;
			}
		}

		// if the trees are NOT being manually faded, we use random generation
		if (!isFadingOut)
		{
			// first find whether or not the paths will split, merge, or stay as is
			if (splitInt > 0)
			{
				int splitAbs = Mathf.Abs(splitInt);
				int upperLimit = 1 + Mathf.RoundToInt( (splitAbs * splitAbs) / 2 - 0.1f);

				if (remainingRows > 2 && Random.Range(0, upperLimit) > splitAbs) splitInt = -1;
				else splitInt++;
			}

			else if (splitInt < 0 && (Mathf.Abs (pathA - pathB) < 3) )
			{
				int splitAbs = Mathf.Abs(splitInt);
				int upperLimit = 1 + Mathf.RoundToInt( (splitAbs * splitAbs) / 2 - 0.1f );
				
				if (remainingRows > 2 && Random.Range(0, upperLimit) > splitAbs)	splitInt = 1;
				else splitInt--;
			}

			// if we are splitting a merged path
			if (splitInt == -1)
			{
				if (pathA == 0)
				{
					pathA = 0;
					pathB = 1;

					treeArrCur[0] = -3;
					treeArrCur[1] = -2;
				}
				else if (pathA == 3)
				{
					pathA = 2;
					pathB = 3;

					treeArrCur[2] = -1;
					treeArrCur[3] = -3;
				}
				else if (pathA == 1)
				{
					pathA = 0;
					pathB = 2;

					treeArrCur[0] = -1;
					treeArrCur[1] = 0;
					treeArrCur[2] = -2;
				}
				else if (pathA == 2)
				{
					pathA = 1;
					pathB = 3;

					treeArrCur[1] = -1;
					treeArrCur[2] = 0;
					treeArrCur[3] = -2;
				}
			}

			// else if we are merging adjacent paths
			else if (splitInt == 1)
			{
				int rand = Random.Range(0, 2);

				if (pathA == 0)
				{
					if (pathB == 1 )
					{
						if (rand == 0 && remainingRows > 2)
						{
							pathA = 0;
							treeArrCur[0] = -3;
							treeArrCur[1] = 2;
						}
						else
						{
							pathA = 1;
							treeArrCur[0] = 1;
							treeArrCur[1] = -3;
						}
					}
					else if (pathB == 2)
					{
						pathA = 1;
					
						treeArrCur[0] = 1;
						treeArrCur[1] = 4;
						treeArrCur[2] = 2;
					}
				}
				else if (pathB == 3)
				{
					if (pathA == 2)
					{
						if (rand == 0 && remainingRows > 2)
						{
							pathA = 3;
							treeArrCur[2] = 1;
							treeArrCur[3] = -3;
						}
						else
						{
							pathA = 2;
							treeArrCur[2] = -3;
							treeArrCur[3] = 2;
						}
					}
					else if (pathA == 1)
					{
						pathA = 2;
						
						treeArrCur[1] = 1;
						treeArrCur[2] = 4;
						treeArrCur[3] = 2;
					}
				}
				else if (pathB - 1 == pathA)
				{
					if (rand == 0)
					{
						treeArrCur[pathA] = -3;
						treeArrCur[pathA + 1] = 2; 
					}
					else
					{
						treeArrCur[pathA] = 1;
						treeArrCur[pathA + 1] = -3; 

						pathA = pathA + 1;
					}
				}
				else
				{
					if (rand == 0)
					{
						treeArrCur[pathA] = -3;
						treeArrCur[pathA - 1] = 1; 
					}
					else
					{
						treeArrCur[pathA] = 2;
						treeArrCur[pathA - 1] = -3; 
										
						pathA = pathA - 1;
					}
				}

				pathB = -1;
			}

			// else if we are continuing a merged path
			else if (splitInt > 1)
			{
				if (pathA == 0)
				{
					if (Random.Range(0,2) == 0)
					{
						pathA = 0;
						treeArrCur[0] = -3;
						treeArrCur[1] = 3;
					}
					else
					{
						pathA = 1;
						treeArrCur[0] = 1;
						treeArrCur[1] = -2;
					}
				}
				else if (pathA == 3)
				{
					if (Random.Range(0,2) == 0)
					{
						pathA = 3;
						treeArrCur[2] = 3;
						treeArrCur[3] = -3;
					}
					else
					{
						pathA = 2;
						treeArrCur[2] = -1;
						treeArrCur[3] = 2;
					}
				}
				else
				{
					int rand = Random.Range(0,3);

					if (rand == 0)
					{
						treeArrCur[pathA] = -3;
					}
					else if (rand == 1)
					{
						treeArrCur[pathA - 1] = -1;
						treeArrCur[pathA] = 2; 

						pathA -= 1;
					}
					else
					{
						treeArrCur[pathA] = 1;
						treeArrCur[pathA + 1] = -2; 

						pathA = pathA + 1;
					}
				}

				pathB = -1;
			}

			// else we are continuing 2 separate paths
			else
			{
				// if these paths just split and are still adjacent, continue to part them
				if (Mathf.Abs (pathA - pathB) == 1)
				{
					if (pathA == 0)
					{
						pathA = 0;
						pathB = 2;

						treeArrCur[0] = -3;
						treeArrCur[1] = 1;
						treeArrCur[2] = -2;
					}
					else if (pathB == 3)
					{
						pathA = 1;
						pathB = 3;
						
						treeArrCur[1] = -1;
						treeArrCur[2] = 2;
						treeArrCur[3] = -3;
					}
					else 
					{
						int rand = Random.Range(0,3);

						if (rand == 0)
						{
							pathA = 0;
							pathB = 3;
							
							treeArrCur[0] = -1;
							treeArrCur[1] = 2;
							treeArrCur[2] = 1;
							treeArrCur[3] = -2;
						}
						else if (rand == 1)
						{
							pathA = 0;
							pathB = 2;

							treeArrCur[0] = -1;
							treeArrCur[1] = 0;
							treeArrCur[2] = -3;
						}
						else
						{
							pathA = 1;
							pathB = 3;
							
							treeArrCur[1] = -3;
							treeArrCur[2] = 0;
							treeArrCur[3] = -2;
						}
					}
				}

				// else if pathA is on the left and pathB in the 3rd collumn
				else if (pathA == 0 && pathB == 2)
				{
					int rand = Random.Range(0,4);

					if (rand == 0)
					{
						pathA = 0;
						pathB = 2;
						
						treeArrCur[0] = -3;
						treeArrCur[2] = -3;
					}
					else if (rand == 1)
					{
						pathA = 0;
						pathB = 3;

						treeArrCur[0] = -3;
						treeArrCur[2] = 1;
						treeArrCur[3] = -2;
					}
					else
					{
						pathA = 1;
						pathB = 3;
						
						treeArrCur[0] = treeArrCur[2] = 1;
						treeArrCur[1] = treeArrCur[3] = -2;
					}
				}

				// else if pathA is in the 2nd collumn and pathB on the far right
				else if (pathA == 1 && pathB == 3)
				{
					int rand = Random.Range(0,4);
					
					if (rand == 0)
					{
						pathA = 1;
						pathB = 3;
						
						treeArrCur[1] = -3;
						treeArrCur[3] = -3;
					}
					else if (rand == 1)
					{
						pathA = 0;
						pathB = 3;

						treeArrCur[0] = -1;
						treeArrCur[1] = 2;
						treeArrCur[3] = -3;
					}
					else
					{
						pathA = 0;
						pathB = 2;

						treeArrCur[0] = treeArrCur[2] = -1;
						treeArrCur[1] = treeArrCur[3] = 2;
					}
				}

				// else it must be a wide split with A in the far left and b in the far right
				else
				{
					int rand = Random.Range(0,3);

					if (rand == 0)
					{
						pathA = 0;
						pathB = 3;
						
						treeArrCur[0] = -3;
						treeArrCur[3] = -3;
					}
					else if (rand == 1)
					{
						pathA = 1;
						pathB = 3;
						
						treeArrCur[0] = 1;
						treeArrCur[1] = -2;
						treeArrCur[3] = -3;
					}
					else if (rand == 2)
					{
						pathA = 0;
						pathB = 2;
						
						treeArrCur[0] = -3;
						treeArrCur[2] = -1;
						treeArrCur[3] = 2;
					}
				}
			}
		}
	
		// using the generated array, we can now spawn the current row of appropriate obstacles
		SpawnTreeRow(thisRowIndex, sortNum);
	}


	// method to physically spawn a row of tree objects
	void SpawnTreeRow(int thisRowIndex, int sortNum)
	{
		// remove all transforms from the current row using box collider for blocks or mesh collider for triangles
		BoxCollider[] boxChildren = parentRowArr[thisRowIndex].GetComponentsInChildren<BoxCollider>();
		MeshCollider[] meshChildren = parentRowArr[thisRowIndex].GetComponentsInChildren<MeshCollider>();

		for (int i = 0 ; i < boxChildren.Length ; i++)
		{
			Transform childTrans = boxChildren[i].transform;
			childTrans.parent = poolParent;
			childTrans.localPosition = Vector3.zero;

			if (childTrans.name.Equals("TREESblock")) treeBlockPool.Add(childTrans);
			else Debug.Log("Could not assign child named: " + childTrans.name + " to a pool.");
		}

		for (int i = 0 ; i < meshChildren.Length ; i++)
		{
			Transform childTrans = meshChildren[i].transform;
			childTrans.parent = poolParent;
			childTrans.localPosition = Vector3.zero;

			string childName = childTrans.name;

			if (childName.Equals("TREESopenL")) treeOpenLPool.Add(childTrans);
			else if (childName.Equals("TREESopenR")) treeOpenRPool.Add(childTrans);
			else if (childName.Equals("TREESclosedL")) treeClosedLPool.Add(childTrans);
			else if (childName.Equals("TREESclosedR")) treeClosedRPool.Add(childTrans);
			else if (childName.Equals("TREESclosedV")) treeClosedVPool.Add(childTrans);
			else if (childName.Equals("TREESopenV")) treeOpenVPool.Add(childTrans);
			else Debug.Log("Could not assign child named: " + childName + " to a pool.");
		}


		for (int i = 0 ; i < 4 ; i++)
		{
			Transform thisBlock = null;

			if (treeArrCur[i] == 3) 
			{
				thisBlock = treeBlockPool[0];
				treeBlockPool.Remove(thisBlock);
				treeBlockPool.TrimExcess();
			}
			else if (treeArrCur[i] == 1)
			{
				thisBlock = treeClosedLPool[0];
				treeClosedLPool.Remove(thisBlock);
				treeClosedLPool.TrimExcess();
			}
			else if (treeArrCur[i] == 2)
			{
				thisBlock = treeClosedRPool[0];
				treeClosedRPool.Remove(thisBlock);
				treeClosedRPool.TrimExcess();
			}
			else if (treeArrCur[i] == -1)
			{
				thisBlock = treeOpenLPool[0];
				treeOpenLPool.Remove(thisBlock);
				treeOpenLPool.TrimExcess();
			}
			else if (treeArrCur[i] == -2)
			{
				thisBlock = treeOpenRPool[0];
				treeOpenRPool.Remove(thisBlock);
				treeOpenRPool.TrimExcess();
			}
			else if (treeArrCur[i] == 0)
			{
				thisBlock = treeClosedVPool[0];
				treeClosedVPool.Remove(thisBlock);
				treeClosedVPool.TrimExcess();
			}
			else if (treeArrCur[i] == 4)
			{
				thisBlock = treeOpenVPool[0];
				treeOpenVPool.Remove(thisBlock);
				treeOpenVPool.TrimExcess();
			}

			// if the 'if' statements returned a non-null transform for "thisBlock" then position the block and assign its sprite renderer's sorting order appropriately
			if (thisBlock != null)
			{
				thisBlock.parent = parentRowArr[thisRowIndex];
				thisBlock.localPosition = new Vector3( -45 + (treeOffset+2) * i, 0,0);
				thisBlock.GetComponent<SpriteRenderer>().sortingOrder = sortNum;
				//Bebug.Log("Assigned sorting order of: " + thisBlock.GetComponent<SpriteRenderer>().sortingOrder.ToString());
			} 
		}
	}
}
