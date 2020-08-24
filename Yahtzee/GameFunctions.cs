using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yahtzee;

namespace Yahtzee
{
    public class GameFunctions
    {
        private int[] dice;
        private int[] finalDice;
        private TextFormatting format;
        private Player p1, p2;

        //init
        public GameFunctions()
        {
            //creates a text formmatting instance
            format = new TextFormatting();

            //launch init function for character selection etc.
            GameStart();
        }

        public void Playing(Random rtd)
        {
            Console.WriteLine("Press any key to start the game");
            Console.WriteLine();

            format.PrintScoreCard(p1, p2);

            bool gamePlaying = true;

            while (gamePlaying == true)
            {
                Console.ReadLine();
                SelectKeep(rtd, p1);

                format.PrintScoreCard(p1, p2);
                Console.ReadLine();
                Console.Clear();

                if (p2.IsBot)
                {
                    BotTurn(rtd, p2);
                }
                else
                {
                    SelectKeep(rtd, p2);
                }

                format.PrintScoreCard(p1, p2);

                if(IsItOver())
                {
                    gamePlaying = false;
                }
            }
            Console.WriteLine(WhoWon());
            Console.ReadLine();
        }

        //Starts the game and creates play/bot type Player instances
        private void GameStart()
        {
            //returns true once initialized
            bool Initialized = false;

            //loops until properly initialized
            while (!Initialized)
            {
                //clears the console for clean presentation
                Console.Clear();


                Console.WriteLine("Welcome to Yahtzee!!!");
                Console.Write("Are you playing with one player or two? ");

                // gets player input of # of players and converts to Lowercase and trims
                string numPlayers = Console.ReadLine().ToLower().Trim();


                //switch to construct player or cpu Player class instances
                switch (numPlayers)
                {
                    case "1":
                    case "one":
                        //false because real player
                        p1 = new Player(false);

                        //true because it is bot
                        p2 = new Player(true);

                        //initiallized
                        Initialized = true;
                        break;

                    case "2":
                    case "two":
                        //false because real player
                        Console.WriteLine("Player 1 enter your name");
                        p1 = new Player(false);
                        Console.WriteLine("Player 2 enter your name");
                        p2 = new Player(false);

                        //initiallized
                        Initialized = true;

                        break;

                    //if input isn't suitable Intiallized == false so it loops
                    default:
                        Console.WriteLine("Sorry this input is incorrect, please select one or two players");
                        Console.ReadKey();

                        Initialized = false;
                        break;
                }

                //end of Init loop
            }
        }

        private void SelectKeep(Random rtd, Player player)
        {
            //count of how many rolls/rerolls have happened
            int rollNum = 1;

            //Initiallize fresh dice
            dice = new int[6];
            finalDice = new int[6];


            //looping for each number allowed re-rolls
            while (rollNum < 3)
            {

                //clear console for clean presentation
                Console.Clear();

                //rolls un-saved dice
                Roll(rtd);

                //displays selected players name
                Console.WriteLine(player.Name + "  it is your turn!");

                //displays saved dice
                Console.WriteLine("Your saved dice are: " + format.PrintDice(finalDice));

                //displays the dice they can choose to keep and requests to select which they will keep
                Console.WriteLine("you rolled: " + format.PrintDice(dice));

                //how many rerolls user has
                Console.WriteLine("You have:" + (3 - rollNum) + " more roll(s)");

                Console.WriteLine("Choose the dice you would like to keep, seperated by spaces");
                Console.Write("The remaining dice will be rerolled: ");

                //splits a string into an array of strings seperated by spaces
                string[] inputString = Console.ReadLine().Split(' ');

                //creates new array to load selected int values
                int[] selectedDice = new int[6];

                //for each string it tries to parse it to an integer
                foreach (string str in inputString)
                {
                    try
                    {
                        //tries to parse str array value to Int
                        int tempNum = Int32.Parse(str);

                        //if the value is an int and between 1 and 6 it adds it to the selected array
                        //selected temp num is subtracted by one because dice array is 0-5
                        if (tempNum > 0 && tempNum < 7) { selectedDice[tempNum - 1]++; }
                        //else error message
                        else { Console.WriteLine("please enter a number 1-6"); }
                    }
                    //if anything fails to try it will set temp to -1 and give error message
                    catch (FormatException) { Console.WriteLine("please enter a number 1-6"); }
                }

                //if dice were selected it adds them to the final dice
                if (HowManyDice(selectedDice) > 0) finalDice = AddDice(finalDice, CheckCap(dice, selectedDice));

                //if you've selected all your dice the loop is set to end
                if (HowManyDice(finalDice) == 5) { rollNum = 3; }


                //if  this is the final round it will roll the last of the unselected and add it to the final array
                else if (rollNum == 2)
                {
                    Roll(rtd);
                    finalDice = AddDice(finalDice, dice);
                    Console.WriteLine("you rolled: " + format.PrintDice(dice));
                    Console.WriteLine("your final set is: " + format.PrintDice(finalDice));

                }

                //adds to the roll counter
                rollNum++;
            }

            //moves over to scoring with the player that is currently play
            Scoring(player);

            //outside of select keep loop
        }


