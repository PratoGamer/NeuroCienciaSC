using System;
using System.Collections.Generic;
using System.Text;

namespace NeuroCienciaSC.Services
{
    // Clase Principal del Servicio de NeuroCiencia
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

        // Guardar Resultado en Tupla Indice - Operacion
        public List<(int indice, string operacion)> secuenciaOrdenada { get; set; } = new List<(int, string)>();

        // Lista de Costos
        public List<int> costos { get; set; } = new List<int>();

        // Metodo para Sumar Todos los Costos LINQ
        public int SumarCostos() => costos.Sum();

        // Diccionario para Memoria
        public Dictionary<string, Memoria> estadoMemoria = new Dictionary<string, Memoria>();

        private int mejorCosto; 

        // Metodo para Ordenar de Forma Ascendente
        public async Task OrdenarAscendenteMinimo()
        {
            // Limpiar todos los Valores
            secuenciaOrdenada.Clear();
            costos.Clear();
            estadoMemoria.Clear();
            mejorCosto = int.MaxValue;

            // Verificar que Secuencia no este Vacío o con un Solo Valor
            if (secuencia == null || secuencia.Count <= 1) return;

            // Copia de la Secuencia Original
            List<int> secuenciaOriginal = new List<int>(secuencia);

            // Usar el Algortimo Recursivo
            Memoria solucionOptima = ResolverMemorizacion(secuenciaOriginal, 0);

            // Guardar la Solucion Optima
            if (solucionOptima != null)
            {
                secuenciaOrdenada = solucionOptima.Operaciones;
                costos = solucionOptima.Costos;
            }
        }

        // Metodo Recursivo con Memorizacion
        private Memoria ResolverMemorizacion(List<int> secuenciaActual, int costoAcumulado)
        {
            // PODA del Costo para no Desbordar Memoria
            if (costoAcumulado >= mejorCosto || costoAcumulado >= 15) return null;

            // Transformar a un String
            string estadoActual = string.Join(",", secuenciaActual);

            // Memorizacion Verificar si el Estado ya fue Calculado
            if (estadoMemoria.ContainsKey(estadoActual))
            {
                return estadoMemoria[estadoActual];
            }

            // Secuencia ya Ordenada Ascendentemente
            if (OrdenadaAscendente(secuenciaActual))
            {
                // Guardar el Mejor Costo
                if (costoAcumulado < mejorCosto)
                {
                    mejorCosto = costoAcumulado;
                }

                // Devolver la Misma Secuencia
                return new Memoria
                {
                    Operaciones = new List<(int, string)>(),
                    Costos = new List<int>(),
                    Secuencia = new List<int>(secuenciaActual)
                };
            }

            // Empezar la Recursion
            Memoria mejorMemoria = null;
            int menorCosto = int.MaxValue;

            // Recorrer la Secuencia para Operar
            for (int i = 0; i < secuenciaActual.Count; i++)
            {
                // Valor Original
                int valorOriginal = secuenciaActual[i];

                // Operaciones que se Pueden Aplicar
                List<string> operacionesDisponibles = new List<string> { "+1", "-1", "*2" };
                if (valorOriginal % 2 == 0) operacionesDisponibles.Add("/2");

                // Aplicar Cada una de las Operaciones Disponibles
                foreach (string op in operacionesDisponibles)
                {
                    // Aplicar cada Operacion
                    if (op == "+1" && secuenciaActual[i] < 100) secuenciaActual[i] += 1;
                    else if (op == "-1" && secuenciaActual[i] > 1) secuenciaActual[i] -= 1;
                    else if (op == "*2" && secuenciaActual[i] > 0 && secuenciaActual[i] < 50) secuenciaActual[i] *= 2;
                    else if (op == "/2") secuenciaActual[i] /= 2;
                    else continue;

                    // Almacenar el Costo de la Operacion
                    int costoOperacion = operacion[op];

                    // Usar la Recursion de Memoria
                    Memoria subMemoria = ResolverMemorizacion(secuenciaActual, costoAcumulado + costoOperacion);

                    // Verificar Resultado de la SubMemoria
                    if (subMemoria != null)
                    {
                        // Obtener el Costo Total de esta Ruta
                        int costoTotal = costoOperacion + subMemoria.Costos.Sum();

                        // Verificar el Menor Costo
                        if (costoTotal < menorCosto)
                        {
                            menorCosto = costoTotal;

                            // Construir la Solucion
                            mejorMemoria = new Memoria();
                            mejorMemoria.Operaciones.Add((i, op));
                            mejorMemoria.Operaciones.AddRange(subMemoria.Operaciones);
                            mejorMemoria.Costos.Add(costoOperacion);
                            mejorMemoria.Costos.AddRange(subMemoria.Costos);
                            mejorMemoria.Secuencia = new List<int>(subMemoria.Secuencia);

                        }
                    }

                    // Devolver el Valor Original
                    secuenciaActual[i] = valorOriginal;
                }
            }

            // Guardar en Memoria antes de Terminar
            if (mejorMemoria != null)
            {
                estadoMemoria[estadoActual] = mejorMemoria;
            }

            // Terminar la Recursion
            return mejorMemoria;

        }

        // Verificar si la Secuencia esta Ordenada Ascendentemente
        private bool OrdenadaAscendente(List<int> lista)
        {
            for (int i = 0; i < lista.Count - 1; i++)
            {
                if (lista[i] > lista[i + 1]) return false;
            }
            return true;
        }

    }

    // Clase para Guardar Memoria
    public class Memoria
    {
        public List<(int indice, string operacion)> Operaciones { get; set; } = new List<(int, string)>();
        public List<int> Costos { get; set; } = new List<int>();
        public List<int> Secuencia { get; set;  } = new List<int>();
    }

}
