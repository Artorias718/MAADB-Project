using System.Data;
using MySql.Data.MySqlClient;

public class Utils
{
    public static string[] ExtractEmoji(string fileUrl)
    {
        int start = File.ReadAllText(fileUrl).IndexOf('[');
        int end = File.ReadAllText(fileUrl).LastIndexOf(']');

        string text = File.ReadAllText(fileUrl).Substring(start + 1, end - start - 1);
        string[] splittedText = text.Split(',');


        for (int i = 0; i < splittedText.Length; i++)
        {

            splittedText[i] = splittedText[i].Trim();

            if (splittedText[i][0].Equals('\\'))
            {
                splittedText[i] = splittedText[i].Remove(0, 1).Replace("\r\n", "");
            }

            //Console.WriteLine("***" + splittedText[i] + "***");

            splittedText[i] = splittedText[i].Substring(1, splittedText[i].Length - 2);

            //Console.WriteLine(splittedText[i]);

        }
        return splittedText;
    }

    public static void StampaDizionario(Dictionary<string, Dictionary<string, int>> d)
    {
        foreach (var kv in d)
        {
            Console.WriteLine("Lemma: " + kv.Key);
            Console.WriteLine("Risorse:");
            foreach (var risorsa in kv.Value)
            {

                Console.WriteLine(risorsa.Key + ": " + risorsa.Value);

            }
            Console.WriteLine();
        }
    }


/*
INSERT INTO table_name (column1, column2, column3, ...)
VALUES (value1, value2, value3, ...);
*/

    //string emozione, List<Dictionary<string, int>> lemmi, bool dropIfNotEmpty
    public static void UploadLemmiOfLexres()
    {
        MySqlConnection conn = new MySqlConnection("server=localhost;user=alfredo;pwd=password;database=maadb");
        MySqlCommand cmd = new MySqlCommand();
        cmd.Connection = conn;
        conn.Open();

        cmd.CommandText = "CREATE TABLE MyTable (ID int PRIMARY KEY, Name varchar(255), Age int);";
        cmd.ExecuteNonQuery();

        conn.Close();

/*
        if (dropIfNotEmpty)
        {
            DropIfNotEmpty(cmd, NomiDbMySql.RISORSA_LESSICALE, emozione);
        }

        List<Tuple<string>> parolePerRisLes = new List<Tuple<string>>();
        List<Tuple<string>> parole = new List<Tuple<string>>();

        foreach (var lemma in lemmi)
        {
            var resources = lemma.Keys;
            parole.Add(new Tuple<string>(lemma["lemma"].ToString()));

            foreach (var res in resources)
            {
                parolePerRisLes.Add(new Tuple<string>(res.ToString(), emozione, lemma["lemma"].ToString()));
            }
        }

        InsertParoleInRisLes(cmd, parolePerRisLes);
        InsertParole(cmd, parole);
        Disconnect(conn);
        return parolePerRisLes.Count;
        */
    }
}
