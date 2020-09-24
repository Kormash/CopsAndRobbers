using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CopsAndRobbers
{
    class Program
    {

        int mapSizeX;
        int mapSizeY;


        static void Main(string[] args)
        {
            Program program = new Program();
            program.SetUp();
            Console.ReadKey();
        }

        void SetUp()
        {
            //Sets up the simulation.
            mapSizeX = 100;
            mapSizeY = 25;
            int numCops = 5;
            int numRobbers = 5;
            int numCivilians = 10;
            Random rnd = new Random();

            List<Person> citizens = new List<Person>();

            for (int i = 0; i < numCops; i++)
            {
                Cop cop = new Cop(rnd.Next(mapSizeX), rnd.Next(mapSizeY));
                citizens.Add(cop);
            }
            for (int i = 0; i < numRobbers; i++)
            {
                Robber robber = new Robber(rnd.Next(mapSizeX), rnd.Next(mapSizeY));
                citizens.Add(robber);
            }
            for (int i = 0; i < numCivilians; i++)
            {
                Civilian civilian = new Civilian(rnd.Next(mapSizeX), rnd.Next(mapSizeY));
                citizens.Add(civilian);
            }

            Simulate(citizens);

        }

        void Simulate(List<Person> citizens)
        {
            Random rnd = new Random();
            while (true)
            {
                //Move all People
                foreach (Person person in citizens)
                {
                    person.Move(rnd.Next(-1, 2), rnd.Next(-1, 2));
                    if (person.xPos < 0)
                    {
                        person.xPos = mapSizeX;
                    }
                    if (person.xPos > mapSizeX)
                    {
                        person.xPos = 0;
                    }
                    if (person.yPos < 0)
                    {
                        person.yPos = mapSizeY;
                    }
                    if (person.yPos > mapSizeY)
                    {
                        person.yPos = 0;
                    }
                }

                //Draw the city.
                List<string> city = new List<string>();
                int arrests = 0;
                int robberies = 0;

                for (int y = 0; y < mapSizeY; y++)
                {
                    string row = "";
                    for (int x = 0; x < mapSizeX; x++)
                    {
                        int check = CheckSquare(x, y, citizens);
                        switch (check)
                        {
                            case 0:
                                //Nobody in Square
                                row += " ";
                                break;
                            case 1:
                                //Cop
                                row += "P";
                                break;
                            case 2:
                                //Robber
                                row += "T";
                                break;
                            case 3:
                                //Civilian
                                row += "M";
                                break;
                            case 4:
                                //Cop and Robber
                                row += "X";
                                Arrest(x, y, citizens);
                                arrests++;
                                break;
                            case 5:
                                //Robber and Civilian
                                Robb(x, y, citizens);
                                robberies++;
                                row += "X";
                                break;
                            case 6:
                                //Cop, Robber and Civilian
                                row += "X";
                                Arrest(x, y, citizens);
                                arrests++;
                                break;
                        }
                    }
                    city.Add(row);
                }

                foreach (string row in city)
                {
                    Console.WriteLine(row);
                }

                bool wait = false;
                if (arrests + robberies > 0)
                {
                    //Something interesting happened, wait longer
                    wait = true;
                }

                //Print the interesting Events.
                while (arrests > 0)
                {
                    Console.WriteLine("Cop caught a Robber!");
                    arrests--;
                }
                while (robberies > 0)
                {
                    Console.WriteLine("Robber stole from a Civilian!");
                    robberies--;
                }

                if (wait)
                {
                    Thread.Sleep(2000);
                }
                else
                {
                    Thread.Sleep(200);
                }
                Console.Clear();
            }
        }

        int CheckSquare(int x, int y, List<Person> citizens)
        {
            //Checks what people are on a given square.
            //Returns 0 = Empty, 1 = Cop, 2 = Robber, 3 = Civ, 4 = Cop + Rob, 5 = Rob + Civ, 6 = Cop + Rob + Civ
            int result = 0;
            bool cop = false;
            bool robber = false;
            bool civilian = false;

            foreach (Person person in citizens)
            {
                if (person.xPos == x && person.yPos == y)
                {
                    string type = person.GetType().Name;
                    switch (type)
                    {
                        case "Cop":
                            cop = true;
                            break;
                        case "Robber":
                            robber = true;
                            break;
                        case "Civilian":
                            civilian = true;
                            break;
                    }
                }
            }
            if (cop && robber && civilian)
            {
                result = 6;
            }
            else if (robber && civilian)
            {
                result = 5;
            }
            else if (cop && robber)
            {
                result = 4;
            }
            else if (civilian)
            {
                result = 3;
            }
            else if (robber)
            {
                result = 2;
            }
            else if (cop)
            {
                result = 1;
            }

            return result;
        }

        void Arrest(int x, int y, List<Person> citizens)
        {
            Person robber = new Person();
            Person cop = new Person();

            bool robberFound = false;
            bool copFound = false;

            //Find the Cop and Robber in question.
            foreach (Person person in citizens)
            {
                if (person.xPos == x && person.yPos == y)
                {
                    string type = person.GetType().Name;
                    switch (type)
                    {
                        case "Robber":
                            if (!robberFound)
                            {
                                robber = person;
                                robberFound = true;
                            }
                            break;
                        case "Cop":
                            if (!copFound)
                            {
                                cop = person;
                                copFound = true;
                            }
                            break;
                    }
                    if (robberFound && copFound)
                    {
                        break;
                    }
                }

                //Remove all items from Robber and give to Cop.
                if (robber.Inventory.Any())
                {
                    foreach (string item in robber.Inventory)
                    {
                        cop.Inventory.Add(item);
                        robber.Inventory.Remove(item);
                    }
                }
            }
        }

        void Robb(int x, int y, List<Person> citizens)
        {
            Person robber = new Person();
            Person civilian = new Person();

            bool robberFound = false;
            bool civilianFound = false;

            //Find the Robber and Civilian in question.
            foreach (Person person in citizens)
            {
                if (person.xPos == x && person.yPos == y)
                {
                    string type = person.GetType().Name;
                    switch (type)
                    {
                        case "Robber":
                            if (!robberFound)
                            {
                                robber = person;
                                robberFound = true;
                            }
                            break;
                        case "Civilian":
                            if (!civilianFound)
                            {
                                civilian = person;
                                civilianFound = true;
                            }
                            break;
                    }
                    if (robberFound && civilianFound)
                    {
                        break;
                    }
                }

                String item;

                //Remove one item from Civilian and give to Robber.
                if (civilian.Inventory.Any())
                {
                    item = civilian.Inventory.ElementAt(0);
                    civilian.Inventory.RemoveAt(0);
                    robber.Inventory.Add(item);
                }
            }
        }
        class Person
        {
            public int xPos;
            public int yPos;
            public List<string> Inventory = new List<string>();

            public void Move(int xMove, int yMove)
            {
                xPos += xMove;
                yPos += yMove;
            }

        }

        class Cop : Person
        {
            public Cop(int x, int y)
            {
                xPos = x;
                yPos = y;
            }
        }

        class Robber : Person
        {
            public Robber(int x, int y)
            {
                xPos = x;
                yPos = y;
            }
        }

        class Civilian : Person
        {
            public Civilian(int x, int y)
            {
                xPos = x;
                yPos = y;
                Inventory.Add("Keys");
                Inventory.Add("Phone");
                Inventory.Add("Money");
                Inventory.Add("Watch");
            }
        }
    }
}