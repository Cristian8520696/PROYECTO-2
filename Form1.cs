using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using PROYECTO_AGRI_AI;



namespace PROYECTO_AGRI_AI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnConsultar_Click(object sender, EventArgs e)
        {
            string[] palabrasClave = {
    // Cultivos comunes
    "maíz", "frijol", "tomate", "chile", "yuca", "cebolla", "miltomate", "plátano", "ayote", "café", "maiz",

    // Tareas del campo
    "sembrar", "siembra", "cosecha", "regar", "riego", "chapear", "arar", "abonar", "fumigar", "limpiar", "curar", "echar veneno",

    // Partes de la planta
    "mata", "palo", "flor", "hoja", "raíz", "fruto", "raiz",

    // Suelo/clima
    "tierra", "barro", "hondo", "quebrado", "arenoso", "bajío", "seco", "húmedo",
    "sol", "lluvia", "calor", "frío", "viento", "tormenta",

    // Plagas y problemas
    "plaga", "bicho", "gusano", "insecto", "ratón", "chapulín", "culebra", "zancudo", "moho",
    "quemado", "podrido", "marchita", "seca", "muriendo", "no crece", "enferma", "tiene gusano",

    // Fertilizantes y químicos
    "abono", "foliar", "urea", "triple 15", "cal agrícola", "remedio", "químico", "veneno", "medicina", "quimico",

    // Animales
    "gallina", "zorro", "venado",

    // Expresiones rurales
    "la mata está enferma", "se lo comió", "se está secando", "qué le echo", "lo quemó el sol", "está muriendo"
};
            string consultaUsuario = txtConsulta.Text.ToLower();

            // Verificar si la consulta contiene alguna palabra clave agrícola
            bool esConsultaAgricola = palabrasClave.Any(p => consultaUsuario.Contains(p));

            if (!esConsultaAgricola)
            {
                MessageBox.Show("❌ La consulta no parece estar relacionada con la agricultura. Por favor, verifica e intenta de nuevo.", "Consulta no válida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                statusStrip1.Text = "⚠️ Consulta no válida: no está relacionada con la agricultura.";
                return;
            }
            string prompt = $"Responde como un profesional de la agricultura con experiencia en Jutiapa, Guatemala. La consulta es: {txtConsulta.Text}";


            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", " Bearer ");

                var body = new
                {
                    model = "mistralai/mistral-7b-instruct:free",
                    messages = new[]
                    {
                new { role = "user", content = prompt }
            }
                };

                string json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
                string result = await response.Content.ReadAsStringAsync();
                dynamic respuesta = JsonConvert.DeserializeObject(result);

                if (respuesta?.choices != null && respuesta.choices.Count > 0)
                {
                    richTextBoxRespuesta.Text = respuesta.choices[0].message.content;
                }
                else
                {
                    richTextBoxRespuesta.Text = "No se recibió una respuesta válida de la IA.";
                    statusStrip1.Text = "⚠️ Error: respuesta vacía o malformada.";
              }
                ESTRUCTURA.GuardarEnBD(prompt, richTextBoxRespuesta.Text);
                ESTRUCTURA.CrearCarpeta();
                ESTRUCTURA.CrearWord(prompt, richTextBoxRespuesta.Text);
            }

        }


        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtConsulta.Text = string.Empty;
            richTextBoxRespuesta.Text = string.Empty;


            txtConsulta.Focus();
        }
    }
}
