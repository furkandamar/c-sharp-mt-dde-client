using NDde.Client;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace DDE_TEST
{
    public partial class Form1 : Form
    {
        public class QuoteModel
        {
            public string Symbol { get; set; }
            public decimal Bid { get; set; }
            public decimal Ask { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
        
        public delegate void TickReceived(QuoteModel model);
        public event TickReceived OnTick;

        private DdeClient ddeClient;
        private List<QuoteModel> DataList;

        public Form1()
        {
            InitializeComponent();
            DataList = new List<QuoteModel>
            {
                new QuoteModel { Symbol = "BCHUSD"},
                new QuoteModel { Symbol = "BTCUSD"},
                new QuoteModel { Symbol = "ETHUSD"},
                new QuoteModel { Symbol = "EOSUSD"},
                new QuoteModel { Symbol = "XRPUSD"},
            };

            dataGridView1.DataSource = DataList;

            this.OnTick += Form1_OnTick;
        }

        void Form1_OnTick(QuoteModel model)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new TickReceived(Form1_OnTick), model);
                return;
            }
            dataGridView1.Update();
            dataGridView1.Refresh();
        }

        private void InitializeClient()
        {
            try
            {
                DdeClient client = new DdeClient("MT4", "QUOTE");
                ddeClient = client;

                client.Disconnected += OnDisconnected;
                client.Advise += OnAdvise;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ConnectService()
        {
            if (!ddeClient.IsConnected)
            {
                try
                {
                    ddeClient.Connect();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
           
        }
        private void OnDisconnected(object sender, DdeDisconnectedEventArgs args)
        {
            MessageBox.Show("DDE connection closed");
            ddeClient.Dispose();
        }
        private void Subscribe()
        {
            if ( ddeClient.IsConnected)
            {
                foreach (QuoteModel model in DataList)
                {

                    ddeClient.StartAdvise(model.Symbol, 1, true, 60000);
                }

            }
        }
       

        private void OnAdvise(object sender, DdeAdviseEventArgs args)
        {
            string[] response = (args.Item + " " + args.Text).Split(new char[] { ' ' });

            try
            {
                QuoteModel model = new QuoteModel()
                {
                    Symbol = response[0],
                    Bid = decimal.Parse(response[3]),
                    Ask = decimal.Parse(response[4]),
                    UpdatedAt = DateTime.Parse(response[1] + " " + response[2]),
                };


                int index = DataList.FindIndex(x => x.Symbol == model.Symbol);
                DataList[index] = model;
                if (OnTick != null)
                {
                    OnTick.Invoke(model);
                }
            }

            catch
            {
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeClient();
            ConnectService();
            Subscribe();
        }
    }
}
