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
            // variables setting up for PSO
            //int dimension = 2;
            int numOfParticles = 5;
            int maxIter = 5;
            //double exitError = 0.0;
            //double minX = 0.0;
            //double maxX = 0.0;

            List<Node> nodes = new List<Node>();
            readDataFromFile(nodes, "burma.csv");
            /*
            //Console.WriteLine(nodes[0]);
            //Console.WriteLine(nodes[1]);

            //Console.WriteLine(Fitness(nodes[0], nodes[1]));

            List<Node> initSeq = new List<Node>();
            initSeq = initialize(nodes);
            
            //print the original solution
            for (int i = 0; i < initSeq.Count; i++)
                Console.WriteLine(initSeq[i]);

            double distance = totalDistance(initSeq);
            Console.WriteLine(distance);
            
            //int[] swap = swapOperator(nodes);
            //for (int j = 0; j < swap.Length; j++)
                //Console.WriteLine(swap[j]);

            //Solve(5, nodes);
            List<int[]> ss = swapSequence(initSeq);
            //Console.WriteLine(ss.Count);
            //Console.WriteLine(ss[0] + "," + ss[0].GetValue(0) + "," + ss[0].GetValue(1));
            for (int j = 0; j < ss.Count; j++)
            //{
                //Console.WriteLine(j);
                Console.WriteLine(j + "," + ss[j].GetValue(0) + "," + ss[j].GetValue(1));
            //for (int i = 0; i < 2; i++)
            //Console.Write(ss[j].GetValue(0) + ",");
            //}

            //Random random = new Random();
            //Console.WriteLine(random.Next(65535) % 14);

            List<Node> route = swap(initSeq, ss);
            for (int i = 0; i < route.Count; i++)
                Console.WriteLine(route[i]);
            double newDistance = totalDistance(route);
            Console.WriteLine(newDistance);
            */
            Solve(numOfParticles,maxIter,nodes);

            //minus(initSeq, nodes);
            //Console.WriteLine(nodes[13]);
        }

        //read data from csv file
        static List<Node> readDataFromFile(List<Node> nodes,String filename)
        {
            StreamReader sr = new StreamReader(filename);
            String line;
            while ((line = sr.ReadLine())!=null)
            {
                String[] tokens = line.Split(',');
                Node c = new Node();
                c.id = tokens[0];
                c.latitude = tokens[1];
                c.longitude = tokens[2];
                nodes.Add(c);
            }
            sr.Close();

            return nodes;
        }

        //convert the data into geographical latitude and longitude given in radians
        static double[] convertion(Node node)
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
        static double Fitness(Node nodeStart, Node nodeCurrent)
        {
            double[] nodeBegin = convertion(nodeStart);
            double[] nodeEnd = convertion(nodeCurrent);
            const double RRR = 6378.388;
            double q1 = Math.Cos(nodeBegin[1] - nodeEnd[1]);
            double q2 = Math.Cos(nodeBegin[0] - nodeEnd[0]);
            double q3 = Math.Cos(nodeBegin[0] + nodeEnd[0]);
            double dij = (int)(RRR * Math.Acos(0.5*((1.0+q1)*q2 - (1.0-q1)*q3)) + 1.0);

            return dij;

            /*
            //based on the equation [ x * exp( -(x^2 + y^2) ) ] , lowest point x = -sqrt(2), y = 0
            //result of equation when lowest point =  0.42888194248035300000
            //double bestMin = -0.42888194248035300000;
            double bestMin = 0;

            // formula: Volume / (4 * Volume per 15mins)
            double currentMin = x[0] * Math.Exp(-(x[0]*x[0] + x[1]*x[1]));
            return (currentMin - bestMin) * (currentMin - bestMin);
            */
        }

        static double totalDistance(List<Node> seq)
        {
            //declare for calculate distance
            double timeNeeded = 0;

            //since we need link last back to first so only loop until the path before last
            for (int j = 0; j < seq.Count-1; j++)
            {
                //calculate the distance between path then add it up
                timeNeeded += Fitness(seq[j], seq[j+1]);
            }

            //add up the last path into the calculate time
            timeNeeded += Fitness(seq[seq.Count-1], seq[0]);

            return timeNeeded;
        }

        //random assign all nodes available into the the path for initialization
        static List<Node> initialize(List<Node> nodes)
        {
            Random random = new Random();

            List<Node> initSeq = new List<Node>();
            List<Node> availableNodes = new List<Node>(); //dont direct delete the nodes in the main

            for (int i = 0; i < nodes.Count; i++)
                availableNodes.Add(nodes[i]);

            for (int i = availableNodes.Count; i > 0; i--)
            {
                int j = random.Next(0, availableNodes.Count - 1);
                initSeq.Add(availableNodes[j]);
                availableNodes.Remove(availableNodes[j]);
            }
            return initSeq;
        }

        static int[] swapOperator(List<Node> nodes)
        {
            Random random = new Random();

            int[] swap = new int[2];
            
            //ensure that it wont swap the same position
            do
            {
                for (int i = 0; i < swap.Length; i++)
                    swap[i] = random.Next(0, nodes.Count - 1);
            } while (swap[0].Equals(swap[1]));
            
            return swap;
        }

        //we will add in more swap operator during iteration
        static List<int[]> swapSequence(List<Node> nodes)
        {
            //Random random = new Random();

            List<int[]> swapSeq = new List<int[]>();

            //set a random number of swap operator, 10 can be changed
            //int swapOperatorCount = random.Next(1, 10);

            //for (int i = 0; i < swapOperatorCount; i++)
            swapSeq.Add(swapOperator(nodes));
            
            return swapSeq;
        }

        //find the index number of the swap operator in list
        static int findNum(List<Node> nodes, int numToFind)
        {
            int ID = -1;
            for (int i = 0; i < nodes.Count; i++)
            {
                Int32.TryParse(nodes[i].getID(), out ID);
                if (ID == numToFind)
                {
                    return i;
                    //nodes.FindIndex(nodes[i].id);
                    //ID = i;
                    //break;
                } 
            }
            return ID;
        }

        //swapping the position of nodes based on the swap sequence
        static List<Node> swap(List<Node> nodes, List<int[]> swapSeq)
        {
            //create a new list to  prevent overwrite the original solution
            List<Node> foundedRoute = new List<Node>();
            for (int i = 0; i < nodes.Count; i++)
                foundedRoute.Add(nodes[i]);

            //print original solution, debug purpose
            //for (int i = 0; i < foundedRoute.Count; i++)
                //Console.WriteLine(foundedRoute[i]);
            //Console.WriteLine(swapSeq.Count);
            //access the swap operators in the swap sequence
            for (int i = 0; i < swapSeq.Count; i++)
            {
                int source = (int)swapSeq[i].GetValue(0); //the point that needed to be switch
                int target = (int)swapSeq[i].GetValue(1); //the point that need to be switch with the source

                int sourceIndex = findNum(foundedRoute, source); //the index of source in the founded route
                int targetIndex = findNum(foundedRoute, target); //the index of target in the founded route
                //Console.WriteLine(sourceIndex);
                Console.WriteLine(targetIndex);
                swapIndex(nodes, sourceIndex, targetIndex);
                //Node temp = foundedRoute[sourceIndex]; //idk why here will get out of bound when first run
                //foundedRoute[sourceIndex] = foundedRoute[targetIndex];
                //foundedRoute[targetIndex] = temp;
            }
            return foundedRoute;
        }

        static void swapIndex(List<Node> nodes, int sourceIndex, int targetIndex)
        {
            Node temp = nodes[sourceIndex]; 
            nodes[sourceIndex] = nodes[targetIndex];
            nodes[targetIndex] = temp;
        }

        //node class
        public class Node
        {
            public string id;
            public string latitude;
            public string longitude;

            public string getID()
            {
                return this.id;
            }

            public override String ToString()
            {
                return id+" "+latitude+","+longitude+"\n";
            }
        }

        //we need swap from the current to best
        //best = minus + SS, where SS is what we want to return and best = best route particle find and current route particle found
        //should be take from main eh node where for loop but use current or best count is the same
        static List<int[]> minus(List<Node> best, List<Node> current)
        {
            List<int[]> swappingList = new List<int[]>(); // the swapping list we want to return, SS
            // create a copy of current to not direct edit it
            List<Node> currentList = new List<Node>();
            for (int i = 0; i < current.Count; i++)
            {
                currentList.Add(current[i]);
            }
            int index; //store the index we need to find
            int[] swapOperator = new int[2]; // the things that we need to swap
            //check the difference of the order of city in the route
            for (int i = 0; i < current.Count; i++)
            {
                if (best[i].id != currentList[i].id)
                {
                    int ID;
                    Int32.TryParse(best[i].getID(), out ID);
                    //find the index of the city that is different
                    index = findNum(currentList, ID);
                    //Console.WriteLine(index);
                    //swap the index with i
                    swapIndex(currentList, i, index);
                    //record the swap operator
                    swapOperator[0] = i;
                    swapOperator[1] = index;
                    swappingList.Add(swapOperator);
                }
            }
            return swappingList;
        }

        //node need to be the list that read data from file
        static void Solve(int numOfParticle, int maxIter, List<Node> node)
        {
            Random random = new Random();

            Particle[] particle = new Particle[numOfParticle];
            double bestTotalDistance; //best total distance found by particle
            bestTotalDistance = double.MaxValue; //set to to max since we want to find shortest
            List<Node> bestRoute = new List<Node>(); //store the best route found by particle
            //List<int[]> bestSwapSequence = new List<int[]>(); //store the best swap sequence where the swap sequence can get the shortest distance

            //particle initialization
            for (int i = 0; i < particle.Length; i++)
            {
                //random create a solution
                List<Node> initSeq = initialize(node);

                //create particle
                particle[i] = new Particle(initSeq, totalDistance(initSeq), swapOperator(initSeq), swapSequence(initSeq), totalDistance(initSeq));
                //debug purpose Console.WriteLine("Particle " + i + " created");
                //check the best distance and route find by particle and update in global if got
                if (particle[i].totalDistanceNeeded < bestTotalDistance)
                {
                    bestTotalDistance = particle[i].totalDistanceNeeded;
                    //update the best route found in the global var
                    for (int j = 0; j < particle[i].foundedRoute.Count; j++)
                        bestRoute.Add(particle[i].foundedRoute[j]);
                    //update the best swap sequence found in the global var
                    //for (int k = 0; k < particle[i].swapSequence.Count; k++)
                        //bestSwapSequence.Add(particle[i].swapSequence[k]);
                }
                //debug purpose Console.WriteLine("Particle " + i + " bset route");
                //debug purpose for (int j = 0; j < particle[i].foundedRoute.Count; j++)
                //debug purpose Console.WriteLine(particle[i].foundedRoute[j]);
                //debug purpose Console.WriteLine(particle[i].bestTotalDistanceNeeded);
            }
            //end of particle initialization
            //debug purpose Console.WriteLine("Best route");
            //debug purpose for (int i = 0; i < bestRoute.Count; i++)
            //debug purpose Console.WriteLine(bestRoute[i]);
            //debug purpose Console.WriteLine(bestTotalDistance);

            //basic parameter preparation
            //double w = 0.5; //value of inertia
            //double c1 = 1; //local attraction
            //double c2 = 1; //global attraction
            //double r1, r2; //value between 0,1 add in to improve the randomisation of searching
            int eachItr = 0;

            //int[] newSwapOperator;
            List<int[]> newSwapSequence = new List<int[]>();
            double newTimeNeeded;

            //main loop
            while (eachItr < maxIter)
            {
                Console.WriteLine(eachItr);
                for (int i = 0; i < particle.Length; i++) //to access each particle
                {
                    Particle currentParticle = particle[i];

                    
                    int len; //access the number of SO in the swap sequence
                    //new swap sequence
                    //swap sequence = velocity
                    //formula for new velocity: vid=vid⊕α∗(Pid−Xid)⊕β∗(Pgd−Xid)α,β∈[0,1]
                    //new velocity = current velocity + α * (personal best velocity - current velocity) + β ∗ (global best velocity - current velocity)
                    
                    //vid
                    len = currentParticle.swapSequence.Count;
                    //new vid=vid
                    for (int j = 0; j < len; j++)
                        newSwapSequence.Add(currentParticle.swapSequence[j]);
                    //(Pid−Xid)
                    List<int[]> swapSequenceLocal = minus(currentParticle.bestRoute, currentParticle.foundedRoute);
                    //α∗(Pid−Xid)
                    len = swapSequenceLocal.Count * random.Next(0, 2);//return only 0 or 1
                    //new vid=vid⊕α∗(Pid−Xid)
                    for (int j = 0; j < len; j++)
                        newSwapSequence.Add(swapSequenceLocal[j]);
                    //(Pgd−Xid)
                    List<int[]> swapSequenceGlobal = minus(bestRoute, currentParticle.foundedRoute);
                    //β∗(Pgd−Xid)
                    len = swapSequenceGlobal.Count * random.Next(0, 2);
                    // new vid=vid⊕α∗(Pid−Xid)⊕β∗(Pgd−Xid)
                    for (int j = 0; j < len; j++)
                        newSwapSequence.Add(swapSequenceGlobal[j]);

                    //update new swap sequence to the particle
                    currentParticle.swapSequence = newSwapSequence;

                    //new position for the particle, perform new swap sequence
                    currentParticle.foundedRoute = swap(currentParticle.foundedRoute, newSwapSequence);

                    //check is the current iteration find a better route and solution
                    newTimeNeeded = totalDistance(currentParticle.foundedRoute);
                    currentParticle.totalDistanceNeeded = newTimeNeeded;

                    //update for local pupose
                    if (currentParticle.totalDistanceNeeded < currentParticle.bestTotalDistanceNeeded)
                    {
                        currentParticle.bestTotalDistanceNeeded = currentParticle.totalDistanceNeeded;
                        currentParticle.bestRoute.Clear();
                        for (int j = 0; j < currentParticle.foundedRoute.Count; j++)
                            currentParticle.bestRoute.Add(currentParticle.foundedRoute[j]);
                    }

                    //update for global purpose
                    if (currentParticle.totalDistanceNeeded < bestTotalDistance)
                    {
                        bestTotalDistance = currentParticle.totalDistanceNeeded;
                        bestRoute.Clear();
                        for (int j = 0; j < currentParticle.foundedRoute.Count; j++)
                            bestRoute.Add(currentParticle.foundedRoute[j]);
                    }
                }//end point for each particle in an iteration
                eachItr++;
            }
            //Console.WriteLine(particle[0].ToString()); 
            Console.WriteLine(bestTotalDistance);
            //for (int j = 0; j < bestRoute.Count; j++)
                //Console.WriteLine(bestRoute[j]);
        }


        /*
        static double[] Solve(int dimension, int numOfParticle, int maxIter, double exitError, double minX, double maxX)
        {
            Random random = new Random(1);

            Particle[] particle = new Particle[numOfParticle];
            double[] bestGlobalPosition = new double[dimension]; //best position found by the particle
            double bestGlobalError = double.MaxValue; //set the var to contain highest value of double, will be replace later in PSO, bcuz you want smaller value to replace it

            //particle initialization
            for (int i = 0; i < particle.Length; i++)
            {
                double[] randomPosition = new double[dimension];
                for (int j = 0; j < randomPosition.Length; j++)
                {
                    randomPosition[j] = (maxX - minX) * random.NextDouble() + minX; //move the particle to a random position, .NextDouble() return value between 0 to 1
                }

                //get the error value in the start random position
                double fitness = Fitness(randomPosition);
                double[] randomVelocity = new double[dimension];

                for (int j = 0; j < randomVelocity.Length; j++)
                {
                    double lo = minX * 0.1;
                    double hi = maxX * 0.1;
                    randomVelocity[j] = (lo - hi) * random.NextDouble() + lo;
                }
                //create particle
                particle[i] = new Particle(randomPosition, fitness, randomVelocity, randomPosition, fitness);

                //check does the particle has bestGlobalPosition
                if (particle[i].fit < bestGlobalError)
                {
                    bestGlobalError = particle[i].fit;
                    particle[i].position.CopyTo(bestGlobalPosition, 0);
                }
            }
            //end particle initialization

            //basic parameter preparation
            double w = 0.5; //value of inertia
            double c1 = 1; //local attraction
            double c2 = 1; //global attraction
            double r1, r2; //value between 0,1 add in to improve the randomisation of searching
            double probDeath = 0.01; //idk why want add this but ok
            int eachItr = 0;

            double[] newVelocity = new double[dimension];
            double[] newPosition = new double[dimension];
            double newFitness;

            //main loop
            while (eachItr < maxIter)
            {
                for (int i = 0; i < particle.Length; i++) // to access each particle
                {
                    Particle currentParticle = particle[i]; //for clarity

                    //new velocity
                    for (int j = 0; j < currentParticle.velocity.Length; j++)
                    {
                        r1 = random.NextDouble();
                        r2 = random.NextDouble();

                        newVelocity[j] = (w * currentParticle.velocity[j]) + (c1 * r1 * (currentParticle.bestPosition[j] - currentParticle.position[j])) + (c2 * r2 * (bestGlobalPosition[j] - currentParticle.position[j])); //formula to update speed
                    }
                    newVelocity.CopyTo(currentParticle.velocity, 0);//update the new velocity to current particle

                    //new position
                    for (int j = 0; j < currentParticle.position.Length; j++)
                    {
                        newPosition[j] = currentParticle.position[j] + currentParticle.velocity[j]; //formula to update position

                        //check does the new position over the limit set
                        if (newPosition[j] > maxX)
                        {
                            newPosition[j] = maxX;
                        }
                        else if (newPosition[j] < minX)
                        {
                            newPosition[j] = minX;
                        }
                    }
                    newPosition.CopyTo(currentParticle.position, 0);

                    //check the particle for current position error
                    newFitness = Fitness(newPosition);
                    currentParticle.fit = newFitness;

                    //update for local purpose 
                    if (currentParticle.fit < currentParticle.bestFit)
                    {
                        newPosition.CopyTo(currentParticle.bestPosition, 0);
                        currentParticle.bestFit = newFitness;
                    }

                    //update for global communication purpose
                    if (currentParticle.fit < bestGlobalError)
                    {
                        newPosition.CopyTo(bestGlobalPosition, 0);
                        bestGlobalError = newFitness;
                    }

                    //death ? this abit no learn guo eh
                    double die = random.NextDouble();
                    if (die < probDeath)
                    {
                        //if die, create a new position for the particle
                        for (int j = 0; j < currentParticle.position.Length; j++)
                        {
                            currentParticle.position[j] = (maxX - minX) * random.NextDouble() + minX;
                        }
                        //since new position, update error, leave velocity (although wan restart the velocity also can)
                        currentParticle.fit = Fitness(currentParticle.position);
                        currentParticle.position.CopyTo(currentParticle.bestPosition, 0);
                        currentParticle.bestFit = currentParticle.fit;

                        //check if new error can beat the bestGlobalError
                        if (currentParticle.fit  < bestGlobalError)
                        {
                            bestGlobalError = currentParticle.fit;
                            currentParticle.position.CopyTo(bestGlobalPosition, 0);
                        }
                    }
                }//ending point for accesing each particle
                eachItr++;
            }//ending point for while (eachItr < maxIter)

            //show final output after getting result
            Console.WriteLine("\nProcessing Complete");
            Console.WriteLine("\nResult of particles\n");
            for (int i = 0; i < particle.Length; i++)
            {
                Console.WriteLine(particle[i].ToString());
            }
            double[] result = new double[dimension];
            bestGlobalPosition.CopyTo(result, 0);
            return result;
        }//PSO solve

        */

        //particle class
        public class Particle
        {
            //var
            public List<Node> oriSolution; //the original generated route
            public List<Node> foundedRoute; //will be changed in each iteration because new swap operator add in
            public double totalDistanceNeeded; //total distance based on the current route
            public int[] currentSwapOperator; //swap operator
            public List<int[]> swapSequence; //swap sequence contain many swap operator, will increase one or many SO per interation
            public List<int[]> bestSwapSequence; //personal best swap sequence, store the swap sequence when the total distance is the lowest
            public List<Node> bestRoute; //the best route found
            public double bestTotalDistanceNeeded; //best distance found
            
            //construtor
            public Particle(List<Node> nodes, double totalDistanceNeeded, int[] currentSwapOperator, List<int[]> swapSequence, double bestTotalDistanceNeeded)
            {
                this.oriSolution = new List<Node>();
                for (int i = 0; i < nodes.Count; i++)
                    this.oriSolution.Add(nodes[i]);
                this.foundedRoute = new List<Node>();
                for (int i = 0; i < nodes.Count; i++)
                    this.foundedRoute.Add(nodes[i]);
                this.totalDistanceNeeded = totalDistanceNeeded;
                this.currentSwapOperator = new int[currentSwapOperator.Length];
                currentSwapOperator.CopyTo(this.currentSwapOperator, 0);
                this.swapSequence = new List<int[]>();
                for (int i = 0; i < swapSequence.Count; i++)
                    this.swapSequence.Add(swapSequence[i]);
                this.bestSwapSequence = new List<int[]>();
                for (int i = 0; i < swapSequence.Count; i++)
                    this.bestSwapSequence.Add(swapSequence[i]);
                this.bestRoute = new List<Node>();
                for (int i = 0; i < nodes.Count; i++)
                    this.bestRoute.Add(nodes[i]);
                this.bestTotalDistanceNeeded = bestTotalDistanceNeeded;
            }

            public override string ToString()
            {
                string s = "";
                s += "==========================\n";
                s += "Original Solution: ";
                for (int i = 0; i < this.oriSolution.Count; ++i)
                    s += this.oriSolution[i].ToString() + " ";
                s += "\n";
                s += "Founded Route: ";
                for (int i = 0; i < this.foundedRoute.Count; ++i)
                    s += this.foundedRoute[i].ToString() + " ";
                s += "\n";
                s += "Error = " + this.totalDistanceNeeded.ToString("F4") + "\n";
                s += "Velocity: ";
                for (int i = 0; i < this.currentSwapOperator.Length; ++i)
                    s += this.currentSwapOperator[i].ToString()+ " ";
                s += "\n";
                s += "Best Position: ";
                for (int i = 0; i < this.swapSequence.Count; ++i)
                    s += this.swapSequence[i].GetValue(0) + "," + this.swapSequence[i].GetValue(1) + "\n";
                s += "\n";
                s += "Best Error = " + this.bestTotalDistanceNeeded.ToString("F4") + "\n";
                s += "==========================\n";
                return s;
            }
        }
        /*
        public class Particle
        {
            //var
            //public double[] position;
            public List<Node> solution;
            public double fit;
            public List<int[]> swapSequence;
            public Node bestPosition;
            public double bestFit;

            //constructor
            public Particle(List<Node> nodes, double fit, List<int[]> swap, Node bestPos, double bestFit)
            {
                //this.position = new double[pos.Length];
                //pos.CopyTo(this.position, 0);
                this.solution = new List<Node>();
                for (int i = 0; i < nodes.Count; i++)
                    this.solution.Add(nodes[i]);
                this.fit = fit;
                this.swapSequence = new List<int[]>();
                for(int i = 0; i < swap.Count; i++)
                    this.swapSequence.Add(swap[i]);
                //this.bestPosition = new double[bestPos.Length];
                //bestPos.CopyTo(this.bestPosition, 0);
                this.bestPosition = new Node();
                this.bestPosition = bestPos;
                this.bestFit = bestFit;
            }

            public override string ToString()
            {
                string s = "";
                s += "==========================\n";
                s += "Solution: ";
                for (int i = 0; i < this.solution.Count; ++i)
                    s += this.solution[i].ToString() + " ";
                s += "\n";
                s += "Error = " + this.fit.ToString("F4") + "\n";
                s += "Velocity: ";
                for (int i = 0; i < this.swapSequence.Count; ++i)
                    s += this.swapSequence[i].GetValue(0) + "," + this.swapSequence[i].GetValue(1) + "\n";
                s += "\n";
                s += "Best Position: ";
                //for (int i = 0; i < this.bestPosition.Count; ++i)
                    //s += this.bestPosition[i].ToString() + " ";
                s += "\n";
                s += "Best Error = " + this.bestFit.ToString("F4") + "\n";
                s += "==========================\n";
                return s;
            }
        }
        */
    }
}
