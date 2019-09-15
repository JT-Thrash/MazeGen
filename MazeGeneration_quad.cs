using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Diagnostics;

public class MazeGeneration_quad : MonoBehaviour
{
    //objects that will be instantiated
    public List<GameObject> segments;

    public GameObject container;
    public string name;
    public int startLevelNum;
    public string folder;
    private int genNum = 0; //number of mazes made so far;
    public int numToGenerate;

    public bool runScript = true;
    public bool autoRun;

    private int quadrantsFinished = 0;
    private float oneToTwo;
    private float oneToThree;
    private float twoToFour;

    public bool findPaths;
    GetDir pathFinder;

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



    //list of segments that have each path
    public List<GameObject> leftPaths = new List<GameObject>(8);
    public List<GameObject> rightPaths = new List<GameObject>(8);
    public List<GameObject> frontPaths = new List<GameObject>(8);
    public List<GameObject> backPaths = new List<GameObject>(8);
    public HashSet<GameObject> noLeft;
    HashSet<GameObject> noRight;
    HashSet<GameObject> noFront;
    HashSet<GameObject> noBack;
    HashSet<GameObject> hasLeft;
    HashSet<GameObject> hasRight;
    HashSet<GameObject> hasFront;
    HashSet<GameObject> hasBack;





    public bool isOnSolutionPath(Vector3 pos)
    {
        bool tF = false;

        for (int i = 0; i < solutionPath.Count; i++)
        {
            tF = solutionPath[i].Equals(pos);
            if (tF) { break; }
        }



        return tF;
    }


    // true is front and false is right
    public void goFrontOrRight()
    {
        int num = UnityEngine.Random.Range(0, 2);


        if (num == 1)
        {
            usableBlocks.ExceptWith(noFront);

        }
        else
        {
            usableBlocks.ExceptWith(noRight);

        }







    }





    /// <summary>
    ///all possible blocks that may currently be used to generate the next part of the maze
    ///this list is modified to only include segments acceptable for the current situation
    /// </summary>
    public HashSet<GameObject> usableBlocks = new HashSet<GameObject>();



    // position where the current maze segment is being generated
    public Vector3 position = new Vector3(0, 0, 0);

    Stopwatch timeForOneMaze = new Stopwatch();

    void Start()
    {
        if (findPaths)
        {
            pathFinder = GetComponent<GetDir>();
            leftPaths = pathFinder.leftPaths;
            rightPaths = pathFinder.rightPaths;
            frontPaths = pathFinder.frontPaths;
            backPaths = pathFinder.backPaths;
            segments = pathFinder.segments;
        }
        else
        {
            StartCoroutine(spawnStuff());
        }

        startPosition = position;
        container.transform.position = position;


        noLeft = new HashSet<GameObject>(segments);
        noLeft.ExceptWith(leftPaths);
        hasLeft = new HashSet<GameObject>(leftPaths);



        noRight = new HashSet<GameObject>(segments);
        noRight.ExceptWith(rightPaths);
        hasRight = new HashSet<GameObject>(rightPaths);

        noFront = new HashSet<GameObject>(segments);
        noFront.ExceptWith(frontPaths);
        hasFront = new HashSet<GameObject>(frontPaths);

        noBack = new HashSet<GameObject>(segments);
        noBack.ExceptWith(backPaths);
        hasBack = new HashSet<GameObject>(backPaths);

        width = width / 2;
        height = height / 2;
        oneToTwo = height / 2 * distBetweenSeg;
        oneToThree = width / 2 * distBetweenSeg;



    }


