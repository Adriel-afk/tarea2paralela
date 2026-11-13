
using System;
using System.Collections.Generic;
using System.Threading;

class Contador
{
    private Thread hilo;
    private volatile bool ejecutando;
    private int valor;
    private readonly int intervalo;
    private readonly object bloqueo = new object();

    public int Id { get; private set; }

    public Contador(int id, int intervalo)
    {
        Id = id;
        this.intervalo = intervalo;
        valor = 0;
    }

    public bool EstaActivo
    {
        get { return ejecutando; }
    }

    public int ObtenerValor()
    {
        lock (bloqueo)
        {
            return valor;
        }
    }

    public void Iniciar()
    {
        if (ejecutando)
        {
            Console.WriteLine("El contador " + Id + " ya esta activo.");
            return;
        }

        ejecutando = true;
        hilo = new Thread(Contar);
        hilo.IsBackground = true;
        hilo.Start();

        Console.WriteLine("Contador " + Id + " iniciado.");
    }

    private void Contar()
    {
        while (ejecutando)
        {
            Thread.Sleep(intervalo);

            if (!ejecutando)
                break;

            int actual;

            lock (bloqueo)
            {
                valor++;
                actual = valor;
            }

            lock (Program.bloqueoConsola)
            {
                Console.WriteLine("[Contador " + Id + "] Valor: " + actual);
            }
        }
    }

    public void Detener()
    {
        if (!ejecutando)
        {
            Console.WriteLine("El contador " + Id + " ya esta detenido.");
            return;
        }

        ejecutando = false;

        if (hilo != null && hilo.IsAlive)
        {
            hilo.Join();
        }

        Console.WriteLine("Contador " + Id + " detenido. Valor final: " + ObtenerValor());
    }
}

class Program
{
    public static readonly object bloqueoConsola = new object();

    static void Main()
    {
        List<Contador> contadores = new List<Contador>();

        contadores.Add(new Contador(1, 500));
        contadores.Add(new Contador(2, 1000));
        contadores.Add(new Contador(3, 1500));

        int opcion = 0;

        do
        {
            Console.WriteLine("\n======================================");
            Console.WriteLine("       CONTADORES SIMULTANEOS");
            Console.WriteLine("======================================");
            Console.WriteLine("1. Iniciar un contador");
            Console.WriteLine("2. Detener un contador");
            Console.WriteLine("3. Mostrar estado de los contadores");
            Console.WriteLine("4. Iniciar todos los contadores");
            Console.WriteLine("5. Detener todos los contadores");
            Console.WriteLine("6. Salir");
            Console.WriteLine("======================================");
            Console.Write("Seleccione una opcion: ");

            if (!int.TryParse(Console.ReadLine(), out opcion))
            {
                opcion = 0;
            }

            switch (opcion)
            {
                case 1:
                    SeleccionarContador(contadores, true);
                    break;

                case 2:
                    SeleccionarContador(contadores, false);
                    break;

                case 3:
                    MostrarEstado(contadores);
                    break;

                case 4:
                    foreach (Contador contador in contadores)
                    {
                        contador.Iniciar();
                    }
                    break;

                case 5:
                    foreach (Contador contador in contadores)
                    {
                        contador.Detener();
                    }
                    break;

                case 6:
                    foreach (Contador contador in contadores)
                    {
                        contador.Detener();
                    }

                    Console.WriteLine("Todos los hilos fueron detenidos.");
                    Console.WriteLine("Programa finalizado.");
                    break;

                default:
                    Console.WriteLine("Opcion no valida.");
                    break;
            }

        } while (opcion != 6);
    }

    static void SeleccionarContador(List<Contador> contadores, bool iniciar)
    {
        Console.Write("Ingrese el numero del contador (1, 2 o 3): ");

        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Numero no valido.");
            return;
        }

        if (id < 1 || id > contadores.Count)
        {
            Console.WriteLine("Ese contador no existe.");
            return;
        }

        if (iniciar)
        {
            contadores[id - 1].Iniciar();
        }
        else
        {
            contadores[id - 1].Detener();
        }
    }

    static void MostrarEstado(List<Contador> contadores)
    {
        Console.WriteLine("\n========== ESTADO ACTUAL ==========");

        foreach (Contador contador in contadores)
        {
            string estado = contador.EstaActivo ? "ACTIVO" : "DETENIDO";

            Console.WriteLine(
                "Contador " + contador.Id +
                " | Valor: " + contador.ObtenerValor() +
                " | Estado: " + estado
            );
        }
    }
}
