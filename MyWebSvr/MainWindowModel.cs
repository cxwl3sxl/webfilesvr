using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using PinFun.Core.Net.Http;
using PinFun.Core.ServiceHost.WebApi.Middleware;
using PinFun.Wpf;
using PinFun.Wpf.Controls;

namespace MyWebSvr
{
    public class MainWindowModel : ViewModelBase
    {
        private HttpServer _httpServer;

        public ICommand StartCommand { get; set; }
        public ICommand RemovePathCommand { get; set; }
        public ICommand AddPathCommand { get; set; }
        public ICommand CopyCommand { get; set; }

        public int Port
        {
            get => GetValue(() => Port);
            set => SetValue(() => Port, value);
        }

        public string CommandText
        {
            get => GetValue(() => CommandText);
            set => SetValue(() => CommandText, value);
        }

        public string PendingPath
        {
            get => GetValue(() => PendingPath);
            set => SetValue(() => PendingPath, value);
        }

        public ObservableCollection<string> FilePath
        {
            get => GetValue(() => FilePath);
            set => SetValue(() => FilePath, value);
        }

        public ObservableCollection<string> Urls
        {
            get => GetValue(() => Urls);
            set => SetValue(() => Urls, value);
        }

        public MainWindowModel()
        {
            Port = 80;
            CommandText = "启动";
            Urls = new ObservableCollection<string>();
            FilePath = new ObservableCollection<string>();
            StartCommand = new RelayCommand(Start);
            RemovePathCommand = new RelayCommand<string>(RemovePath);
            AddPathCommand = new RelayCommand(AddPath);
            CopyCommand = new RelayCommand<string>(Copy);
            DiscoverAllUrl();
        }

        void DiscoverAllUrl()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var address in host.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    Urls.Add($"http://{address}{(Port == 80 ? "" : $":{Port}")}");
                }
            }
        }

        async void Start()
        {
            try
            {
                if (CommandText == "启动")
                {
                    await DoStart();
                    CommandText = "停止";
                    ShowToast("服务启动成功", MessageLevel.Success);
                }
                else
                {
                    await DoStop();
                    CommandText = "启动";
                    ShowToast("服务成功停止", MessageLevel.Success);
                }
            }
            catch (Exception ex)
            {
                ShowToast(ex.Message, MessageLevel.Error);
            }
        }

        void Copy(string str)
        {
            str.CopyToClipboard();
            ShowToast("复制成功", MessageLevel.Success);
        }

        Task DoStart()
        {
            return Task.Run(() =>
            {
                SetBusy(true, "正在启动...");
                _httpServer?.Stop();
                _httpServer = new HttpServer(Port, "DefaultWebAipSvr", false, 1024, 6);
                _httpServer.ConfigServer(builder =>
                {
                    builder.Use<WebMiddlewareManager>();
                });
                _httpServer.Start();
                SetBusy(false);
            });
        }

        Task DoStop()
        {
            return Task.Run(() => { _httpServer?.Stop(); });
        }

        async void AddPath()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(PendingPath))
                {
                    var fbd = new FolderBrowserDialog();
                    fbd.ShowDialog();
                    PendingPath = fbd.SelectedPath;
                }
                if (FilePath.Contains(PendingPath)) return;
                FilePath.Add(PendingPath);
                PendingPath = "";
                SetBusy(true, "正在更新...");
                await HtmlPageManager.Instance.BuildIndex(FilePath.ToArray());
                SetBusy(false);
            }
            catch (Exception ex)
            {
                ShowToast(ex.Message, MessageLevel.Error);
            }
        }

        async void RemovePath(string path)
        {
            try
            {
                if (!FilePath.Contains(path)) return;
                FilePath.Remove(path);
                SetBusy(true, "正在更新...");
                await HtmlPageManager.Instance.BuildIndex(FilePath.ToArray());
                SetBusy(false);
            }
            catch (Exception ex)
            {
                ShowToast(ex.Message, MessageLevel.Error);
            }
        }
    }
}