        private void Scoring(Player player)
        {
            //gets the players selected scoring catagory
            int selectedScoreCatagory = ScoringChoice(player);

            int temp = 0;
            
            //the order of score type are 1-6, 3 of a kind, 4 of a kind..
            //full house, small straight, large straight, chance, yahtzee

            //switches to selected scoring catagory and scores accordingly
            switch (selectedScoreCatagory)
            {
                //none
                case -1:
                    Console.WriteLine("you have chosen to score nothing for this round.");
                    break;

                //rolls 1-6
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    //scoring 1-6 is a sum of all the dice

                    //score is set in selected position
                    //dice 1-6 is 0-5 and lines up with said values in the score catagory array
                    //the number of dice in the selected catagory is multiplied by the (selectedcatagory + 1)
                    //+1 to get accuracte sum with values 1-6 being held in 0-5 
                    player.Score[selectedScoreCatagory] = finalDice[selectedScoreCatagory] * (selectedScoreCatagory + 1);
                    break;
                    
               //three of a kind
                case 6:
                    //loops 5-0 if there is 3 of a dice it scores it
                    for (int j = 5; j > -1; j--)
                    {
                        if (finalDice[j] >= 3)
                        {
                            //score is dice value added by one to fix off by 1, and multiplied by 3
                            player.Score[6] = (j + 1) * 3;
                        }
                    }
                    break;

                //four of a kind
                case 7:        
                    //loops 5-0 if there is 4 of a dice it scores it in the appropriate catagory
                    for (int j = 5; j > -1; j--)
                    {
                        if (finalDice[j] >= 4)
                        {
                            //score is dice value added by one to fix off by 1, and multiplied by 3
                            player.Score[7] = (j + 1) * 4;
                        }
                    }
                    break;

                //full house
                case 8:
                    //for each position in the finaldice array
                    foreach (int i in finalDice)
                    {
                        //checks if there are 3 of a kind
                        if (i == 3)
                        {
                            //sifts through again to see if there are any pairs
                            for (int j = 0; j < 6; j++)
                            {
                                //if there is a pair, and that pair isn't the 3 of a kind
                                if (finalDice[j] > 1 && j != i)
                                {
                                    player.Score[8] = 25;
                                }
                            }
                        }
                    }
                    break;

                //small straight
                case 9:
                    //sifts through final dice set
                    foreach (int i in finalDice)
                    {
                        //cycles through and adds to the counter
                        //if theres a value in that dice position it adds one to the counter
                        if (i > 0) { temp++; }
                        //if we haven't found a straight already and the dice position is empty counter is reset
                        else if (temp != 4) { temp = 0; }
                    }

                    //if it's a small straight it scores 30
                    if (temp >= 4)
                    {
                        player.Score[9] = 30;
                    }
                    break;

                //large straight
                case 10:
                    //sifts through final dice set
                    foreach (int i in finalDice)
                    {
                        //cycles through and adds to the counter
                        //if theres a value in that dice position it adds one to the counter
                        if (i > 0) { temp++; }
                    }
                    //if it's a large straight it scores 40
                    if (temp == 5)
                    {
                        player.Score[10] = 40;
                    }
                    break;

                //Chance
                case 11:

                    for (int i = 0; i < 6; i++)
                    {
                        //for each value in the array adds all dice in that position multiplied by the positions value 1-6 (i+1)
                        temp = temp + (finalDice[i] * (i + 1));
                    }

                    player.Score[11] = temp;

                    break;

                //yahtzee
                case 12:
                    //loops through each int in the final dice array if there is 5 of one dice value temp is set to 50
                    foreach (int i in finalDice)
                    {
                        if (i == 5)
                        {
                            temp = 50;
                        }
                    }
                    //Score is set to temp if no yahtzee temp will be 0
                    player.Score[12] = temp;

                    break;

            }
            //if a score is selected and it fails to add a value
            if (selectedScoreCatagory != 13 && player.Score[selectedScoreCatagory] == -1)
            {
                player.Score[selectedScoreCatagory] = 0;
            }
            Console.Clear();
        }

