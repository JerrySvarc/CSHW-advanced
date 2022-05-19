namespace Labs_LinqToObjects
{
    class Program
    {

        static void PrintPeople(IEnumerable<Person> people)
        {
            Console.WriteLine("Main: foreach:");
            foreach (Person person in people)
            {
                Console.WriteLine("Main: got " + person);
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Press ENTER to run without debug prints,");
            Console.WriteLine("Press D1 + ENTER to enable some debug prints,");
            Console.Write("Press D2 + ENTER to enable all debug prints: ");
            string command = Console.ReadLine().ToUpper();
            DebugPrints1 = command == "D2" || command == "D1" || command == "D";
            DebugPrints2 = command == "D2";
            Console.WriteLine();

            var groupA = new Group();

            HighlightedWriteLine("Assignment 1: Vsechny osoby, ktere nepovazuji nikoho za sveho pritele.");
            var query1 = from person in groupA where person.Friends.Count() == 0 select person;
            PrintPeople(query1);
            Console.WriteLine();

            HighlightedWriteLine("Assignment 2: Vsechny osoby setridene vzestupne podle jmena, ktere jsou starsi 15 let, a jejichz jmeno zacina na pismeno D nebo vetsi.");
            var query2 = from person in groupA where person.Age > 15 && person.Name[0] >= 'D' orderby person.Name select person;
            PrintPeople(query2);
            Console.WriteLine();

            HighlightedWriteLine("Assignment 3: Vsechny osoby, ktere jsou ve skupine nejstarsi, a jejichz jmeno zacina na pismeno T nebo vetsi.");
            var query3 = from person in groupA where person.Age == groupA.Max(s => s.Age) && person.Name[0] >= 'T' select person;
            PrintPeople(query3);
            Console.WriteLine();

            HighlightedWriteLine("Assignment 4: Vsechny osoby, ktere jsou starsi nez vsichni jejich pratele.");
            var query4 = from person in groupA where person.Friends.Count() == 0 || person.Age > person.Friends.Max(s => s.Age) select person;
            PrintPeople(query4);
            Console.WriteLine();

            HighlightedWriteLine("Assignment 5: Vsechny osoby, ktere nemaji zadne pratele (ktere nikoho nepovazuji za sveho pritele, a zaroven ktere nikdo jiny nepovazuje za sveho pritele).");
            var query5 = from person in groupA where person.Friends.Count() == 0 && (from x in groupA where x != person && x.Friends.Contains(person) select x).Count() == 0 select person;
            PrintPeople(query5);
            Console.WriteLine();

            HighlightedWriteLine("Assignment 6: Vsechny osoby, ktere jsou necimi nejstarsimi prateli (s opakovanim).");
            var query6 = from person in groupA from x in groupA where person.Friends.Contains(x) && person.Friends.Max(s => s.Age) == x.Age select x;
            PrintPeople(query6);
            Console.WriteLine();

            HighlightedWriteLine("Assignment 6B: Vsechny osoby, ktere jsou necimi nejstarsimi prateli (bez opakovani).");
            var query7 = from person in groupA where (from x in groupA where x.Friends.Contains(person) && person.Age == x.Friends.Max(s => s.Age) select x).Count() > 0 select person;
            PrintPeople(query7); // Na vystupu jen vychazi jine poradi osob oproti referencnimu reseni
            Console.WriteLine();

            HighlightedWriteLine("Assignment 7: Vsechny osoby, ktere jsou nejstarsimi prateli osoby starsi nez ony samy (s opakovanim).");
            var query8 = from person in groupA from x in groupA where person.Age < x.Age && x.Friends.Contains(person) && x.Friends.Max(s=>s.Age) == person.Age select person;
            PrintPeople(query8);// Na vystupu jen vychazi jine poradi osob oproti referencnimu reseni
            Console.WriteLine();

            HighlightedWriteLine("Assignment 7B: Vsechny osoby, ktere jsou nejstarsimi prateli osoby starsi nez ony samy (bez opakovani).");
            var query9 = from person in groupA where (from x in groupA where person.Age < x.Age && x.Friends.Contains(person) && x.Friends.Max(s => s.Age) == person.Age select x).Count() > 0 select person;
            PrintPeople(query9);// Na vystupu jen vychazi jine poradi osob oproti referencnimu reseni
            Console.WriteLine();

            HighlightedWriteLine("Assignment 7C: Vsechny osoby, ktere jsou nejstarsimi prateli osoby starsi nez ony samy (bez opakovani a setridene sestupne podle jmena osoby).");
            var query10 = from person in groupA where (from x in groupA where person.Age < x.Age && x.Friends.Contains(person) && x.Friends.Max(s => s.Age) == person.Age select x).Count() > 0 orderby person.Name descending select person;
            PrintPeople(query10);// Na vystupu jen vychazi jine poradi osob oproti referencnimu reseni
        }

        public static void HighlightedWriteLine(string s)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(s);
            Console.ForegroundColor = oldColor;
        }

        public static bool DebugPrints1 = false;
        public static bool DebugPrints2 = false;

        class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public IEnumerable<Person> Friends { get; private set; }

            /// <summary>
            /// DO NOT USE in your LINQ queries!!!
            /// </summary>
            public IList<Person> FriendsListInternal { get; private set; }

            class EnumWrapper<T> : IEnumerable<T>
            {
                IEnumerable<T> innerEnumerable;
                Person person;
                string propName;

                public EnumWrapper(Person person, string propName, IEnumerable<T> innerEnumerable)
                {
                    this.person = person;
                    this.propName = propName;
                    this.innerEnumerable = innerEnumerable;
                }

                public IEnumerator<T> GetEnumerator()
                {
                    if (Program.DebugPrints1) Console.WriteLine(" # Person(\"{0}\").{1} is being enumerated.", person.Name, propName);

                    foreach (var value in innerEnumerable)
                    {
                        yield return value;
                    }

                    if (Program.DebugPrints2) Console.WriteLine(" # All elements of Person(\"{0}\").{1} have been enumerated.", person.Name, propName);
                }

                System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }

            public Person()
            {
                FriendsListInternal = new List<Person>();
                Friends = new EnumWrapper<Person>(this, "Friends", FriendsListInternal);
            }

            public override string ToString()
            {
                return string.Format("Person(Name = \"{0}\", Age = {1})", Name, Age);
            }
        }

        class Group : IEnumerable<Person>
        {
            Person anna, blazena, ursula, daniela, emil, vendula, cyril, frantisek, hubert, gertruda;

            public Group()
            {
                anna = new Person { Name = "Anna", Age = 22 };
                blazena = new Person { Name = "Blazena", Age = 18 };
                ursula = new Person { Name = "Ursula", Age = 22, FriendsListInternal = { blazena } };
                daniela = new Person { Name = "Daniela", Age = 18, FriendsListInternal = { ursula } };
                emil = new Person { Name = "Emil", Age = 21 };
                vendula = new Person { Name = "Vendula", Age = 22, FriendsListInternal = { blazena, emil } };
                cyril = new Person { Name = "Cyril", Age = 21, FriendsListInternal = { daniela } };
                frantisek = new Person { Name = "Frantisek", Age = 15, FriendsListInternal = { anna, blazena, cyril, daniela, emil } };
                hubert = new Person { Name = "Hubert", Age = 10 };
                gertruda = new Person { Name = "Gertruda", Age = 10, FriendsListInternal = { frantisek } };

                blazena.FriendsListInternal.Add(ursula);
                blazena.FriendsListInternal.Add(vendula);
                ursula.FriendsListInternal.Add(daniela);
                daniela.FriendsListInternal.Add(cyril);
                emil.FriendsListInternal.Add(vendula);
            }

            public IEnumerator<Person> GetEnumerator()
            {
                if (Program.DebugPrints1) Console.WriteLine("*** Group is being enumerated.");

                yield return hubert;
                yield return anna;
                yield return frantisek;
                yield return blazena;
                yield return ursula;
                yield return daniela;
                yield return emil;
                yield return vendula;
                yield return cyril;
                yield return gertruda;

                if (Program.DebugPrints1) Console.WriteLine("*** All elements of Group have been enumerated.");
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
