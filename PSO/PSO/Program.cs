using System;
using System.ComponentModel;
using System.Data.Common;
using System.Security.Cryptography;

namespace PSO
{
    class ParticleProgram
    {
        public static void Main(String[] args)
        {
            int numOfParticles = 100;
            int maxIter = 20000;

            List<Node> nodes = new List<Node>();

            //readDataFromFile(nodes, "burma.csv");
            //readDataFromFile(nodes, "ulysses16.csv");

            //readDataFromFile(nodes, "gr96.csv");
            //readDataFromFile(nodes, "gr137.csv");
            //readDataFromFile(nodes, "gr202.csv");
            //readDataFromFile(nodes, "gr229.csv");
            //readDataFromFile(nodes, "gr431.csv");
            //readDataFromFile(nodes, "ali535.csv");
            readDataFromFile(nodes, "gr666.csv");
            //readDataFromFile(nodes, "ulysses22.csv");

            //for (int i = 0; i < nodes.Count; i++)
            //Console.WriteLine(nodes[i]);

            for (int z = 0; z < 5; z++)
            {
                solve(numOfParticles, maxIter, nodes);
            }


            /*
            initSeq.Add(nodes[0]);
            initSeq.Add(nodes[9]);
            initSeq.Add(nodes[8]);
            initSeq.Add(nodes[10]);
            initSeq.Add(nodes[7]);
            initSeq.Add(nodes[12]);
            initSeq.Add(nodes[6]);
            initSeq.Add(nodes[11]);
            initSeq.Add(nodes[5]);
            initSeq.Add(nodes[4]);
            initSeq.Add(nodes[3]);
            initSeq.Add(nodes[2]);
            initSeq.Add(nodes[13]);
            initSeq.Add(nodes[1]);
            
            for (int i = 0; i < initSeq.Count; i++)
                Console.WriteLine(initSeq[i]);

            Console.WriteLine(totalDistance(initSeq));
            
            List<Node> initSeq = new List<Node>();
            initSeq.Add(nodes[6]);
            initSeq.Add(nodes[12]);
            initSeq.Add(nodes[7]);
            initSeq.Add(nodes[10]);
            initSeq.Add(nodes[8]);
            initSeq.Add(nodes[9]);
            initSeq.Add(nodes[0]);
            initSeq.Add(nodes[1]);
            initSeq.Add(nodes[13]);
            initSeq.Add(nodes[2]);
            initSeq.Add(nodes[3]);
            initSeq.Add(nodes[4]);
            initSeq.Add(nodes[5]);
            initSeq.Add(nodes[11]);

            Console.WriteLine(totalDistance(initSeq));

            */
        }

        //nodes id is 1 - 14
        //read data from csv file
        static List<Node> readDataFromFile(List<Node> nodes, String filename)
        {
            StreamReader sr = new StreamReader(filename);
            String line;
            while ((line = sr.ReadLine())!=null)
            {
                int ID = -1;
                String[] tokens = line.Split(',');
                Node c = new Node();
                Int32.TryParse(tokens[0], out ID);
                c.id = ID;
                c.latitude = tokens[1];
                c.longitude = tokens[2];
                nodes.Add(c);
            }
            sr.Close();

            return nodes;
        }

        static SO swapOperator(List<Node> nodes)
        {
            Random random = new Random();

            int source = -1;
            int target = -1;

            do
            {
                source = random.Next(1, nodes.Count + 1);
                target = random.Next(1, nodes.Count + 1);
            } while (source == target);

            return new SO(source, target);
        }

        static List<SO> swapSequence(List<Node> nodes)
        {
            List<SO> swapSeq = new List<SO>();

            for (int i =0; i<nodes.Count; i++)
                swapSeq.Add(swapOperator(nodes));

            return swapSeq;
        }



        //node class
        public class Node
        {
            public int id;
            public string latitude;
            public string longitude;

            public int getID()
            {
                return this.id;
            }

            public override String ToString()
            {
                return id+" "+latitude+","+longitude+"\n";
            }
        }

        public class SO
        {
            public int source;
            public int target;