        private int ScoringChoice(Player player)
        {

            //-1 indicates a choice hasn't been selected
            int returnPosition = -1;

            //loops until a proper value is selected
            while (returnPosition == -1)
            {
                //console clear so loops looks alright
                Console.Clear();


                //prints current scores and lets player know proper input types by seeing them
                Console.WriteLine("The current scores are..");

                Console.WriteLine();

                format.PrintScoreCard(p1, p2);

                Console.WriteLine();

                //presents final rolls and asks what section they want to score under
                Console.WriteLine("Your final rolls are: " + format.PrintDice(finalDice));
                Console.WriteLine("What section would you like to score under? You can only select each score type once, or none");

                //gets user input converts to lowercase and trims it
                string userInput = Console.ReadLine().Trim().ToLower();

                //if they just hit enter, input count will == 0 and it will score none
                if (userInput.Count() == 0) { Console.WriteLine("You have selected none..."); userInput = "none"; }

                //main switch that grabs acceptable scoring type input
                //returns the position of the selected score type
                switch (userInput)
                {
                    case "1":
                    case "one": { returnPosition = 0; break; }

                    case "2":
                    case "two": { returnPosition = 1; break; }

                    case "3":
                    case "three": { returnPosition = 2; break; }

                    case "4":
                    case "four": { returnPosition = 3; break; }

                    case "5":
                    case "five": { returnPosition = 4; break; }

                    case "6":
                    case "six": { returnPosition = 5; break; }

                    case "3 of a kind":
                    case "three of a kind": { returnPosition = 6; break; }

                    case "4 of a kind":
                    case "four of a kind": { returnPosition = 7; break; }

                    case "full house": { returnPosition = 8; break; }

                    case "small straight": { returnPosition = 9; break; }

                    case "large straight": { returnPosition = 10; break; }

                    case "chance": { returnPosition = 11; break; }

                    case "yahtzee": { returnPosition = 12; break; }

                    case "none":
                        {
                            returnPosition = 13;
                            break;
                        }

                }
            }

            //after the loop is done and a proper value is selected the scoring array position is returned
            return returnPosition;
        }

        //Random comes from the main loop to insure accuracy
        private void Roll(Random rtd)
        {
            //variable deciding how many dice you will roll
            //rolled dice are 5 - however many you've saved
            int toRoll = 5 - HowManyDice(finalDice);

            //resets the rolling dice for new rolls
            dice = new int[6];

            //loops for however many dice are needed to be rolled
            for (int j = 0; j < (toRoll); j++)
            {
                //rolls values 0-5
                //dice[0] == rolls of 1 so this lines up nicely
                int roll = rtd.Next(0, 6);
                //adds 1 to that dice value in the array
                dice[roll] += 1;
            }
        }