    IEnumerator spawnStuff()
    {

        Stopwatch timer = new Stopwatch();
        timer.Start();
        timeForOneMaze.Start();

        bool exitFound = false;
        bool finalExitFound = false;
        while (true)
        {
            yield return null;

            //list of segments that may be placed
            //elements that won't line up with previous segments are removed
            usableBlocks.UnionWith(segments);


            bool isSolution = false;

            if (rowNum == height && quadrantsFinished == 3)
            {
                exitFound = finalExitFound;
            }

            if (exitFound)
            {
                usableBlocks.ExceptWith(frontPaths);
            }

            if (position.Equals(startPosition))
            {

                solutionPath = new List<Vector3>(1000)
                {
                    position
                };
            }


            int posX = (int)(position.x / distBetweenSeg);

            if (posX > width)
            {
                posX -= width;
            }

            int num = 0;


            if (posX == 0 || posX == width) { num = 1; posX = 0; } else if (posX == (width - 1) || posX == (width * 2)) { num = 2; posX = width - 1; }

            //Debug.Log("num result is " + num + " at position" + position.x);

            //updates usable segments for positions in the very first row of the maze
            if (position.z == 0 || position.z == height * distBetweenSeg)
            {


                switch (num)
                { //position 1

                    case 1:


                        usableBlocks.ExceptWith(noFront);
                        usableBlocks.ExceptWith(noRight);
                        if (!(quadrantsFinished >= 2 && (position.x == oneToThree || position.x == twoToFour)))
                        {
                            usableBlocks.ExceptWith(backPaths);

                        }
                        else
                        {
                            usableBlocks.ExceptWith(noBack);
                        }
                        usableBlocks.ExceptWith(leftPaths);

                        isSolution = true;



                        break;

                    case 2:

                        if (!(quadrantsFinished >= 2 && (position.x == oneToThree || position.x == twoToFour)))
                        {
                            usableBlocks.ExceptWith(backPaths);

                        }
                        else
                        {
                            usableBlocks.ExceptWith(noBack);
                        }
                        usableBlocks.ExceptWith(noFront);
                        usableBlocks.ExceptWith(rightPaths);



                        if (hasRight.Contains(curSegs[width - 2]))
                        {

                            usableBlocks.ExceptWith(noLeft);
                            if (curPos[posX - 1].y == 1)
                            {
                                isSolution = true;
                            }

                        }
                        else
                        {
                            usableBlocks.ExceptWith(leftPaths);
                        }


                        break;

                    default:

                        usableBlocks.ExceptWith(noFront);
                        usableBlocks.ExceptWith(backPaths);




                        if (hasRight.Contains(curSegs[posX - 1]))
                        {


                            usableBlocks.ExceptWith(noLeft);



                            if ((curPos[posX - 1]).y == 1)
                            {
                                isSolution = true;
                                goFrontOrRight();


                            }
                        }
                        else
                        {
                            usableBlocks.ExceptWith(leftPaths);

                        }

                        if (quadrantsFinished >= 2 && ((position.x < oneToThree) || (position.x > oneToThree && position.x != twoToFour)))
                        {

                            usableBlocks.UnionWith(rightPaths);

                            usableBlocks.ExceptWith(noRight);
                            if (hasRight.Contains(curSegs[posX - 1])) { usableBlocks.ExceptWith(noLeft); } else { usableBlocks.ExceptWith(hasLeft); }


                            usableBlocks.ExceptWith(hasBack);
                        }
                        else if (quadrantsFinished >= 2)
                        {
                            usableBlocks.UnionWith(backPaths);
                            usableBlocks.ExceptWith(noBack);
                            usableBlocks.ExceptWith(noLeft);
                        }


                        break;

                }




            }
            //updates usable segments depending on the block to the left and behind
            else
            {

                switch (num)
                {

                    case 1: // the first position in the row //

                        usableBlocks.ExceptWith(leftPaths);



                        if (hasFront.Contains(curSegs[0]))
                        {

                            usableBlocks.ExceptWith(noBack);
                            if ((curPos[0]).y == 1)
                            {
                                isSolution = true;


                                if (!exitFound) { goFrontOrRight(); } else { usableBlocks.ExceptWith(noRight); }
                                //exitFound = wentFront && rowNum == height;

                            }
                        }
                        else
                        {
                            usableBlocks.ExceptWith(backPaths);
                        }

                        if (rowNum % width == 0 && rowNum < height)
                        {
                            usableBlocks.ExceptWith(noRight);

                        }

                        if (rowNum == height && !isSolution)
                        {
                            usableBlocks.ExceptWith(frontPaths);
                        }


                        if (quadrantsFinished == 1 && position.z == oneToTwo)
                        {
                            usableBlocks.UnionWith(segments);
                            usableBlocks.ExceptWith(noLeft);
                            usableBlocks.ExceptWith(noFront);
                            usableBlocks.ExceptWith(noRight);
                            if (hasFront.Contains(curSegs[0])) { usableBlocks.ExceptWith(noBack); } else { usableBlocks.ExceptWith(hasBack); }

                        }

                        break;

                    case 2: // last position in the row //



                        int blockToLeft = curSegs.Count - 1;
                        int blockBehind = curSegs.Count - width;

                        bool comingFromLeft = hasRight.Contains(curSegs[blockToLeft]);
                        bool comingFromBehind = hasFront.Contains(curSegs[blockBehind]);
                        bool leftGoesForward = hasFront.Contains(curSegs[blockToLeft]);
                        bool leftSolution = curPos[blockToLeft].y == 1;
                        bool backSolution = curPos[blockBehind].y == 1;


                        usableBlocks.ExceptWith(rightPaths);

                        bool backToFront = true;




                        if (comingFromLeft && leftSolution && !leftGoesForward)
                        {

                            usableBlocks.ExceptWith(noLeft);
                            isSolution = true;

                            if (!exitFound && quadrantsFinished != 2) { usableBlocks.ExceptWith(noFront); }


                            backToFront = false;
                            //exitFound = true;

                        }
                        else if (comingFromLeft && leftSolution)
                        {
                            usableBlocks.ExceptWith(noLeft);
                            usableBlocks.ExceptWith(frontPaths);

                            isSolution = true;
                            backToFront = false;
                            //exitFound = true;

                        }
                        else if (!comingFromLeft)
                        {
                            usableBlocks.ExceptWith(leftPaths);
                        }
                        else if (comingFromLeft)
                        {
                            usableBlocks.ExceptWith(noLeft);
                        }




                        int count = curSegs.Count;

                        if (comingFromBehind && backSolution && backToFront && !exitFound)
                        {
                            usableBlocks.ExceptWith(noBack);
                            usableBlocks.ExceptWith(noFront);

                            isSolution = true;

                            //exitFound = true;

                        }
                        else if (!comingFromBehind)
                        {
                            usableBlocks.ExceptWith(backPaths);
                        }
                        else if (comingFromBehind && backSolution)
                        {
                            usableBlocks.ExceptWith(noBack);
                            isSolution = true;
                        }
                        else if (comingFromBehind)
                        {
                            usableBlocks.ExceptWith(noBack);
                        }

                        if (exitFound && !comingFromBehind && !comingFromLeft)
                        {
                            usableBlocks.UnionWith(frontPaths);
                        }


                        if (quadrantsFinished == 0 && position.z == oneToTwo)
                        {
                            usableBlocks.UnionWith(rightPaths);
                            if (comingFromBehind) { usableBlocks.ExceptWith(noBack); } else { usableBlocks.ExceptWith(hasBack); }
                            if (comingFromLeft) { usableBlocks.ExceptWith(noLeft); } else { usableBlocks.ExceptWith(hasLeft); }
                            usableBlocks.ExceptWith(noRight);
                        }


                        break;

                    default: // the middle positions //

                        count = curSegs.Count;

                        blockToLeft = curSegs.Count - 1;
                        blockBehind = curSegs.Count - width;

                        // whether spaces to left and behind this one lead to solution
                        leftSolution = curPos[blockToLeft].y == 1;
                        backSolution = curPos[blockBehind].y == 1;

                        comingFromLeft = hasRight.Contains(curSegs[blockToLeft]);
                        bool comingFromBack = hasFront.Contains(curSegs[blockBehind]);



                        //check the space to the left

                        if (comingFromLeft && leftSolution && !hasFront.Contains(curSegs[blockToLeft]))
                        {

                            //connect to the left space, add this space to the solution set, connect right and/or forward

                            usableBlocks.ExceptWith(noLeft);

                            isSolution = true;


                            if (!exitFound) { goFrontOrRight(); }

                            //exitFound = wentFront && rowNum == height;



                        }
                        else if (comingFromLeft && leftSolution) // go left to right
                        {
                            //connect to the left, add this space to the solution set, don't go forward

                            usableBlocks.ExceptWith(noLeft);
                            isSolution = true;

                            usableBlocks.ExceptWith(frontPaths);

                        }
                        else if (comingFromLeft)
                        {
                            usableBlocks.ExceptWith(noLeft);
                        }
                        else
                        {
                            usableBlocks.ExceptWith(leftPaths);
                        }

                        //check the space behind this one

                        if (comingFromBack && backSolution && !leftSolution)
                        {

                            //connect to the space behind, add this space to the solution set, connect right and/or forward

                            usableBlocks.ExceptWith(noBack);
                            isSolution = true;

                            if (!exitFound) { goFrontOrRight(); }
                            //exitFound = wentFront && rowNum == height;


                        }
                        else if (comingFromBack && backSolution)
                        {
                            usableBlocks.ExceptWith(noBack); //connect backwards

                            isSolution = true;
                        }
                        else if (comingFromBack)
                        {
                            usableBlocks.ExceptWith(noBack);
                        }

                        else if (!comingFromBack)
                        {
                            usableBlocks.ExceptWith(backPaths); //disconnect from behind
                        }


                        if (rowNum % width == 0 && rowNum < height)
                        {
                            usableBlocks.ExceptWith(noRight);
                        }




                        if (exitFound || (!isSolution && rowNum == height))
                        {
                            usableBlocks.ExceptWith(frontPaths);
                        }

                        break;



                }






            }





            List<GameObject> usableList = new List<GameObject>(usableBlocks);
            //generates random segment from the remaining list and places it
            int rand = UnityEngine.Random.Range(0, usableBlocks.Count);
            var newSeg = Instantiate(usableList[rand], position, usableList[rand].transform.rotation);



            newSeg.transform.parent = container.transform;


            if (hasFront.Contains(usableList[rand]) && rowNum == height && isSolution)
            {
                exitFound = true;

                if (quadrantsFinished == 0)
                {
                    oneToThree = position.x;
                }
                else if (quadrantsFinished == 1)
                {
                    twoToFour = position.x;
                }
                else if (quadrantsFinished > 1)
                {
                    finalExitFound = true;
                }
            }




            //adds current segment to the small list of recents
            curSegs.Add(usableList[rand]);


            //add current position to small list of recents
            //change y value to 1 to show that this position is on the solution path
            //I did this to eliminate the repeated usage of a for loop when checking for this

            if (isSolution)
            {
                curPos.Add(new Vector3(position.x, 1, position.z));


            }
            else
            {
                curPos.Add(position);

            }





            //list of recents is updated so that the newest four are moved to the front
            if (curSegs.Count == (width * 2))
            {
                curSegs.RemoveRange(0, width);
                curPos.RemoveRange(0, width);
            }

            //increment row number becaue solution start resets every fifth row
            if (posX == width - 1)
            {
                rowNum++;

            }



            //moves on to the next position in left to right then forward order
            if (quadrantsFinished == 0 || quadrantsFinished == 2)
            {
                if (position.x < distBetweenSeg * (width - 1))
                {
                    position.x += distBetweenSeg;
                }
                else
                {
                    position.x = 0;
                    position.z += distBetweenSeg;
                }
            }
            else if (quadrantsFinished == 1 || quadrantsFinished == 3)
            {
                if (position.x < distBetweenSeg * (width * 2 - 1))
                {
                    position.x += distBetweenSeg;
                }
                else
                {
                    position.x = width * distBetweenSeg;
                    position.z += distBetweenSeg;
                }
            }


            timer.Stop();
            //UnityEngine.Debug.Log(timer.ElapsedMilliseconds);

            if (rowNum > height)
            {
                quadrantsFinished++;

                rowNum = 1;

                exitFound = false;
                if (quadrantsFinished == 1)
                {
                    position = new Vector3(distBetweenSeg * width, 0, 0);
                }
                else if (quadrantsFinished == 2)
                {
                    UnityEngine.Debug.Log("passed height");
                    position = new Vector3(0, 0, height * distBetweenSeg);

                }
                else if (quadrantsFinished == 3)
                {

                    position = new Vector3(distBetweenSeg * width, 0, height * distBetweenSeg);
                    UnityEngine.Debug.Log(position);

                }


                curSegs = new List<GameObject>();
                curPos = new List<Vector3>();
            }

            if (quadrantsFinished == 4)
            {
                timeForOneMaze.Stop();
                UnityEngine.Object prefab = PrefabUtility.SaveAsPrefabAsset(container, "Assets/Resources/" + folder + "/" + name + "_" + (genNum + startLevelNum) + ".prefab");
                genNum++;
                if (genNum == numToGenerate)
                {
                    yield break;
                }
                runScript = autoRun;


                yield return new WaitUntil(() => runScript == true);
                Destroy(container);
                position = startPosition;
                rowNum = 1;
                exitFound = false;
                curSegs = new List<GameObject>();

                curPos = new List<Vector3>();
                quadrantsFinished = 0;
                container = Instantiate(new GameObject("MazeContainer"), new Vector3(0, 0, 0), transform.rotation);


                //UnityEngine.Debug.Log(timeForOneMaze.Elapsed);
            }
        }
    }






}
