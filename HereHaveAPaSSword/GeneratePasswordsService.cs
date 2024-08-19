using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HereHaveAPaSSword
{
    public class GeneratePasswordsService
    {
        public GeneratePasswordsService() { }

        public List<string> GetPasswords(int totalCharacters, int alphabetMin, int alphabetMax, int numericMin, int numericMax, int specialMin, int specialMax)
        {
            var theLfsrString = GenerateSeed();
            List<string> passwordList = new List<string>();

            for (int i = 0; i < 50; i++)
            {
                var receivedTuple = GetOnePassword(totalCharacters, alphabetMin, alphabetMax, numericMin, numericMax, specialMin, specialMax, theLfsrString);
                var password = receivedTuple.Item1;
                theLfsrString = receivedTuple.Item2;
                passwordList.Add(password);
            }

            return passwordList;
        }

        private Tuple<string, string> GetOnePassword(int totalCharacters, int alphabetMin, int alphabetMax, int numericMin, int numericMax, int specialMin, int specialMax, string theLfsrString)
        {
            string newPassword = "";
            int alphabet = 0;
            int numeric = 0;
            int special = 0;

            for (int i = 0; i < alphabetMin; i++)
            {
                var caseTuple = GetRandomNumber(theLfsrString);
                int upperCase = caseTuple.Item1 % 2;
                theLfsrString = caseTuple.Item2;

                var recievedTuple = GetRandomNumber(theLfsrString);
                int randomNumber = recievedTuple.Item1 % 26 + ('a') * (1 - upperCase) + ('A') *upperCase;
                theLfsrString = recievedTuple.Item2;

                newPassword += Convert.ToChar(randomNumber);
                alphabet++;
            }

            for (int i = 0; i < numericMin; i++)
            {
                var recievedTuple = GetRandomNumber(theLfsrString);
                int randomNumber = recievedTuple.Item1 % 10;
                theLfsrString = recievedTuple.Item2;

                newPassword += randomNumber;
                numeric++;
            }

            string specialCharacters = "!#$%&()*+,-./:;?@[]^_`{|}";
            for (int i = 0; i < specialMin; i++)
            {
                var recievedTuple = GetRandomNumber(theLfsrString);
                int randomNumber = recievedTuple.Item1 % specialCharacters.Length;
                theLfsrString = recievedTuple.Item2;
                newPassword += specialCharacters[randomNumber];
                special++;
            }

            while (totalCharacters > newPassword.Length)
            {
                var someTuple = GetRandomNumber(theLfsrString);
                int characterType = someTuple.Item1 % 20;
                theLfsrString = someTuple.Item2;

                if (characterType < 10 && (alphabetMax == -1 || alphabet < alphabetMax))
                {
                    var caseTuple = GetRandomNumber(theLfsrString);
                    int upperCase = caseTuple.Item1 % 2;
                    theLfsrString = caseTuple.Item2;

                    var recievedTuple = GetRandomNumber(theLfsrString);
                    int randomNumber = recievedTuple.Item1 % 26 + ('a') * (1 - upperCase) + ('A') * upperCase;
                    theLfsrString = recievedTuple.Item2;

                    newPassword += Convert.ToChar(randomNumber);
                    alphabet++;
                }

                if (totalCharacters == newPassword.Length) break;

                if (characterType > 10 && (numericMax == -1 || numeric < numericMax))
                {
                    var recievedTuple = GetRandomNumber(theLfsrString);
                    int randomNumber = recievedTuple.Item1 % 10;
                    theLfsrString = recievedTuple.Item2;

                    newPassword += randomNumber;
                    numeric++;
                }

                if (totalCharacters == newPassword.Length) break;

                if (characterType == 10 && (specialMax == -1 || special < specialMax))
                {
                    var recievedTuple = GetRandomNumber(theLfsrString);
                    int randomNumber = recievedTuple.Item1 % specialCharacters.Length;
                    theLfsrString = recievedTuple.Item2;
                    newPassword += specialCharacters[randomNumber];
                    special++;
                }
            }

            var receivedTuple = ShufflePassword(newPassword, theLfsrString);

            return Tuple.Create(receivedTuple.Item1, receivedTuple.Item2);
        }

        private Tuple<string, string> ShufflePassword(string password, string theLfsrString)
        {
            var shuffledPassword = "";
            var initialLenght = password.Length;

            for (int i = 0; i < initialLenght; i++)
            {
                var receivedTuple = GetRandomNumber(theLfsrString);
                var number = receivedTuple.Item1 % password.Length;
                theLfsrString = receivedTuple.Item2;
                shuffledPassword += password[number];
                password = password.Remove(number, 1);
            }

            return Tuple.Create(shuffledPassword, theLfsrString);
        }

        private String GenerateSeed()
        {
            var currentTime = DateTime.Now;
            string scrambledDate = currentTime.Month.ToString() + currentTime.Day.ToString() + currentTime.Hour.ToString() + currentTime.Year.ToString() + currentTime.Millisecond.ToString() + currentTime.Minute.ToString();
            string fileName = "SpecialImage.png";
            scrambledDate += System.IO.Path.GetFullPath(fileName).ToString();
            scrambledDate += currentTime.Ticks.ToString();

            SHA256 mySHA256 = SHA256.Create();
            var hashedSeed = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(scrambledDate));

            StringBuilder seed = new StringBuilder();
            foreach (byte b in hashedSeed)
            {
                seed.Append(b.ToString("x2"));
            }

            string resultedString = "";
            foreach (char ch in seed.ToString())
            {
                resultedString += CharToBinary((char)ch);
            }

            return resultedString;
        }

        private string CharToBinary(char c)
        {
            var number = Convert.ToInt32(c);
            return Convert.ToString(number, 2);
        }

        private Tuple<int, string> GetRandomNumber(string theLfsrString)
        {
            var receivedTuple = LFSR(theLfsrString);
            var generateRandomBytes = receivedTuple.Item1;
            theLfsrString = receivedTuple.Item2;

            int number = 0;
            for (int i = 0; i < 22; i++)
            {
                number *= 2;
                if (generateRandomBytes[i] == '1') { number++; }
            }

            return Tuple.Create(number, theLfsrString);
        }

        private char XorOperation(char a, char b)
        {
            if (a == b)
            {
                return '0';
            }
            return '1';
        }

        private char MultipleXOR(string myArray)
        {
            char rez = myArray[2];
            for (int i = 4; i <= 15; i += 2)
            {
                rez = XorOperation(rez, myArray[i]);
            }
            return rez;
        }

        private Tuple<string, string> LFSR(string myArray)
        {
            string generatedArray = "";
            while (true)
            {
                generatedArray += myArray[myArray.Length - 1];

                if (generatedArray.Length == 22)
                {
                    break;
                }

                char rezXor = MultipleXOR(myArray);

                myArray = myArray.Remove(myArray.Length - 1, 1);
                myArray = rezXor + myArray;
            }
            return Tuple.Create(generatedArray, myArray);
        }
    }
}