            public SO(int source, int target)
            {
                this.source = source;
                this.target = target;
            }
        }

        static void solve(int numOfParticle, int maxItr, List<Node> nodes)
        {
            Random random = new Random();

            Particle[] particle = new Particle[numOfParticle];
            double globalBestTotalDistance;
            globalBestTotalDistance = double.MaxValue;
            List<Node> globalBestRoute = new List<Node>();
            List<SO> globalRandomSwap = swapSequence(nodes);

            //particle initialize
            for (int i = 0; i < numOfParticle; i++)
            {

                //create particle
                particle[i] = new Particle(nodes);

                //check to update global
                if (particle[i].bestTotalDistance < globalBestTotalDistance)
                {
                    globalBestTotalDistance = particle[i].bestTotalDistance;
                    for (int j = 0; j < particle[i].foundedRoute.Count; j++)
                        globalBestRoute.Add(particle[i].foundedRoute[j]);
                }
            }
            //end of particle initialize

            int currentItr = 0;
            List<SO> newSwapSequence = new List<SO>();
            Particle currentParticle;
            List<Node> route;
            int w = 1;
            int len;
            double totalDistance;

            //main loop
            while (currentItr < maxItr)
            {
                for (int i = 0; i < particle.Length; i++)
                {
                    newSwapSequence.Clear();
                    currentParticle = particle[i];

                    len = -1; //store number of SO wanted

                    //β∗(Pgd−Xid), move toward global best
                    currentParticle.minus(globalBestRoute);

                    //new velocity = swapSequence
                    //vi = random get from the swapSequence that is produced in the global
                    //if wan * W shud be multiply add globalRandomSwap
                    //w to prevent set the last element in the list so that it can at least swap
                    len = random.Next(0, globalRandomSwap.Count - w);
                    newSwapSequence.AddRange(globalRandomSwap.GetRange(len, random.Next(w, globalRandomSwap.Count - len)));

                    //α∗(Pid−Xid)
                    currentParticle.minusOffset(newSwapSequence);

                    //check is the current iteration find a better route and solution
                    totalDistance = currentParticle.totalDistance();

                    //update for global purpose
                    if (totalDistance < globalBestTotalDistance)
                    {
                        globalBestTotalDistance = totalDistance;
                        globalBestRoute.Clear();
                        for (int j = 0; j < currentParticle.foundedRoute.Count; j++)
                            globalBestRoute.Add(currentParticle.foundedRoute[j]);
                    }
                }//end point for each particle in an iteration
                currentItr++;
            }
            Console.WriteLine(globalBestTotalDistance);
            for (int j = 0; j < globalBestRoute.Count; j++)
                Console.Write(globalBestRoute[j].id + " ");
            Console.WriteLine();
        }

        //particle class
        public class Particle
        {
            //var
            public List<Node> foundedRoute { get; }
            public List<Node> offsetRoute;
            public double bestTotalDistance;

            public Particle(List<Node> nodes)
            {
                this.offsetRoute = initialize(nodes);
                this.foundedRoute = new List<Node>();
                for (int i = 0; i < offsetRoute.Count; i++)
                    this.foundedRoute.Add(offsetRoute[i]);
                this.bestTotalDistance = totalDistance();
            }

            //random assign all nodes available into the the path for initialization
            private List<Node> initialize(List<Node> nodes)
            {
                Random random = new Random();

                List<Node> initSeq = new List<Node>();
                List<Node> availableNodes = new List<Node>(); //dont direct delete the nodes in the main

                for (int i = 0; i < nodes.Count; i++)
                    availableNodes.Add(nodes[i]);

                for (int i = availableNodes.Count; i > 0; i--)
                {
                    int j = random.Next(0, availableNodes.Count);
                    initSeq.Add(availableNodes[j]);
                    availableNodes.Remove(availableNodes[j]);
                }
                return initSeq;
            }

