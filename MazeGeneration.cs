using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MazeGeneration : MonoBehaviour
{
    //objects that will be instantiated
    public List<GameObject> segments;

    public GameObject redCube;
    /// <summary>
    /// the most recent segments placed
    /// used to determine which blocks can be placed next
    /// 
    /// positions in curSegs of maze width 4:
    /// ***********        
    /// * 4 5 6 7 *
    /// * 0 1 2 3 *
    /// ***********   
    /// </summary>

    public List<GameObject> curSegs;

    //positions of the last segments generated
    public List<Vector3> curPos;

    

    float genLimit = 500;
    public Transform playerPos;

    //counts number of rows generated.
    //every width row is fully connected
    int rowNum = 1;

    //position player starts from
    public Vector3 startPosition;
    public List<Vector3> solutionPath;

    //player y position;
    float playerY;

    ///<summary>
    ///distance between positions where segments are placed
    ///</summary>
    public float distBetweenSeg = 18;

    //size of maze being made
    public int width;
    public int height;

    // whether end of maze has been reached
    private bool exitFound = false;

    //list of segments that have each path
    public List<GameObject> leftPaths;
    public List<GameObject> rightPaths;
    public List<GameObject> frontPaths;
    public List<GameObject> backPaths;

    //booleans to check whether a segment has a path in a given direction
    bool hasLeft(GameObject obj)
    {
        return leftPaths.Contains(obj);
    }

    bool noLeft(GameObject obj)
    {
        return !leftPaths.Contains(obj);
    }

    bool hasRight(GameObject obj)
    {
        return rightPaths.Contains(obj);
    }

    bool noRight(GameObject obj)
    {
        return !rightPaths.Contains(obj);
    }

    bool hasFront(GameObject obj)
    {
        return frontPaths.Contains(obj);
    }

    bool noFront(GameObject obj)
    {
        return !frontPaths.Contains(obj);
    }

    bool hasBack(GameObject obj)
    {
        return backPaths.Contains(obj);
    }

    bool noBack(GameObject obj)
    {
        return !backPaths.Contains(obj);
    }


    public bool extendMap(Transform player)
    {
        return genLimit - player.position.z < 100;
    }

    public bool isOnSolutionPath(Vector3 pos)
    {
        bool tF = false;

        foreach (Vector3 position in solutionPath)
        {
            if (position.Equals(pos))
            {
                tF = true;
            }
        }

        return tF;
    }


    // true is front and false is right
    public bool goFrontOrRight()
    {
        int num = UnityEngine.Random.Range(0, 2);
        bool boolin = true;
        if (!exitFound && rowNum < height)
        {
            if (num == 1)
            {
                usableBlocks.RemoveAll(noFront);
                
            }
            else
            {
                usableBlocks.RemoveAll(noRight);
                boolin = false;
            }

            
        }else
        if (!exitFound)
        {
            if (num == 1)
            {
                usableBlocks.RemoveAll(noFront);
                usableBlocks.RemoveAll(hasRight);
                
            }
            else
            {
                usableBlocks.RemoveAll(noRight);
                usableBlocks.RemoveAll(hasFront);
                boolin = false;
            }
        }
        else
        {
            usableBlocks.RemoveAll(noRight);
            boolin = false;
        }

        return boolin;
        
    }



   
    
    /// <summary>
    ///all possible blocks that may currently be used to generate the next part of the maze
    ///this list is modified to only include segments acceptable for the current situation
    /// </summary>
    public List<GameObject> usableBlocks;

    // position where the current maze segment is being generated
    public Vector3 position = new Vector3(0, 0, 0);

    void Start()
    {
        StartCoroutine(spawnStuff());
        

    }


    IEnumerator spawnStuff()
    {

        

        while (true)
        {
            yield return null;

            //list of segments that may be placed
            //elements that won't line up with previous segments are removed
            usableBlocks = new List<GameObject>(segments);

            

            if (position.Equals(startPosition))
            {

                solutionPath = new List<Vector3>()
                {
                    position
                };
            }
            else
            if (rowNum % width == 0 && position.x.Equals(0))
            {


                solutionPath = new List<Vector3>()
                {
                    new Vector3(0, 0, position.z),
                };


            }

            int posX = (int) (position.x / distBetweenSeg);

            if (exitFound) { usableBlocks.RemoveAll(hasFront); }
            

            //updates usable segments for positions in the very first row of the maze
            if (position.z.Equals(0))
            {
                

                
                    if (posX.Equals(0)) //position 1
                    {

                        
                        usableBlocks.RemoveAll(noFront);
                        usableBlocks.RemoveAll(noRight);
                        usableBlocks.RemoveAll(hasBack);
                        usableBlocks.RemoveAll(hasLeft);

                    Debug.Log("checkpoint 1: " + usableBlocks.Count);
                    

                    }
                    else if (!posX.Equals(width - 1)) // middle positions
                    {

                        
                        usableBlocks.RemoveAll(noFront);
                        usableBlocks.RemoveAll(hasBack);




                        if (hasRight(curSegs[posX - 1]))
                        {
                            usableBlocks.RemoveAll(noLeft);
                            if (isOnSolutionPath(curPos[posX - 1]))
                            {
                                solutionPath.Add(position);
                                goFrontOrRight();
                            Instantiate(redCube, position + Vector3.up * 5f, redCube.transform.rotation);
                            Debug.Log("Solution path first row");
                                
                            }
                        }
                        else
                        {
                            usableBlocks.RemoveAll(hasLeft);
                        }


                    Debug.Log("checkpoint 2: " + usableBlocks.Count);

                }
                    
                    else // last position
                    {

                        usableBlocks.RemoveAll(hasBack);
                        usableBlocks.RemoveAll(noFront);
                        usableBlocks.RemoveAll(hasRight);

                        if (hasRight(curSegs[width - 2]))
                        {
                            usableBlocks.RemoveAll(noLeft);
                            
                        }
                        else
                        {
                            usableBlocks.RemoveAll(hasLeft);
                        }

                    Debug.Log("checkpoint 3: " + usableBlocks.Count);
                }

                
                



            }
            //updates usable segments depending on the block to the left and behind
            else
            {
                

                
                    if (curSegs.Count.Equals(width)) // first position of row
                    {


                        usableBlocks.RemoveAll(hasLeft);

                        if (hasFront(curSegs[0]))
                        {

                            usableBlocks.RemoveAll(noBack);
                            if (isOnSolutionPath(curPos[0]))
                            {
                                solutionPath.Add(position);
                            Instantiate(redCube, position + Vector3.up * 5f, redCube.transform.rotation);
                            bool wentFront = goFrontOrRight();
                                exitFound = wentFront && rowNum == height;
                            Debug.Log("We're on the last row: " + (rowNum == height));
                            Debug.Log("We went with going front: " + wentFront);
                        }
                        }
                        else
                        {
                            usableBlocks.RemoveAll(hasBack);
                        }

                        if (rowNum % width == 0 && rowNum < height)
                        {
                            usableBlocks.RemoveAll(noRight);

                        }

                    Debug.Log("checkpoint 0: " + usableBlocks.Count);
                }
                    else if (!curSegs.Count.Equals(width * 2 - 1)) //middle positions
                    {
                        int count = curSegs.Count;

                        bool isSolution = false;

                    
                        if (hasRight(curSegs[count - 1]))
                        {
                            usableBlocks.RemoveAll(noLeft);
                            if (isOnSolutionPath(curPos[count - 1]) && !hasFront(curSegs[count - 1]))
                            {
                                solutionPath.Add(position);
                            Instantiate(redCube, position + Vector3.up * 5f, redCube.transform.rotation);
                            bool wentFront = goFrontOrRight();
                            exitFound = wentFront && rowNum == height;
                            Debug.Log("We're on the last row: " + (rowNum == height));
                            Debug.Log("We went with going front: " + wentFront);

                            isSolution = true;
                            }
                            else if(isOnSolutionPath(curPos[count - 1]))
                            {
                            solutionPath.Add(position);
                            Instantiate(redCube, position + Vector3.up * 5f, redCube.transform.rotation);
                            usableBlocks.RemoveAll(hasFront);
                                isSolution = true;
                            }
                        }
                        else
                        {
                            usableBlocks.RemoveAll(hasLeft);
                        }


                        if (hasFront(curSegs[count - width]))
                        {
                            usableBlocks.RemoveAll(noBack);
                            if (isOnSolutionPath(curPos[count - width]) && !isSolution)
                            {
                                solutionPath.Add(position);
                            Instantiate(redCube, position + Vector3.up * 5f, redCube.transform.rotation);
                            bool wentFront = goFrontOrRight();
                            exitFound = wentFront && rowNum == height;
                            Debug.Log("We're on the last row: " + (rowNum == height));
                            Debug.Log("We went with going front: " + wentFront);
                            }

                        }
                        else
                        {
                            usableBlocks.RemoveAll(hasBack);
                        }

                        if (rowNum % width == 0 && rowNum < height)
                        {
                            usableBlocks.RemoveAll(noRight);
                        }
                    Debug.Log("checkpoint " + posX + ": " + usableBlocks.Count);
                }
                    
                    else // last position in row
                    {

                        usableBlocks.RemoveAll(hasRight);
                        bool goingForward = false;

                        int posToLeft = width * 2 - 2;

                        
                        if (hasRight(curSegs[posToLeft]))
                        {
                        
                            usableBlocks.RemoveAll(noLeft);
                            if (isOnSolutionPath(curPos[posToLeft]) && !exitFound)
                            {
                               
                                solutionPath.Add(position);
                            Instantiate(redCube, position + Vector3.up * 5f, redCube.transform.rotation);
                            if (noFront(curSegs[posToLeft]))
                                {
                                    usableBlocks.RemoveAll(noFront);
                                    exitFound = true;
                                }
                                else
                                {
                                    usableBlocks.RemoveAll(hasFront);
                                }

                                goingForward = true;
                            }
                        }
                        else
                        {
                            usableBlocks.RemoveAll(hasLeft);
                        }

                        int count = curSegs.Count;

                        if (hasFront(curSegs[count - width]))
                        {
                            usableBlocks.RemoveAll(noBack);
                            if (isOnSolutionPath(curPos[count - width]) && !goingForward)
                            {
                                solutionPath.Add(position);
                            Instantiate(redCube, position + Vector3.up * 5f, redCube.transform.rotation);
                            usableBlocks.RemoveAll(noFront);
                                exitFound = true;
                            }
                        }
                        else
                        {
                            usableBlocks.RemoveAll(hasBack);
                        }

                    Debug.Log("checkpoint 3: " + usableBlocks.Count);
                    
                }

                 
                

            }

            
            //generates random segment from the remaining list and places it
            int rand = UnityEngine.Random.Range(0, usableBlocks.Count);
            Instantiate(usableBlocks[rand], position, usableBlocks[rand].transform.rotation);

            Debug.Log("Exit found: " + exitFound);

           

            //adds current segment to the small list of recents
            curSegs.Add(usableBlocks[rand]);
            curPos.Add(position);


            

            //list of recents is updated so that the newest four are moved to the front
            if (curSegs.Count.Equals(width * 2))
            {
                curSegs.RemoveRange(0, width);
                curPos.RemoveRange(0, width);
            }

            //increment row number becaue solution start resets every fifth row
            if (position.x.Equals(distBetweenSeg * (width - 1)))
            {
                rowNum++;

            }

            

            //moves on to the next position in left to right then forward order
            if (position.x < distBetweenSeg * (width - 1))
            {
                position.x += distBetweenSeg;
            }
            else
            {
                position.x = 0;
                position.z += distBetweenSeg;
            }

            
            if(rowNum > height)
            {
                yield break;
            }
        }
    }


}
