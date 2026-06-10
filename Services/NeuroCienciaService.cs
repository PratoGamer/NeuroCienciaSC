using System;
using System.Collections.Generic;
using System.Text;

namespace NeuroCienciaSC.Services
{
    // Clase Principal del Servicio de NeuroCiencia
    public class NeuroCienciaService
    {
        // Lista para Almacenar la Secuencia de Numeros Originales
        public List<int> secuencia { get; set; } = new List<int>();

        // Diccionario para los Costos de Operacion
        public Dictionary<string, int> operacion { get; set; } = new Dictionary<string, int>()
        {
            { "+1", 1 },
            { "-1", 1 },
            { "*2", 3 },
            { "/2", 2 }
        };

        // Resultado Ascendente
        public List<(int indice, string operacion, int valorAntes, int valorDespues)> secuenciaOrdenadaAscendente { get; set; } = new List<(int, string, int, int)>();
        public List<int> costosAscendente { get; set; } = new List<int>();
        public int SumarCostosAscendente() => costosAscendente.Sum();
        public List<int> secuenciaFinalAscendente { get; set; } = new List<int>();

        // Resuldado Descendente
        public List<(int indice, string operacion, int valorAntes, int valorDespues)> secuenciaOrdenadaDescendente { get; set; } = new List<(int, string, int, int)>();
        public List<int> costosDescendente { get; set; } = new List<int>();
        public int SumarCostosDescendente() => costosDescendente.Sum();
        public List<int> secuenciaFinalDescendente { get; set; } = new List<int>();

        // Cual es Mejor Costo Encontrado
        public string mejorOrden { get; set; } = string.Empty;

        // Mejor Costo Encontrado
        private int mejorCostoAscendente;
        private int mejorCostoDescendente;

        // Poda de Profundidad
        private int poda = 60;

        // Metodo para Ordenar de Forma Ascendente
        public async Task CalcularOrdenMinimo()
        {
            // Limpiar todos los Valores Ascendentes
            secuenciaOrdenadaAscendente.Clear();
            costosAscendente.Clear();
            mejorCostoAscendente = int.MaxValue;
            secuenciaFinalAscendente.Clear();

            // Limpiar todos los Valores Descendentes
            secuenciaOrdenadaDescendente.Clear();
            costosDescendente.Clear();
            mejorCostoDescendente = int.MaxValue;
            secuenciaFinalDescendente.Clear();

            // Limpiar el Mejor Orden
            mejorOrden = string.Empty;

            // Instacia de Memorias
            var memoriaAscendente = new Dictionary<string, Memoria>();
            var memoriaDescendente = new Dictionary<string, Memoria>();

            // Instancia de Estados Fallidos
            var estadosFallidosAscendente = new HashSet<string>();
            var estadosFallidosDescendente = new HashSet<string>();

            // Verificar que Secuencia no este Vacio o con un Solo Valor
            if (secuencia == null || secuencia.Count <= 1) return;

            // Copias de la Secuencia Original
            List<int> copiaAscendente = new List<int>(secuencia);
            List<int> copiaDescendente = new List<int>(secuencia);

            // Usar el Algortimos Recursivos con Hilos en Paralelo
            Task<Memoria> tareaAscendente = Task.Run(() => ResolverAscendente(copiaAscendente, 0, memoriaAscendente, estadosFallidosAscendente));
            Task<Memoria> tareaDescendente = Task.Run(() => ResolverDescendente(copiaDescendente, 0, memoriaDescendente, estadosFallidosDescendente));

            // Esperar a que Ambas Tareas Terminen a la Vez
            await Task.WhenAll(tareaAscendente, tareaDescendente);

            // Guardar Soluciones de las Tareas
            Memoria solucionAscendente = await tareaAscendente;
            Memoria solucionDescendente = await tareaDescendente;

            // Guardar la Soluciones Optimas
            if (solucionAscendente != null)
            {
                secuenciaOrdenadaAscendente = solucionAscendente.Operaciones;
                costosAscendente = solucionAscendente.Costos;
                secuenciaFinalAscendente = solucionAscendente.Secuencia;
            }
            if (solucionDescendente != null)
            {
                secuenciaOrdenadaDescendente = solucionDescendente.Operaciones;
                costosDescendente = solucionDescendente.Costos;
                secuenciaFinalDescendente = solucionDescendente.Secuencia;
            }

            // Comparacion de Estrategias
            int costoTotalAscendente = solucionAscendente != null ? solucionAscendente.Costos.Sum() : int.MaxValue;
            int costoTotalDescendente = solucionDescendente != null ? solucionDescendente.Costos.Sum() : int.MaxValue;

            // Determinar el Mejor Orden
            if (costoTotalAscendente == int.MaxValue && costoTotalDescendente == int.MaxValue)
            {
                mejorOrden = "Ninguna (Excede Limites)";
            }
            else if (costoTotalAscendente < costoTotalDescendente)
            {
                mejorOrden = "Ascendente";
            }
            else if (costoTotalDescendente < costoTotalAscendente)
            {
                mejorOrden = "Descendente";
            }
            else
            {
                mejorOrden = "Ambos (Empate)";
            }
            
        }

