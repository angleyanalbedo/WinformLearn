namespace ObservableContainer
{
    internal class Program
    {

        static void TestObserableDict()
        {
            var dict = new ObservableDictionary<string, int>();
            dict.DictionaryChanged += (sender, e) =>
            {
                Console.WriteLine($"ChangeType: {e.ChangeType}, Key: {e.Key}, Value: {e.Value}");
            };
            dict.Add("One", 1);
            dict["Two"] = 2;
            dict.Remove("One");
            dict.Clear();
        }
        static void Main(string[] args)
        {
           TestObserableDict();
        }
    }
}
