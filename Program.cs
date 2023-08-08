using System;
using System.Collections.Generic;
using System.Threading;


public interface ITaskManager
{
    void AgregarTarea(string descripcion);
    void ActualizarTarea(int taskId, string nuevaDescripcion);
    void EliminarTarea(int taskId);
    List<TaskItem> TareasPendientes();
    List<TaskItem> TareasCompletadas();
}

public interface IUserInterface
{
    void MostrarMenu();
    void MostrarTareas(List<TaskItem> tasks);
}


public class TaskItem
{
    public int Id { get; set; }
    public string Descripcion { get; set; }
    public bool Completada { get; set; }
}

public class TaskManager : ITaskManager
{
    private List<TaskItem> tasks = new List<TaskItem>();
    private object lockObj = new object();

    public void AgregarTarea(string descripcion)
    {
        lock (lockObj)
        {
            int newId = tasks.Count + 1;
            tasks.Add(new TaskItem { Id = newId, Descripcion = descripcion, Completada = false });
        }
    }

    public void ActualizarTarea(int taskId, string nuevaDescripcion)
    {
        lock (lockObj)
        {
            TaskItem tareaActualizar = tasks.Find(task => task.Id == taskId);
            if (tareaActualizar != null)
            {
                tareaActualizar.Descripcion = nuevaDescripcion;
            }
        }
    }

    public void EliminarTarea(int taskId)
    {
        //bloquearobjeto
        lock (lockObj)
        {
            tasks.RemoveAll(task => task.Id == taskId);
        }
    }

    public List<TaskItem> TareasPendientes()
    {
        lock (lockObj)
        {
            return tasks.FindAll(task => !task.Completada);
        }
    }

    public List<TaskItem> TareasCompletadas()
    {
        lock (lockObj)
        {
            return tasks.FindAll(task => task.Completada);
        }
    }
}

public class ConsoleUserInterface : IUserInterface
{
    public void MostrarMenu()
    {
        Console.WriteLine("TAREAS");
        Console.WriteLine("");
        Console.WriteLine("1. Agregar Tarea");
        Console.WriteLine("2. Actualizar Tarea");
        Console.WriteLine("3. Eliminar Tarea");
        Console.WriteLine("4. Mostrar Tareas pendientes");
        Console.WriteLine("5. Mostrar tareas completadas");
        Console.WriteLine("6. Cerrar");
    }

    public void MostrarTareas(List<TaskItem> tareas)
    {
        foreach (var tarea in tareas)
        {
            Console.WriteLine($"{tarea.Id}. {(tarea.Completada ? "[Completada]" : "[Pendiente]")} {tarea.Descripcion}");
        }
    }
}

public class Program
{
    static void Main(string[] args)
    {
        ITaskManager taskManager = new TaskManager();
        IUserInterface userInterface = new ConsoleUserInterface();

        bool exit = false;

        while (!exit)
        {
            userInterface.MostrarMenu();
            Console.Write("Ingrese una opción: ");
            int choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    Console.Write("Ingrese Descripcion: ");
                    string descripcion = Console.ReadLine();
                    taskManager.AgregarTarea(descripcion);
                    break;
                case 2:
                    Console.Write("Ingrese el ID de la tarea a actualizar: ");
                    int taskId = Convert.ToInt32(Console.ReadLine());
                    Console.Write("Ingrese nueva descripción de la tarea: ");
                    string nuevaDescripcion = Console.ReadLine();

                  
                    Thread updateThread = new Thread(() => taskManager.ActualizarTarea(taskId, nuevaDescripcion));
                    updateThread.Start();
                    break;
                case 3:
                    Console.Write("Ingrese el ID de la tarea a eliminar: ");
                    int taskIdToDelete = Convert.ToInt32(Console.ReadLine());

        
                    Thread deleteThread = new Thread(() => taskManager.EliminarTarea(taskIdToDelete));
                    deleteThread.Start();
                    break;
                case 4:
                    List<TaskItem> tareasPendientes = taskManager.TareasPendientes();
                    userInterface.MostrarTareas(tareasPendientes);
                    break;
                case 5:
                    List<TaskItem> tareasCompletadas = taskManager.TareasCompletadas();
                    userInterface.MostrarTareas(tareasCompletadas);
                    break;
                case 6:
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Opción Incorrecta, intente otra vez.");
                    break;
            }
        }
    }
}