        // Metodo Recursivo con Memorizacion Ascendente
        private Memoria ResolverAscendente(List<int> secuenciaActual, int costoAcumulado, Dictionary<string, Memoria> memoriaAscendente, HashSet<string> estadosFallidosAscendente)
        {
            // PODA del Costo para no Desbordar Memoria
            if (costoAcumulado >= mejorCostoAscendente) return null;

            // Transformar a un String
            string estado = string.Join(",", secuenciaActual);

            // Verificar los Estados Fallidos
            if (estadosFallidosAscendente != null && estadosFallidosAscendente.Contains(estado)) return null;

            // Memorizacion Verificar si el Estado ya fue Calculado
            if (memoriaAscendente != null && memoriaAscendente.ContainsKey(estado))
            {
                return memoriaAscendente[estado];
            }

            // Secuencia ya Ordenada Ascendentemente
            if (OrdenadaAscendente(secuenciaActual))
            {
                // Guardar el Mejor Costo
                mejorCostoAscendente = costoAcumulado;
                
                // Devolver la Misma Secuencia
                return new Memoria
                {
                    Operaciones = new List<(int, string, int, int)>(),
                    Costos = new List<int>(),
                    Secuencia = new List<int>(secuenciaActual)
                };
            }

            // PODA de Profundidad Maximo de Costo
            if (costoAcumulado > poda)
            {
                estadosFallidosAscendente.Add(estado);
                return null;
            }

            // Empezar la Recursion
            Memoria mejorMemoria = null;
            int menorCosto = int.MaxValue;

            // Lista para Guardar Elementos Desordenados Ascendete
            List<int> indices = ObtenerIndices(secuenciaActual, true);

            // Recorrer Solo los Indices para Operar
            foreach (int ind in indices)
            {
                // Almacenar el Valor Original
                int valorOriginal = secuenciaActual[ind];

                // Aplicar Cada una de las Operaciones Disponibles
                foreach (string op in ObtenerOperaciones(valorOriginal))
                {
                    // Aplicar Operacion
                    AplicarOperacion(secuenciaActual, ind, op);

                    // Usar la Recursion de Memoria
                    Memoria subMemoria = ResolverAscendente(secuenciaActual, costoAcumulado + operacion[op], memoriaAscendente, estadosFallidosAscendente);

                    // Verificar Resultado de la SubMemoria
                    if (subMemoria != null)
                    {
                        // Obtener el Costo Total de esta Ruta
                        int costoTotal = operacion[op] + subMemoria.Costos.Sum();

                        // Verificar el Menor Costo
                        if (costoTotal < menorCosto)
                        {
                            menorCosto = costoTotal;

                            // Construir la Solucion
                            mejorMemoria = new Memoria();
                            mejorMemoria.Operaciones.Add((ind, op, valorOriginal, secuenciaActual[ind]));
                            mejorMemoria.Operaciones.AddRange(subMemoria.Operaciones);
                            mejorMemoria.Costos.Add(operacion[op]);
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
                memoriaAscendente[estado] = mejorMemoria;
            }
            else
            {
                estadosFallidosAscendente.Add(estado);
            }

            // Terminar la Recursion
            return mejorMemoria;

        }

        // Metodo Recursivo con Memorizacion Desendente
        private Memoria ResolverDescendente(List<int> secuenciaActual, int costoAcumulado, Dictionary<string, Memoria> memoriaDescendente, HashSet<string> estadosFallidosDescendente)
        {
            // PODA del Costo para no Desbordar Memoria
            if (costoAcumulado >= mejorCostoDescendente) return null;

            // Transformar a un String
            string estado = string.Join(",", secuenciaActual);

            // Verificar los Estados Fallidos
            if (estadosFallidosDescendente != null && estadosFallidosDescendente.Contains(estado)) return null;

            // Memorizacion Verificar si el Estado ya fue Calculado
            if (memoriaDescendente != null && memoriaDescendente.ContainsKey(estado)) return memoriaDescendente[estado];

            // Secuencia ya Ordenada Descendentemente
            if (OrdenadaDescendente(secuenciaActual))
            {
                // Guardar el Mejor Costo
                mejorCostoDescendente = costoAcumulado;
                
                // Devolver la Misma Secuencia
                return new Memoria
                {
                    Operaciones = new List<(int, string, int, int)>(),
                    Costos = new List<int>(),
                    Secuencia = new List<int>(secuenciaActual)
                };
            }

            // PODA de Profundidad Maximo de Costo
            if (costoAcumulado > poda)
            {
                estadosFallidosDescendente.Add(estado);
                return null;
            }

            // Empezar la Recursion
            Memoria mejorMemoria = null;
            int menorCosto = int.MaxValue;

            // Lista para Guardar Elementos Desordenados Ascendete
            List<int> indices = ObtenerIndices(secuenciaActual, false);

            // Recorrer Solo los Indices para Operar
            foreach (int ind in indices)
            {
                // Almacenar el Valor Original
                int valorOriginal = secuenciaActual[ind];

                // Aplicar Cada una de las Operaciones Disponibles
                foreach (string op in ObtenerOperaciones(valorOriginal))
                {
                    // Aplicar Operacion
                    AplicarOperacion(secuenciaActual, ind, op);

                    // Usar la Recursion de Memoria
                    Memoria subMemoria = ResolverDescendente(secuenciaActual, costoAcumulado + operacion[op], memoriaDescendente, estadosFallidosDescendente);

                    // Verificar Resultado de la SubMemoria
                    if (subMemoria != null)
                    {
                        // Obtener el Costo Total de esta Ruta
                        int costoTotal = operacion[op] + subMemoria.Costos.Sum();

                        // Verificar el Menor Costo
                        if (costoTotal < menorCosto)
                        {
                            menorCosto = costoTotal;

                            // Construir la Solucion
                            mejorMemoria = new Memoria();
                            mejorMemoria.Operaciones.Add((ind, op, valorOriginal, secuenciaActual[ind]));
                            mejorMemoria.Operaciones.AddRange(subMemoria.Operaciones);
                            mejorMemoria.Costos.Add(operacion[op]);
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
                memoriaDescendente[estado] = mejorMemoria;
            }
            else
            {
                estadosFallidosDescendente.Add(estado);
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

        // Verificar si la Secuencia esta Ordenada Descendentemente
        private bool OrdenadaDescendente(List<int> lista)
        {
            for (int i = 0; i < lista.Count - 1; i++)
            {
                if (lista[i] <= lista[i + 1]) return false;
            }
            return true;

        }

        // Obtener los Indices de los Elementos Desordenados
        private List<int> ObtenerIndices(List<int> secuencia, bool ascendente)
        {
            // Crear una Lista Auxiliar
            List<int> indices = new List<int>();

            // Calcular el Orden Ascendente o Descendente
            for (int i = 0; i < secuencia.Count; i++)
            {
                // Si Necessita Modificar
                bool modificacion = false;

                // Verificar si el Elemento a la Izquierda esta Desordenado
                for (int j = 0; j < i; j++)
                {
                    if (ascendente)
                    {
                        if (secuencia[j] >= secuencia[i])
                        {
                            modificacion = true;
                            break;
                        }
                    }
                    else
                    {
                        if (secuencia[j] <= secuencia[i])
                        {
                            modificacion = true;
                            break;
                        }
                    }
                }

                // Verificar si el Elemento a la Derecha esta Desordenado
                if (!modificacion)
                {
                    for (int j = i + 1; j < secuencia.Count; j++)
                    {
                        if (ascendente)
                        {
                            if (secuencia[i] >= secuencia[j])
                            {
                                modificacion = true;
                                break;
                            }
                        }
                        else
                        {
                            if (secuencia[i] <= secuencia[j])
                            {
                                modificacion = true;
                                break;
                            }
                        }
                    }
                }

                // Agregar a la Lista si es Modificacion
                if (modificacion) indices.Add(i);

            }

            if (indices.Count == 0)
            {
                return Enumerable.Range(0, secuencia.Count).ToList();
            }
            else
            {
                return indices;
            }

        }

        // Obtener la Lista de Operaciones Disponibles
        private List<string> ObtenerOperaciones(int valor)
        {
            // Variable Auxiliar
            List<string> operacionesDisponibles = new List<string>();

            // Operaciones a Aplicar
            if (valor % 2 == 0 && valor > 1) operacionesDisponibles.Add("/2");
            if (valor > 1) operacionesDisponibles.Add("-1");
            if (valor < 50) operacionesDisponibles.Add("+1");
            if (valor > 1 && valor < 50) operacionesDisponibles.Add("*2");

            // Devolver la Lista
            return operacionesDisponibles;
        } 

        // Aplicar la Operacion Disponible
        private void AplicarOperacion(List<int> lista, int indice, string operacion)
        {
            // Aplicar la Operacion Correspondiente
            if (operacion == "+1") lista[indice] += 1;
            else if (operacion == "-1") lista[indice] -= 1;
            else if (operacion == "*2") lista[indice] *= 2;
            else if (operacion == "/2") lista[indice] /= 2;

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
