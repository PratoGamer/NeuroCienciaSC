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
        public List<(int indice, string operacion, int valorAntes, int valorDespues)> secuenciaOrdenada { get; set; } = new List<(int, string, int, int)>();

        // Lista de Costos
        public List<int> costos { get; set; } = new List<int>();

        // Metodo para Sumar Todos los Costos LINQ
        public int SumarCostos() => costos.Sum();

        // Lista de la Secuencia Final
        public List<int> secuenciaFinal { get; set; } = new List<int>();

        // Diccionario para Memoria
        public Dictionary<string, Memoria> estadoMemoria = new Dictionary<string, Memoria>();

        // Almacenar Estados Fallidos para Evitar Desvordamiento de Memoria
        private HashSet<string> estadosFallidos = new HashSet<string>();

        // Mejor Costo Encontrado
        private int mejorCosto; 

        // Metodo para Ordenar de Forma Ascendente
        public async Task OrdenarAscendenteMinimo()
        {
            // Limpiar todos los Valores
            secuenciaOrdenada.Clear();
            costos.Clear();
            estadoMemoria.Clear();
            estadosFallidos.Clear();
            mejorCosto = int.MaxValue;
            secuenciaFinal.Clear();

            // Verificar que Secuencia no este Vacio o con un Solo Valor
            if (secuencia == null || secuencia.Count <= 1) return;

            // Copia de la Secuencia Original
            List<int> secuenciaOriginal = new List<int>(secuencia);

            // Usar el Algortimo Recursivo
            Memoria solucionOptima = await Task.Run(() => ResolverMemorizacion(secuenciaOriginal, 0));

            // Guardar la Solucion Optima
            if (solucionOptima != null)
            {
                secuenciaOrdenada = solucionOptima.Operaciones;
                costos = solucionOptima.Costos;
                secuenciaFinal = solucionOptima.Secuencia;
            }
        }

        // Metodo Recursivo con Memorizacion
        private Memoria ResolverMemorizacion(List<int> secuenciaActual, int costoAcumulado)
        {
            // PODA del Costo para no Desbordar Memoria
            if (costoAcumulado >= mejorCosto) return null;

            // Transformar a un String
            string estadoActual = string.Join(",", secuenciaActual);

            // Verificar los Estados Fallidos
            if (estadosFallidos.Contains(estadoActual)) return null;

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
                    Operaciones = new List<(int, string, int, int)>(),
                    Costos = new List<int>(),
                    Secuencia = new List<int>(secuenciaActual)
                };
            }

            // PODA de Profundidad
            if (costoAcumulado > 30)
            {
                estadosFallidos.Add(estadoActual);
                return null;
            }

            // Empezar la Recursion
            Memoria mejorMemoria = null;
            int menorCosto = int.MaxValue;

            // Lista para Guardar Elemtos Desordenados
            List<int> indiceOperar = new List<int>();

            // Recorrer la Secuencia para Operar
            for (int i = 0; i < secuenciaActual.Count; i++)
            {

                // Verificar si hay Elementos Desordenados
                bool modificacion = false;

                // Elementos Desordenados a la Izquierda
                for (int j = 0; j < i; j++)
                {
                    if (secuenciaActual[j] >= secuenciaActual[i])
                    {
                        modificacion = true;
                        break;
                    }
                }

                // Elementos Desordenados a la Derecha
                if (!modificacion)
                {
                    for (int j = i + 1; j < secuenciaActual.Count; j++)
                    {
                        if (secuenciaActual[j] <= secuenciaActual[i])
                        {
                            modificacion = true;
                            break;
                        }
                    }
                }

                // Agregar a la Lista
                if (modificacion)
                {
                    indiceOperar.Add(i);
                }

            }

            // Evitar la Lista Vacia
            if (indiceOperar.Count == 0)
            {
                indiceOperar = Enumerable.Range(0, secuenciaActual.Count).ToList();
            }

            // Recorrer Solo los Indices para Operar
            foreach (int ind in indiceOperar)
            {
                // Valor Original
                int valorOriginal = secuenciaActual[ind];

                // Operaciones a Aplicar
                List<string> operacionesDisponibles = new List<string>();
                if (valorOriginal % 2 == 0 && valorOriginal > 1) operacionesDisponibles.Add("/2");
                if (valorOriginal > 1) operacionesDisponibles.Add("-1");
                if (valorOriginal < 50) operacionesDisponibles.Add("+1");
                if (valorOriginal > 1 && valorOriginal < 50) operacionesDisponibles.Add("*2");

                // Aplicar Cada una de las Operaciones Disponibles
                foreach (string op in operacionesDisponibles)
                {
                    // Aplicar Operacion
                    if (op == "+1") secuenciaActual[ind] += 1;
                    else if (op == "-1") secuenciaActual[ind] -= 1;
                    else if (op == "*2") secuenciaActual[ind] *= 2;
                    else if (op == "/2") secuenciaActual[ind] /= 2;
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

                            int valorDespues = secuenciaActual[ind];

                            mejorMemoria.Operaciones.Add((ind, op, valorOriginal, valorDespues));
                            mejorMemoria.Operaciones.AddRange(subMemoria.Operaciones);
                            mejorMemoria.Costos.Add(costoOperacion);
                            mejorMemoria.Costos.AddRange(subMemoria.Costos);
                            mejorMemoria.Secuencia = new List<int>(subMemoria.Secuencia);

                        }

                    }

                    // Devolver el Valor Original
                    secuenciaActual[ind] = valorOriginal;

                }

            }

            // Guardar en Memoria antes de Terminar
            if (mejorMemoria != null)
            {
                estadoMemoria[estadoActual] = mejorMemoria;
            }
            else
            {
                estadosFallidos.Add(estadoActual);
            }

            // Terminar la Recursion
            return mejorMemoria;

        }

        // Verificar si la Secuencia esta Ordenada Ascendentemente
        private bool OrdenadaAscendente(List<int> lista)
        {
            for (int i = 0; i < lista.Count - 1; i++)
            {
                if (lista[i] >= lista[i + 1]) return false;
            }
            return true;
        }

    }

    // Clase para Guardar Memoria
    public class Memoria
    {
        public List<(int indice, string operacion, int valorAntes, int valorDespues)> Operaciones { get; set; } = new List<(int, string, int, int)>();
        public List<int> Costos { get; set; } = new List<int>();
        public List<int> Secuencia { get; set;  } = new List<int>();
    }

}