        private int[] AddDice(int[] d1, int[] d2)
        {
            //dice array to return the combined values
            int[] returnDice = new int[6];
            
            //cycles through all the positions in the dice array and adds the two arguments values to that return array
            for (int diceValue = 0; diceValue < 6; diceValue++)
            {
                returnDice[diceValue] = d1[diceValue] + d2[diceValue];
            }
            
            return returnDice;
        }

        //returns a dice array that doesn't exceed the values of th main array
        public int[] CheckCap(int[] mainDice, int[] checkDice)
        {
            int[] returnDice = new int[6];

            //for each position in the array it checks whether the dice being checked don't exceed limits of the main dice
            for (int diceValue = 0; diceValue < 6; diceValue++)
            {
                //if the dice being checked and the main dice set have values exceeding 0...
                //the returnDice is given the check dice value as long as it does not exceed the maximum of the main dice
                if (checkDice[diceValue] > 0 && mainDice[diceValue] > 0)
                {
                    //if those dice values exceed main it sets the values to that of the main
                    if (checkDice[diceValue] > mainDice[diceValue]) { returnDice[diceValue] = mainDice[diceValue]; }
                    //else it sets the returnDice value to that of the dice being checked
                    else { returnDice[diceValue] = checkDice[diceValue]; }
                }
            }

            return returnDice;
        }

        //easy counter for how many dice values are within the array
        private int HowManyDice(int[] d)
        {
            int returnInt = 0;
            foreach (int i in d)
            {
                returnInt += i;
            }
            return returnInt;
        }

        //"AI" rolls a few times and takes highest score among them
        private void BotTurn(Random rtd, Player player)
        {
            int highestScorePosition = 0;
            int highestScore = 0;
            int testingScore = 0;
            int counter = 0;
            finalDice = new int[6];

            //loops it  to grab the highest values out of a couple roll sets
            while (counter < 4)
            {
                //rolls a fresh roll
                Roll(rtd);

                //loops through all possible score values
                for (int i = 0; i < 13; i++)
                {
                    // temp used for score calculations
                    int temp = 0;

                    //if that score position is open
                    if (player.Score[i] == -1)
                    {


                        //the order of score types is 1-6, 3 of a kind, 4 of a kind..
                        //full house, small straight, large straight, chance, yahtzee
                        switch (i)
                        {
                            //one through six
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                                //checks values 1-6 and saves highest and position
                                testingScore = (i + 1) * dice[i];
                                break;

                            //three of a kind
                            case 6:
                                for (int j = 5; j > -1; j--)
                                {
                                    if (dice[j] >= 3)
                                    {
                                        //score is dice value added by one to fix off by 1, and multiplied by 3
                                        testingScore = (j + 1) * 3;
                                    }
                                }
                                break;
    
                            //four of a kind
                            case 7:
                                //checks all values  in the array from the 6 down to 1
                                for (int j = 5; j > -1; j--)
                                {
                                    if (dice[j] >= 4)
                                    {
                                        //score is dice value added by one to fix off by 1, and multiplied by 3
                                        testingScore = (j + 1) * 4;
                                    }
                                }
                                break;

                            //full house
                            case 8:
                                //searches all dice positions for 3 of a kind
                                foreach (int j in dice)
                                {
                                    if (j == 3)
                                    {
                                        //if 3 of a kind is found it searches for 2 of a kind
                                        for (int k = 0; k < 5; k++)
                                        {
                                            if (k != j && k == 2)
                                            {
                                                //if two of a kind is found and it isn't the same as the 3 of a kind score is set to 25
                                                testingScore = 25;
                                            }
                                        }
                                    }
                                }
                                break;
                            //small straight
                            case 9:
                                //loops through all dice values
                                foreach (int j in dice)
                                {
                                    //if there's a value in that position temp has 1 added to it
                                    if (j > 0)
                                    {
                                        temp++;
                                    }
                                    //else if small straight hasn't been found and a position is empty temp is reset to 0
                                    else if (temp != 4) { temp = 0; }
                                }

                                //if a small straight was found test score is set to 30
                                if (temp >= 4)
                                {
                                    testingScore = 30;
                                }
                                break;

                                //large straight
                            case 10:
                                //loops through all dice values
                                foreach (int j in dice)
                                {
                                    //if there's a value in that position temp has 1 added to it
                                    if (j > 0)
                                    {
                                        temp++;
                                    }
                                    //else if large straight hasn't been found and a position is empty temp is reset to 0
                                    else if (temp != 4) { temp = 0; }
                                }

                                //if a large straight was found test score is set to 40
                                if (temp == 4)
                                {
                                    testingScore = 40;
                                }
                                break;


                            //Chance
                            case 11:

                                //counts filled score slots 1-6 so chance is used later in the game
                                int chanceCount = 0;

                                //loops through scores 1-6
                                for (int k = 0; k < 6; k++)
                                {
                                    //if a score position is unused in this range, chance counter goes up by one
                                    if (player.Score[k] == -1)
                                    {
                                        chanceCount++;
                                    }
                                }

                                //if the bot has less than 4 unscored positions it will calculate chance score
                                if (chanceCount < 4)
                                {
                                    //for each value in dice it sums the dice values
                                    for (int j = 0; j < 6; j++)
                                    {
                                        temp = temp + dice[j] * (j + 1);
                                    }

                                    //score to test is set
                                    testingScore = temp;
                                }

                                break;

                            //Yahtzee
                            case 12:
                                //foreach dice value if there are 5 of that value then temp is set to 50
                                foreach (int j in dice)
                                {
                                    if (j == 5)
                                    {
                                        testingScore = 50;
                                    }
                                }
                                break;
                        }//end of switch

                        //if the temporary score is higher than the high score and that score value is open..
                        //the position and score of highest score is saved  
                        if (testingScore > highestScore && player.Score[i] == -1)
                        {
                            highestScore = testingScore;
                            highestScorePosition = i;
                        }

                    }

                }//end of for loop
                if(testingScore == 0)
                { counter = 0; }
                else
                {
                    counter++;
                }
            }//end of while loop

            //if the highest score is above 0 the score is set to that poition
            //this statement is so the bot will score in no catagories if by some chance their highest is 0
            if (highestScore > 0)
            {
                player.Score[highestScorePosition] = highestScore;
            }

        }

