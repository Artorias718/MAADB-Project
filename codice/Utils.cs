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
    public static void UploadLemmiOfLexres(Dictionary<string, Dictionary<string, int>> lemmi)
    {
        MySqlConnection conn = new MySqlConnection("server=localhost;user=alfredo;pwd=password;database=maadb");
        MySqlCommand cmd = new MySqlCommand();
        cmd.Connection = conn;
        conn.Open();
        
        // Eseguire una query per verificare se la tabella esiste
        cmd.CommandText = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{conn.Database}' AND table_name = 'outer_dict'";
        
        int tableCount = Convert.ToInt32(cmd.ExecuteScalar());

        if (tableCount > 0)
        {
            Console.WriteLine($"La tabella 'outer_dict' esiste nel database '{conn.Database}'.");
        }
        else
        {
            Console.WriteLine($"La tabella 'outer_dict' non esiste nel database '{conn.Database}'.");

            cmd.CommandText = "CREATE TABLE outer_dict (" +
                            "id INT NOT NULL AUTO_INCREMENT," +
                            "PRIMARY KEY (id)" +
                            ");" +

                            "CREATE TABLE inner_dict (" +
                            "id INT NOT NULL AUTO_INCREMENT," +
                            "key_name VARCHAR(255) NOT NULL," +
                            "value INT," +
                            "outer_dict_id INT NOT NULL," +
                            "PRIMARY KEY (id)," +
                            "FOREIGN KEY (outer_dict_id) REFERENCES outer_dict(id)" +
                            ");";
            
            cmd.ExecuteNonQuery();
        }

        // Insert the data into the database
        string insertOuterDictQuery = "INSERT INTO outer_dict () VALUES (); SELECT LAST_INSERT_ID();";
        using (MySqlCommand outerDictCommand = new MySqlCommand(insertOuterDictQuery, conn))
        {
            // Execute the query to insert the outer_dict record and retrieve the inserted ID
            int outerDictId = Convert.ToInt32(outerDictCommand.ExecuteScalar());

            // Iterate through the dictionary and insert each key-value pair into the inner_dict table
            string insertInnerDictQuery = "INSERT INTO inner_dict (key_name, value, outer_dict_id) VALUES (@keyName, @value, @outerDictId);";
            using (MySqlCommand innerDictCommand = new MySqlCommand(insertInnerDictQuery, conn))
            {
                // Add parameters to the innerDictCommand
                innerDictCommand.Parameters.Add("@keyName", MySqlDbType.VarChar);
                innerDictCommand.Parameters.Add("@value", MySqlDbType.Int32);
                innerDictCommand.Parameters.Add("@outerDictId", MySqlDbType.Int32);

                foreach (KeyValuePair<string, Dictionary<string, int>> outerPair in lemmi)
                {
                    // Insert the outer key into the outer_dict table
                    innerDictCommand.Parameters["@outerDictId"].Value = outerDictId;
                    innerDictCommand.Parameters["@keyName"].Value = outerPair.Key;
                    innerDictCommand.Parameters["@value"].Value = null;
                    innerDictCommand.ExecuteNonQuery();

                    // Retrieve the inserted outer key ID
                    int outerKeyId = Convert.ToInt32(innerDictCommand.LastInsertedId);

                    // Iterate through the inner dictionary and insert each key-value pair into the inner_dict table
                    foreach (KeyValuePair<string, int> innerPair in outerPair.Value)
                    {
                        innerDictCommand.Parameters["@outerDictId"].Value = outerDictId;
                        innerDictCommand.Parameters["@keyName"].Value = innerPair.Key;
                        innerDictCommand.Parameters["@value"].Value = innerPair.Value;
                        innerDictCommand.ExecuteNonQuery();
                    }
                }
            }
        }

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
