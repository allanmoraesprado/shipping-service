using System.ServiceProcess;
using System.Timers;
using BLL;

namespace SinoTotalExpress
{
    public partial class Integrador : ServiceBase
    {
        private Timer timer;

        public Integrador()
        {
            InitializeComponent();

            timer = new Timer(600000);
            timer.Elapsed += timer_Elapsed;
        }

        protected override void OnStart(string[] args)
        {
            timer.Start();
        }

        protected override void OnStop()
        {
            timer.Stop();
        }

        protected void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ((Timer)sender).Stop();
            ((Timer)sender).Enabled = false;
            try
            {
                new Processar().EnviarPedidos();
            }
            catch
            {
            }
            finally
            {
            }
            ((Timer)sender).Enabled = true;
            ((Timer)sender).Start();
        }
    }
}
