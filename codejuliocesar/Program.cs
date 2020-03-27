using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace codejuliocesar
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();

        static async Task Main()
        {
            try
            {
                //Get para API
                HttpResponseMessage response = await client.GetAsync("https://api.codenation.dev/v1/challenge/dev-ps/generate-data?token=SEU-TOKEN");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                //criar o caminho arquivo Json
                string arquivo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "answer.json");

                //cria o arquivo
                File.Create(arquivo).Dispose();

                //Preencher com API o Arquivo Json
                File.WriteAllText(arquivo, responseBody);

                Answer answer;

                string palavra = "";

                using (StreamReader r = new StreamReader(arquivo))
                {
                    //lê o arquivo Json
                    string json = r.ReadToEnd();
                    //organiza o conteudo fo Json
                    answer = JsonConvert.DeserializeObject<Answer>(json);

                    palavra = answer.Cifrado;
                    //numerocasas = answer.Numero_Casas;   

                }
                //fazer o codigo aqui para baixo----------------


                int ascii;
                for (int i = 0; i < palavra.Length; i++)
                {

                    string verificar = palavra[i].ToString();
                    var retorno = Regex.IsMatch(verificar, @"\w");

                    if (retorno)
                    {

                        ascii = Convert.ToInt32(palavra[i]) - answer.Numero_Casas;

                        if (ascii < 97) 
                        { 
                            ascii += 26; 
                        }

                    }

                    else
                    {
                        ascii = Convert.ToInt32(palavra[i]);
                    }
                    answer.Decifrado += Convert.ToChar(ascii);
                }

                string hash = Hash(answer.Decifrado);

                answer.Resumo_Criptografico = hash;

                string jsonAnswer = JsonConvert.SerializeObject(answer);

                using (var sw = new StreamWriter(arquivo))
                {
                    sw.Write(jsonAnswer);
                    sw.Flush();
                }


                byte[] bytes = Encoding.ASCII.GetBytes(jsonAnswer);
                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(new MemoryStream(bytes)), "answer", arquivo);


                HttpResponseMessage submitResponse = await client.PostAsync(@"https://api.codenation.dev/v1/challenge/dev-ps/submit-solution?token=SEU-TOKEN", content);

                string mensagem = await submitResponse.Content.ReadAsStringAsync();

                Console.WriteLine(mensagem);


            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }

            Console.ReadLine();

        }

        #region metodo de criptografia
        static string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
        #endregion
    }
}
