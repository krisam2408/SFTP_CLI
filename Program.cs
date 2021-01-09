using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFTPTest
{
    class Program
    {
        static SFTPService Sftp { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine($"Conexión a SFTP {Config.Instance.Host}.");
            Underline(2, true);

            Sftp = new SFTPService(Config.Instance.Host, Config.Instance.Port, Config.Instance.User, Config.Instance.Pass);
            
            string op;

            do
            {
                Menu();

                op = Console.ReadLine();

                switch(op)
                {
                    case "0":
                        Posicion();
                        Underline(2, true);
                        break;
                    case "1":
                        Listar();
                        Console.WriteLine("");
                        Underline(2, true);
                        break;
                    case "2":
                        Navegar();
                        Underline(1);
                        Posicion();
                        Underline(2, true);
                        break;
                    case "3":
                        Cargar();
                        Underline(2, true);
                        break;
                    case "4":
                        Descargar();
                        Underline(2, true);
                        break;
                    case "5":
                        Eliminar();
                        Underline(2, true);
                        break;
                    case "6":
                        break;
                    default:
                        Underline(1);
                        Console.WriteLine("Opción no válida.");
                        Underline(1, true);
                        break;
                }

            } while (op != ((int)Opcion.Salir).ToString());

            Underline(1);
            Console.WriteLine("Cerrando Consola");
            Console.WriteLine("Pulse Tecla para salir.");
            Underline(1);
            Console.ReadKey();

        }

        static void Underline(int times, bool afterSpace = false)
        {
            for(int i = 0; i < times; i++)
            {
                Console.WriteLine("".PadLeft(36,'-'));
            }
            if(afterSpace) Console.WriteLine("");
        }

        static void Menu()
        {
            Console.WriteLine("MENU");
            Underline(2);
            foreach(Opcion op in Enum.GetValues(typeof(Opcion)))
            {
                Console.WriteLine($"{(int)op}.-  {op}");
            }
            Underline(2, true);
        }

        static void Posicion()
        {
            Console.WriteLine($"Posicion actual: {Sftp.CurrentDirectory}.");
        }

        static void Listar()
        {
            Console.WriteLine($"Listar archivos de {Sftp.CurrentDirectory}.");
            Underline(1);
            ListFiles(Sftp.ListDirectoryFiles());
        }

        static void Navegar()
        {
            List<SftpFile> dirList = Sftp.ListDirectoryFiles()
                .Where(f => f.IsDirectory).ToList();

            if(dirList.Count > 0)
            {
                Console.WriteLine($"Listado de directorios en {Sftp.CurrentDirectory}.");
                Underline(1);
                ListFiles(dirList);

                Console.WriteLine("");
                Console.WriteLine("Escriba el directorio al que desee ingresar.");
                string dirName = Console.ReadLine();

                if (dirName == "..") Sftp.ChangeDirectory(dirName);

                foreach(SftpFile f in dirList)
                {
                    if(dirName == f.Name)
                    {
                        Sftp.ChangeDirectory(f);
                    }
                }
            }
            else
            {
                Console.WriteLine("El directorio actual no contiene directorios.");
                Underline(1, true);
            }
        }

        static void Cargar()
        {
            Console.WriteLine($"Estamos considerando el directorio local {Config.Instance.Destination} para la búsquda de archivos.");
            Console.WriteLine("Por favor digite el nombre de archivo que desee cargar al servidor SFTP.");

            string fileName = Console.ReadLine();
            string path = $"{Config.Instance.Destination}/{fileName}";
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if (Sftp.Upload(fs, fileName, true)) Console.WriteLine($"Se ha cargado {fileName} exitosamente.");
                else Console.WriteLine("No se ha podido cargar el archivo");
            }
        }

        static void Descargar()
        {
            List<SftpFile> fileList = Sftp.ListDirectoryFiles()
                .Where(f => !f.IsDirectory).ToList();

            if (fileList.Count() > 0)
            {
                Console.WriteLine($"Listado de archivos en {Sftp.CurrentDirectory}.");
                Underline(1);
                ListFiles(fileList);

                Console.WriteLine("");
                Console.WriteLine("Escriba el archivo que desee descargar.");
                string fileName = Console.ReadLine();

                foreach (SftpFile f in fileList)
                {
                    if (fileName == f.Name)
                    {
                        if (Sftp.Download(f, Config.Instance.Destination)) Console.WriteLine($"Se ha descargado el archivo {fileName}.");
                        else Console.WriteLine("No se ha podido descargar el archivo.");
                    }
                }
            }
            else
            {
                Console.WriteLine("El directorio actual no contiene archivos.");
                Underline(1, true);
            }
        }

        static void Eliminar()
        {
            List<SftpFile> fileList = Sftp.ListDirectoryFiles()
                .Where(f => !f.IsDirectory).ToList();

            if(fileList.Count() > 0)
            {
                Console.WriteLine($"Listado de archivos en {Sftp.CurrentDirectory}.");
                Underline(1);
                ListFiles(fileList);

                Console.WriteLine("");
                Console.WriteLine("Escriba el archivo que desee eliminar.");
                string fileName = Console.ReadLine();

                foreach (SftpFile f in fileList)
                {
                    if (fileName == f.Name)
                    {
                        if (Sftp.Delete(f)) Console.WriteLine($"Se ha eliminado el archivo {fileName}.");
                        else Console.WriteLine("No se ha podido eliminar el archivo.");
                    }
                }
            }
            else
            {
                Console.WriteLine("El directorio actual no contiene archivos.");
                Underline(1, true);
            }
        }

        static void ListFiles(IEnumerable<SftpFile> list)
        {
            if(list.Count() > 0)
            {
                foreach (SftpFile f in list)
                {
                    string type = f.IsDirectory ? "Directorio" : "Archivo";
                    Console.WriteLine($" - {f.Name,-20}{type}");
                }
            }
            else
            {
                Console.WriteLine("No se han encontrado registros...");
            }
        }
    }
}
