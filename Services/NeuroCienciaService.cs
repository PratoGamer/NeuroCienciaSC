using System;
using System.Collections.Generic;
using System.Text;

namespace NeuroCienciaSC.Services
{
    public class NeuroCienciaService
    {
        // Lista para Almacenar la Secuencia de Numeros
        public List<int> secuencia { get; set; } = new List<int>();

        // Diccionario para los Costos de Operacion
        public Dictionary<string, int> operacion { get; set; } = new Dictionary<string, int>()
        {
            { "+1", 1 },
            { "-1", 1 },
            { "*2", 3 },
            { "/2", 2 }
        };

        // Resultados
        public int costoTotal { get; set; } = 0;
        public string tipoOrden { get; set; } = string.Empty;

        // Guardar Resultado en Tupla Indice - Operacion
        public List<(int indice, string operacion)> secuenciaOrdenada { get; set; } = new List<(int indice, string operacion)>();

        // Metodo para Ordenar de Forma Ascendente
        public void OrdenarAscendenteMinimo()
        {


        }

    }

}
