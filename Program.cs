﻿using System;
using System.Timers;
using System.IO;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
namespace Typing_App
{
    interface UploadToJson
    {
        void uploadtojson();
    }
    interface showResult
    {
        void ShowResult();
    }
    public class Test : UploadToJson, showResult
    {
        private ArrayList BadChars { get; set; }
        private int Chars { get; set; }
        private int BackTime { get; set; }
        private int TestTime { get; set; }
        private int State { get; set; }
        private ArrayList word { get; set; }
        private ArrayList recordRandom { get; set; }
        private Timer timer { get; set; }
        private int TimeStart { get; set; }
        private int row { get; set; }
        private string[] wordsArray { get; set; }
        public bool testEnd { get; set; }
        public Test(ushort testTime, string[] _wordsArray)
        {
            BadChars = new ArrayList();
            Chars = 0;
            BackTime = 0;
            State = 0;
            word = new ArrayList();
            recordRandom = new ArrayList();
            TestTime = testTime;
            TimeStart = 0;
            wordsArray = _wordsArray;
            testEnd = false;
        }
        private void setTimer()
        {
            timer = new Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(Timesup);
            timer.AutoReset = true;
            timer.Interval = TestTime * 1000 * 60;
            timer.Enabled = true;
        }
        private int GoodChars()
        {
            return Chars - BadChars.Count;
        }
        private double Accuracy()
        {
            return Convert.ToDouble(GoodChars() + BackTime) /
                Convert.ToDouble(Chars + BackTime);
        }
        private double Backspace_Freq()
        {
            return Convert.ToDouble(BackTime) /
                Convert.ToDouble(Chars + BackTime);
        }
        private float wpm()
        {
            return cpm() / 5;
        }
        private float cpm()
        {
            return Chars / TestTime;
        }
        public void ShowResult()
        {
            Console.WriteLine(
                "WPM: " + (float)((int)(wpm() * 10)) / 10 + ",  CPM: " + (float)((int)(cpm() * 10)) / 10 +
                ",  Accuracy: " + Convert.ToDouble(Convert.ToInt32(Accuracy() * 1000)) / 10 + "%" +
                ",  BackSpace Frequency: " + Convert.ToDouble(Convert.ToInt32(Backspace_Freq() * 1000)) / 10 + "%");
        }
        public void uploadtojson()
        {
            var json_element = new Json_element
            {
                TestEndTime = DateTime.Now.ToString()
                    .Replace("\u4E0B\u5348", "P.M.").Replace("\u4E0A\u5348", "A.M."),
                TestLength = TestTime,
                TotalChars = Chars,
                GoodChars = GoodChars(),
                TotalBack = BackTime,
                WrongChars = (string[])BadChars.ToArray(typeof(string))
            };
            JObject json = JObject.Parse(System.Text.Json.JsonSerializer.Serialize(json_element));
            string readprev = File.ReadAllText(@"Databases/TypingPracticeRecords.json");
            JArray prev = JArray.Parse(readprev);
            prev.Add(json);
            string uploadjson = prev.ToString(Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(@"Databases/TypingPracticeRecords.json", uploadjson);
        }
        public void ifUploadFailed()
        {
            var json_element = new Json_element
            {
                TestEndTime = DateTime.Now.ToString()
                    .Replace("\u4E0B\u5348", "P.M.").Replace("\u4E0A\u5348", "A.M."),
                TestLength = TestTime,
                TotalChars = Chars,
                GoodChars = GoodChars(),
                TotalBack = BackTime,
                WrongChars = (string[])BadChars.ToArray(typeof(string))
            };
            string json = System.Text.Json.JsonSerializer.Serialize(json_element);
            File.WriteAllText(@"Databases/latestTypingRecord.json", json);
        }
        public void createLine()
        {
            Console.WriteLine();
            recordRandom.Clear();
            Console.WriteLine();
            int i = 10;
            Random r = new Random(DateTime.Now.Millisecond);
            int n = r.Next(Array.IndexOf(wordsArray, "1980s") - 20);
            while (i != 0)
            {
                Console.Write("\u001b[1m" + wordsArray[n] + "\u001b[0m      ");
                recordRandom.Add(wordsArray[n]);
                n++;
                i--;
            }
            Console.WriteLine();
        }
        public void input()
        {
            ConsoleKeyInfo k = Console.ReadKey();
            if (k.KeyChar == ' ')
            {
                string result = "";
                foreach (char element in word) result += element.ToString();
                Chars += result.Length + 1;
                string standard(int n)
                {
                    return Convert.ToString(recordRandom[n]);
                }
                string Standard = standard(State % 10);
                if (result == Standard)
                {
                    Console.Write(" \u001b[38;5;82m(O)\u001b[0m ");
                }
                else
                {
                    Console.Write(" \u001b[38;5;160m(X)\u001b[0m ");
                    checkResultByChar(result, Standard);
                }
                word.Clear();
                State++;
                if (State % 10 == 0) createLine();
            }
            else if (k.Key == ConsoleKey.Backspace)
            {
                if (word.Count > 0)
                {
                    if (word.Count > recordRandom[State % 10].ToString().Length)
                    {
                        BadChars.Add("Spaces");
                        word.RemoveAt(word.Count - 1);
                        BackTime++;
                    }
                    else
                    {
                        BadChars.Add(recordRandom[State % 10].ToString().Substring(word.Count - 1, 1));
                        word.RemoveAt(word.Count - 1);
                        BackTime++;
                    }
                }
            }
            else
            {
                if (TimeStart == 0) TimeStart++;
                if (TimeStart == 1) { setTimer(); TimeStart++; }
                word.Add(k.KeyChar);
            }
        }
        public void checkResultByChar(string Result, string standard)
        {
            if (standard.Length == Result.Length)
            {
                for (int i = 0; i < Result.Length; i++)
                {
                    if (standard[i] != Result[i])
                    {
                        BadChars.Add(standard[i].ToString());
                    }
                }
            }
            if (Result.Length > standard.Length)
            {
                while (Result.Length != standard.Length) standard += "!";
                for (int i = 0; i < Result.Length; i++)
                {
                    if (Result[i] != standard[i])
                    {
                        if (standard[i] != '!') BadChars.Add(standard[i].ToString());
                        else if (standard[i] == '!') BadChars.Add("Spaces");
                    }
                }
            }
            if (Result.Length < standard.Length)
            {
                while (Result.Length != standard.Length) Result += "!";
                for (int i = 0; i < standard.Length; i++)
                {
                    if (standard[i] != Result[i])
                    {
                        BadChars.Add(standard[i].ToString());
                    }
                }
            }
        }
        public void Timesup(object source, ElapsedEventArgs e)
        {
            testEnd = true;
            timer.Close();
            string uncompleted = "";
            foreach (char element in word) uncompleted += element.ToString();
            Chars += uncompleted.Length;
            checkResultByChar(uncompleted, Convert.ToString(recordRandom[State % 10]).Substring(0, uncompleted.Length));
            Console.WriteLine();
            Console.WriteLine(".");
            Console.WriteLine(".");
            Console.WriteLine(".");
            ShowResult();
            Console.WriteLine(".");
            Console.WriteLine(".");
            Console.WriteLine(".");
            try
            {
                uploadtojson();
            }
            catch (System.StackOverflowException n)
            {
                Console.WriteLine("There's something going wrong, "+n.Message+"\nthe test record will upload to the 'latestTypingRecord.json' file\n.");
                ifUploadFailed();
            }
            catch (OutOfMemoryException n)
            {
                Console.WriteLine("There's something going wrong, "+n.Message+"\nthe test record will upload to the 'latestTypingRecord.json' file\n.");
                ifUploadFailed();
            }
            catch (IOException n)
            {
                Console.WriteLine("There's something going wrong, "+n.Message+"\nthe test record will upload to the 'latestTypingRecord.json' file\n.");
                ifUploadFailed();
            }
            catch (FormatException n)
            {
                Console.WriteLine("There's something going wrong, "+n.Message+"\nthe test record will upload to the 'latestTypingRecord.json' file\n.");
                ifUploadFailed();
            }
            catch (ArgumentException n)
            {
                Console.WriteLine("There's something going wrong, "+n.Message+"\nthe test record will upload to the 'latestTypingRecord.json' file\n.");
                ifUploadFailed();
            }
            finally
            {
                Console.WriteLine("The Datas have been uploaded.");
                Environment.Exit(0);
            }
        }
    }
    public class Json_element
    {
        public string TestEndTime { get; set; }
        public int TestLength { get; set; }
        public int TotalChars { get; set; }
        public int GoodChars { get; set; }
        public int TotalBack { get; set; }
        public string[] WrongChars { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            ushort TestTime = 1;
            void Exception()
            {
                try
                {
                    Console.WriteLine("How long do you want this test be ?\n");
                    Console.Write("Please Enter (minute): (Ex: 3)");
                    TestTime = Convert.ToUInt16(Console.ReadLine());
                }
                catch (IOException e)
                {
                    Console.Error.WriteLine("\n" + e.Message + "系統將自動判定為 1");
                }
                catch (OutOfMemoryException e)
                {
                    Console.Error.WriteLine("\n" + e.Message + "系統將自動判定為 1");
                }
                catch (ArgumentException e)
                {
                    Console.Error.WriteLine("\n" + e.Message + "系統將自動判定為 1");
                }
                catch (FormatException e)
                {
                    Console.Error.WriteLine("\n" + e.Message + "系統將自動判定為 1");
                }
                catch (System.OverflowException e)
                {
                    Console.Error.WriteLine("\n" + e.Message + "系統將自動判定為 1");
                }
                finally
                {
                    if (TestTime == 0)
                    {
                        Console.Error.WriteLine("不可為 0");
                        Exception();
                    }
                    else
                    {
                        Console.WriteLine("\nThis test is going to be " + TestTime +
                            " minites long. Press Enter to continue.");
                        Console.ReadLine();
                        Console.WriteLine("Ok, Let's Go !");
                    }
                }
            }
            string[] getWords()
            {
                StreamReader jsonfile = new StreamReader(@"Databases/Database.json");
                string data = jsonfile.ReadToEnd();
                JObject Dictionary = JObject.Parse(JArray.Parse(data)[0].ToString());
                JArray arrs = (JArray)Dictionary["Words List"];
                string[] arr = arrs.ToObject<string[]>();
                return arr;
            }
            string[] wordsArray = getWords();

            void Mainfunction()
            {
                TestTime = 1;
                Exception();
                Test newTest = new Test(TestTime, wordsArray);
                newTest.createLine();
                while (newTest.testEnd == false)
                {
                    newTest.input();
                }
            }
            Mainfunction();
        }
    }
}