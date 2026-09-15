using Microsoft.Extensions.Configuration;
using ROTGBot.Service;
using Npgsql;
using Moq;
using Telegram.BotAPI;
using Telegram.BotAPI.GettingUpdates;
using Microsoft.Extensions.Logging;

namespace XUnitTests
{    
    public class TelegramMainServiceUnitTests 
    {
        private IConfiguration configuration;

        public TelegramMainServiceUnitTests()
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json");
            configuration = builder.Build();
        }

        [Fact]
        public async Task Execute_No_Updates_Async()
        {
            var handlerService = new Mock<ITelegramMessageHandler>();
            var wrapperService = new Mock<ITelegramBotWrapper>();
            var logger = new Mock<ILogger<TelegramMainService>>();
            handlerService.Setup(s => s.HandleUpdates(It.IsAny<IEnumerable<Update>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            wrapperService.Setup(s => s.GetUpdatesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var tgMainService = new TelegramMainService(handlerService.Object, wrapperService.Object, logger.Object);

            var result = await tgMainService.Execute(1);

            Assert.Equal(1, result);
        }

        /// <summary>
        /// 0.0.4.2.1
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SetCommands_Success_Async()
        {
            var handlerService = new Mock<ITelegramMessageHandler>();
            var wrapperService = new Mock<ITelegramBotWrapper>();
            var logger = new Mock<ILogger<TelegramMainService>>();
            handlerService.Setup(s => s.HandleUpdates(It.IsAny<IEnumerable<Update>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            wrapperService.Setup(s => s.GetUpdatesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var tgMainService = new TelegramMainService(handlerService.Object, wrapperService.Object, logger.Object);

            var result = await tgMainService.SetCommands();

            Assert.True(result);
        }

        [Fact]
        public async Task Execute_Exists_Updates_Async()
        {
            var handlerService = new Mock<ITelegramMessageHandler>();
            var wrapperService = new Mock<ITelegramBotWrapper>();
            var logger = new Mock<ILogger<TelegramMainService>>();
            handlerService.Setup(s => s.HandleUpdates(It.IsAny<IEnumerable<Update>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            wrapperService.Setup(s => s.GetUpdatesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                [
                    new Update()
                    {
                        UpdateId = 10
                    }
                ]);

            var tgMainService = new TelegramMainService(handlerService.Object, wrapperService.Object, logger.Object);

            var result = await tgMainService.Execute(10);

            Assert.Equal(11, result);
        }
    }
}