            //convert the data into geographical latitude and longitude given in radians
            private double[] convertion(Node node)
            {
                const double PI = 3.141592;
                double deg = Math.Truncate(Convert.ToDouble(node.latitude));
                double min = Convert.ToDouble(node.latitude) - deg;
                double latitude = PI * (deg + 5.0 * min / 3.0) / 180.0;
                deg = Math.Truncate(Convert.ToDouble(node.longitude));
                min = Convert.ToDouble(node.longitude) - deg;
                double longitude = PI * (deg + 5.0 * min / 3.0) / 180.0;

                return new double[] { latitude, longitude };
            }

            //error, lower error better fitness
            //calculate distance between 2 points, shorter better
            //return in KM
            private double fitness(Node nodeStart, Node nodeCurrent)
            {
                double[] nodeBegin = convertion(nodeStart);
                double[] nodeEnd = convertion(nodeCurrent);
                const double RRR = 6378.388;
                double q1 = Math.Cos(nodeBegin[1] - nodeEnd[1]);
                double q2 = Math.Cos(nodeBegin[0] - nodeEnd[0]);
                double q3 = Math.Cos(nodeBegin[0] + nodeEnd[0]);
                double dij = (int)(RRR * Math.Acos(0.5*((1.0+q1)*q2 - (1.0-q1)*q3)) + 1.0);

                return dij;
            }

            public double totalDistance()
            {
                //declare for calculate distance
                double distance = 0;

                //since we need link last back to first so only loop until the path before last
                for (int j = 0; j < foundedRoute.Count-1; j++)
                {
                    //calculate the distance between path then add it up
                    distance += fitness(foundedRoute[j], foundedRoute[j+1]);
                }

                //add up the last path into the calculate time
                distance += fitness(foundedRoute[foundedRoute.Count-1], foundedRoute[0]);

                //update for local pupose
                if (distance < bestTotalDistance)
                {
                    bestTotalDistance = distance;
                }

                return distance;
            }

            //find the index number of the swap operator in list
            private int findNum(List<Node> nodes, int numToFind)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i].id == numToFind)
                    {
                        return i;
                    }
                }
                return -1;
            }

            //swapping the position of nodes based on the swap sequence
            public void swap(List<Node> route, List<SO> swapSeq)
            {

                for (int i = 0; i < swapSeq.Count; i++)
                {
                    int source = swapSeq[i].source;
                    int target = swapSeq[i].target;
                    int sourceIndex = findNum(route, source); //the index of source in the founded route
                    int targetIndex = findNum(route, target); //the index of target in the founded route

                    swapIndex(route, sourceIndex, targetIndex);
                }
            }

            private void swapIndex(List<Node> nodes, int sourceIndex, int targetIndex)
            {
                Node temp = nodes[sourceIndex];
                nodes[sourceIndex] = nodes[targetIndex];
                nodes[targetIndex] = temp;
            }

            //we need swap from the current to best
            //best = minus + SS, where SS is what we want to return and best = best route particle find and current route particle found
            //should be take from main eh node where for loop but use current or best count is the same
            public void minus(List<Node> best)
            {
                // the swapping list we want to return, SS
                List<SO> swappingList = new List<SO>();
                // create a copy of current to not direct edit it
                List<Node> currentList = new List<Node>();
                for (int i = 0; i < foundedRoute.Count; i++)
                    currentList.Add(foundedRoute[i]);

                int index;  //store the index we need to find
                SO swapOperator; // the SO that we need to swap


                #region "check the difference of the order of city in the route"
                for (int i = 0; i < currentList.Count; i++)
                {
                    if (best[i].id != currentList[i].id)
                    {
                        //find the index of the city that is different
                        index = findNum(currentList, best[i].id);
                        swapIndex(currentList, i, index);
                        //create the swap operator object
                        swapOperator = new SO(currentList[i].id, currentList[index].id);
                        //add into swapSequence list
                        swappingList.Add(swapOperator);
                    }
                }
                int len = (int) (swappingList.Count * new Random().NextDouble());
                #endregion
                swap(foundedRoute, swappingList.GetRange(0, len));
            }

            public void minusOffset(List<SO> swapSeq)
            {
                //Vid to change offsetRoute
                swap(offsetRoute, swapSeq);
                //α∗(Pid−Xid), move toward personal best aka offset
                minus(offsetRoute);
            }
        }     
    }
}