        //function to see if game is over
        private bool IsItOver()
        {
            int counterA = 0;
            int counterB = 0;

            //each score that isn't -1 adds one to the counter

            foreach (int i in p1.Score)
            {
                if(i!=-1)
                {
                    counterA++;
                }
            }

            //each score that isn't -1 adds one to the counter
            foreach (int j in p2.Score)
            {
                if (j!= -1)
                {
                    counterB++;
                }
            }

            //if either of the counters are 13(number of values in the score board) it returns game end to be true
            if (counterA == 13 || counterB == 13)
            {
                return true;
            }
            //otherwise it returns not to end the game
            else
            {
                return false;
            }
        }


        //Note:comment this stuff
        private string WhoWon()
        {
            string returnString = "";

            //counters
            int p1Score = 0;
            int p2Score = 0;

            //goes through each score catagory and adds it's value to the score counter
            foreach(int a in p1.Score)
            {
                p1Score += a;
            }

            //goes through each score catagory and adds it's value to the score counter
            foreach (int b in p2.Score)
            {
                p2Score += b;
            }

            //if the score is below 0 it will round it up to 0
            if (p1Score < 0) p1Score = 0;
            if (p2Score < 0) p2Score = 0;

            //player scores writen to the screen
            Console.WriteLine();
            Console.WriteLine(p1.Name + " score:" + p1Score);
            Console.WriteLine(p2.Name + " score:" + p2Score);

            //if player 1 has a higher score return string is set to say they won
            if (p1Score > p2Score)
            {
                returnString = "Congratulations " + p1.Name + " you're the winner!";
            }
            //if player 2 has a higher score return string is set to say they won
            else if (p2Score > p1Score)
            {
                returnString = "Congratulations " + p2.Name + " you're the winner!";
            }
            //if it's a tie return string is set to return such
            else if (p1Score == p2Score)
            {
                returnString = "Woah you tied!";
            }

            return returnString;
        }

    }

}

