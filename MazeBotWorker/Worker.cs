using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MazeBotWorker
{
    public class Worker : IHostedService
    {
        private MazeBot.Bot bot;

        public Worker()
        {
            /*  
             *  ������� AppHarbor ������ ������������� ���� �� ����� .config, 
             *  ���� ����� �� ��������������� � .Net Core
             *  ����� �� ���������� �������� ����� �� ��������� Regex-������
             */

            var regex = new Regex("\"TelegramBotApiToken\" value=\"(.+)\"");

            var match = regex.Match(File.ReadAllText("MazeBotWorker.dll.config"));

            bot = new MazeBot.Bot(match.Groups[1].Value);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            bot.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            bot.Stop();

            return Task.CompletedTask;
        }
    }
}
