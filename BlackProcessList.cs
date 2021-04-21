using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using System.Timers;
using Microsoft.Win32;

/*
 ***************************************************************************** COMENTARIOS DEL EJERCICIO
 * 
 * 1. Tipo de aplicación SERVICIO DE WINDOWS (.NET FRAMEWORK).
 * 
 * 2. En el archivo BlackProcessList.cs [Diseño] configuramos:
 *      Autolog: true
 *      CanHandlePowerEvent: TRUE
 *      ...
 *      debe detectar todos los eventos del sistema TRUE
 * 
 *      clic derecho y agregamos el instalador. Se crean los componentes "serviceInstaller1" y "serviceProcessInstaller1" en el archivo ProjectInstaller.cs [Diseño]
 * 
 * 3. En el archivo ProjectInstaller.cs [Diseño] se escribe el nombre que tendrá el servicio en el sistema en el componente "serviceInstaller1". En este caso, "DAM - BlackProcessList".
 * También se configura como un servicio local (LocalSystem) en el componente "serviceProcessInstaller1".
 * 
 * 4. Escribimos el código del servicio
 * 
 * 5. Compilamos y el archivo .exe lo copiamos a la ruta especificada del código. También necesitamos los scripts InstalarServei.cmd para instalar 
 * el servicio al sistema y DesinstalarServei.cmd para desinstalarlo. En el archivo InstalarServei.cmd debe especificarse la ruta del servicio y 
 * se debe ejecutar como administrador. Además, se han generado 2 archivos más "ServicioDam.InstallLog" y "ServicioDam.InstallState".
 * 
 * Cuando se instala se observa el servicio detenido. Se debe al StartType del "serviceInstaller1" que figura como manual.
 * 
 * 6. A continuación, iniciamos el servicio en SERVICIOS.
 * 
 * 7. Es importante debugar con las aplicaciones VISOR DE EVENTOS y SERVICIOS
 *      En VISOR DE EVENTOS se registran las anotaciones en REGISTROS DE WINDOWS - APLICACIÓN
 *      En SERVICIOS buscaremos el nombre del servicio en este caso DAM - BlackProcessList
 *      
 * CONTENIDO del archivo blackProcessList.txt
 * notepad
 * calculator
 * 
 * NOTA IMPORTANTE:
 * 
 * Eliminar un servicio
 * 
 * En SERVICIOS consultamos el nombre del servicio que queremos eliminar. Por ejemplo "DAM - BlackProcessList"
 * Abrimos la consola y ejecutamos el comando: sc delete "DAM - BlackProcessList" 
 * Debe eliminarse la misma versión del .exe que se instaló.
 * 
*/

namespace ServicioDam
{
    public partial class BlackProcessList : ServiceBase
    {
        readonly string sourcePath = @"C:\SERVICIOS";
        readonly string fileName = "blackProcessList.txt";

        List<string> blackProcessList = new List<string>();

        Timer tm = new Timer();

        public BlackProcessList()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry("ServicioDam (BlackList): OnStart", "Activamos el servicio", EventLogEntryType.Information);

            // Colocando esta función al timer, garantizamos que cualquier modificación de la lista en tiempo de ejecución del servicio se añadirá
            // a la lista. Puede probarse añadiendo mspaint al archivo...
            // IniciarServicio();
            
            tm.Interval=5000;
            tm.Elapsed+=new ElapsedEventHandler(ComentarEvent);
            tm.Start();
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry("ServicioDam (BlackList): OnStop", "Desactivamos el servicio", EventLogEntryType.Information);
        }

        private void IniciarServicio()
        {
            string sourceFile, line;

            sourceFile = Path.Combine(sourcePath, fileName);
            System.IO.StreamReader file = new System.IO.StreamReader(sourceFile);

            while((line=file.ReadLine())!=null)
            {
                if(line.Trim() != "")
                {
                    EventLog.WriteEntry("ServicioDam (BlackList): IniciarServicio", $"linia: {line}", EventLogEntryType.Information);

                    blackProcessList.Add(line);
                }
            }

            file.Close();
        }

        private void ComentarEvent(object sender, ElapsedEventArgs e)
        {
            // Distintos procedimientos para detectar, cerrar, etc. un servicio según la API .NET
            // https://docs.microsoft.com/es-es/dotnet/api/system.diagnostics.process.getprocesses?view=net-5.0

            IniciarServicio();

            foreach(string blackProcess in blackProcessList)
            {
                EventLog.WriteEntry("ServicioDam (BlackList): ComentarEvent", $"{blackProcess}", EventLogEntryType.Information);

                try
                {
                    foreach(Process process in Process.GetProcessesByName(blackProcess))
                    {
                        EventLog.WriteEntry("ServicioDam (BlackList): ComentarEvent", $"{process} killing...", EventLogEntryType.Information);
                        process.Kill();
                    }
                }
                catch(Exception ex)
                {
                    EventLog.WriteEntry("ServicioDam (BlackList): ComentarEvent", $"No se ha cerrado el proceso: {ex.Message}", EventLogEntryType.Information);
                }
            }
        }
    }
}
