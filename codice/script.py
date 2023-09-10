import json
import nltk
from nltk.stem import WordNetLemmatizer

# Scarica il wordnet corpus (se non è già stato scaricato)
nltk.download('wordnet')

# Crea un oggetto WordNetLemmatizer
lemmatizer = WordNetLemmatizer()

# Definisci una funzione per la lemmatizzazione di una lista di parole
def lemmatize_words(word_list):
    lemma_list = [lemmatizer.lemmatize(word) for word in word_list]
    return lemma_list

# Specifica il percorso del file JSON da cui leggere i dati
input_file_path = "input.json"  # Sostituisci con il percorso del tuo file JSON di input
output_file_path = "output.json"  # Specifica il percorso del file JSON in cui desideri salvare il risultato

# Leggi il JSON dal file di input
with open(input_file_path, "r", encoding="utf-8") as input_file:
    data = json.load(input_file)

# Esegui la lemmatizzazione sui lemmi
for item in data:
    item["Lemmi"] = lemmatize_words(item["Lemmi"])

# Scrivi il risultato in un nuovo file JSON
with open(output_file_path, "w", encoding="utf-8") as output_file:
    json.dump(data, output_file, ensure_ascii=False, indent=4)

print(f"Risultato salvato nel file: {output_file_path}")
