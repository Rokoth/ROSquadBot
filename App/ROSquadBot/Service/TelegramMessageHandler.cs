using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ROTGBot.Contract.Model;
using System.Linq.Dynamic.Core.Tokenizer;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;

namespace ROTGBot.Service
{
    public class TelegramMessageHandler : ITelegramMessageHandler
    {
        private const string HelloMessage = "Привет, {0}! Для работы нажмите кнопку меню - Старт или введите /start";

        private readonly ILogger<TelegramMessageHandler> _logger;
                
        private readonly IUserDataService _userDataService;
        private readonly ITelegramBotWrapper client;

        private readonly int TimeoutSpan = 10;

        public TelegramMessageHandler(
            ILogger<TelegramMessageHandler> logger,
            IUserDataService userDataService,
            IConfiguration configuration,
            ITelegramBotWrapper wrapper)
        {
            _logger = logger;
            _userDataService = userDataService;
            var botSettings = configuration.GetSection("BotSettings").Get<BotSettings>();
            client = wrapper;
        }

        public async Task HandleUpdates(IEnumerable<Update> updates, CancellationToken cancellationToken)
        {            
            ArgumentNullException.ThrowIfNull(updates);

            foreach (var update in updates)
            {
                try
                {
                    await HandleMessage(update.Message, cancellationToken);
                    await HandleCallback(update.CallbackQuery, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке события");
                }
            }
        }

        private async Task HandleMessage(Message? message, CancellationToken cancellationToken)
        {
            if (message == null)
                return;

            _logger.LogInformation("Message update: {name}. {message}", message.Chat.Username, message.Text);

            if (message.From == null)
            {
                await SendTestConnectionMessage(message, "Не удалось получить информацию по отправителю", cancellationToken);
                return;
            }

            if (message.Chat?.Id == null || message.Chat?.Type != "private")
            {
                return;
            }

            var tgUser = message.From;

            var user = await _userDataService.GetOrAddUser(tgUser.Id, tgUser.Username, $"{tgUser.FirstName} {tgUser.LastName} (@{tgUser.Username})", message.Chat.Id, cancellationToken);

            if (user == null)
                return;

            if (message.Text == "/start")
            {
                await StartCommandHandle(user.ChatId, user, "all", cancellationToken);
            }            
            else if (message.IsTopicMessage != true)
            {
                await SendTestConnectionMessage(message, string.Format(HelloMessage, user.Name), cancellationToken);
            }
        }

        private async Task<bool> HandleCallback(CallbackQuery? callbackQuery, CancellationToken token)
        {
            if (callbackQuery == null)
                return false;

            var chatId = callbackQuery.Message?.Chat.Id;
            if (chatId == null)
            {
                return false;
            }

            Contract.Model.User? user = null;

            var tgUser = callbackQuery.From;

            if (callbackQuery.Message?.Chat?.Type != "private")
            {
                user = await _userDataService.GetOrAddUser(tgUser.Id, tgUser.Username, $"{tgUser.FirstName} {tgUser.LastName} (@{tgUser.Username})", null, token);
            }
            else
            {
                user = await _userDataService.GetOrAddUser(tgUser.Id, tgUser.Username, $"{tgUser.FirstName} {tgUser.LastName} (@{tgUser.Username})", chatId.Value, token);
            }
            if (user == null)
            {
                return false;
            }
            var data = callbackQuery.Data;
            if (data == null) return false;
            var result = await HandleData(user.ChatId, user, data, token);
            await client.AnswerCallbackQueryAsync(new AnswerCallbackQueryArgs(callbackQuery.Id), token: token);
            return result;
        }

        private async Task<bool> HandleData(
            long chatId,
            Contract.Model.User user,
            string? dataReq,
            CancellationToken token)
        {
            if (dataReq == null || dataReq == "-") return false;

            var roles = user.Roles;
            var userId = user.Id;

            if (!await CheckRights(user, chatId, RoleEnum.user, token))
                return false;

            switch (dataReq)
            {
                default:
                    await SendUserNotImplemented(chatId, token);
                    break;
            }
            return true;
        }

        private async Task<bool> CheckRights(
            Contract.Model.User user,
            long chatId,
            RoleEnum role,           
            CancellationToken token)
        {                     
            if (!user.Roles.Contains(role))
            {
                await SendUserHasNoRights(chatId, token);
                return false;
            }
            return true;
        }
                
        private async Task SendForwardMessageTitle( NewsCommand userNews, CancellationToken token)
        {
            var user = await _userDataService.GetUser(userNews.UserId, token);
            var tgLogin = !string.IsNullOrEmpty(user.TGLogin) ? $"@{user.TGLogin}" : "Не определен";
            var userName = user.Name ?? "Не определен";
            await client.SendMessageAsync(userNews.GroupId.Value, $"Обращение №{userNews.Number} в раздел \"{userNews.Title}\" от пользователя {userName} (логин: {tgLogin})", (int?)userNews.ThreadId,  token);
        }
                
        private static List<ButtonSetting> ParseButtonsSettings(IEnumerable<NewsCommandMessage> messages)
        {
            var buttons = new List<string>();

            foreach (var message in messages.Where(s => s.TextValue != null))
            {
                var values = message.TextValue?.Split(["\r\n", ";"],
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Where(s => s != null && s != string.Empty);

                if (values?.Any() == true)
                {
                    buttons.AddRange(values);
                }
            }

            var numbers = new List<ButtonSetting>();
            foreach (var item in buttons)
            {
                var itemElements = item.Split(":").Select(s => s.Trim()).ToArray();
                if (int.TryParse(itemElements[0], out int num))
                {
                    string? name = null;
                    int? parent = null;
                    bool isModer = false;
                    if (itemElements.Length > 1)
                    {
                        name = itemElements[1];
                    }
                    if (itemElements.Length > 2)
                    {
                        if (int.TryParse(itemElements[2], out int parNum))
                        {
                            parent = parNum;
                        }
                        else if (itemElements[2] == "m")
                        {
                            isModer = true;
                        }
                    }
                    if (itemElements.Length > 3 && itemElements[3] == "m")
                    {
                        isModer = true;
                    }

                    numbers.Add(new ButtonSetting()
                    {
                        Number = num,
                        Name = name,
                        Parent = parent,
                        IsModerate = isModer
                    });
                }
            }

            return numbers;
        }

        private static ButtonSetting? ParseButtonsSettings(NewsCommandMessage? message)
        {

            var value = message?.TextValue?.Trim();

            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            var itemElements = value.Split(":").Select(s => s.Trim()).ToArray();
            if (int.TryParse(itemElements[0], out int num))
            {
                string? name = null;
                int? parent = null;
                bool isModer = false;
                if (itemElements.Length > 1)
                {
                    name = itemElements[1];
                }
                if (itemElements.Length > 2)
                {
                    if (int.TryParse(itemElements[2], out int parNum))
                    {
                        parent = parNum;
                    }
                    else if (itemElements[2] == "m")
                    {
                        isModer = true;
                    }
                }
                if (itemElements.Length > 3 && itemElements[3] == "m")
                {
                    isModer = true;
                }
                return new ButtonSetting()
                {
                    Number = num,
                    Name = name,
                    Parent = parent,
                    IsModerate = isModer
                };
            }
            else if (itemElements[0] == "_")
            {
                string? name = null;
                int? parent = null;
                if (itemElements.Length > 1)
                {
                    name = itemElements[1];
                }
                else
                {
                    name = "_";
                }
                if (itemElements.Length > 2 && int.TryParse(itemElements[2], out int parNum))
                {
                    parent = parNum;
                }
                return new ButtonSetting()
                {
                    Name = name,
                    Parent = parent,
                    IsParent = true
                };
            }

            return null;
        }

        private async Task SendUserHasNoRights(long chatId, CancellationToken token)
        {
            await client.SendMessageAsync(chatId, "У вас нет прав на это действие", token);
        }

        private async Task SendUserNotImplemented(long chatId, CancellationToken token)
        {
            await client.SendMessageAsync(chatId, "Действие не реализовано", token);
        }

        private static string? GetButtonsView(List<NewsButton> availableButtons, int? parentId = null, int level = 0)
        {
            var result = availableButtons.Where(s => s.ParentId == parentId);
            if (!result.Any())
                return null;

            return string.Join("\n", result.OrderBy(s => s.ButtonNumber)
                .Select(s => GetGroupView(availableButtons, level, s)));
        }

        private static string GetGroupView(List<NewsButton> availableButtons, int level, NewsButton currentButton)
        {
            string chButtonsView = string.Empty;
            var childButtons = GetButtonsView(availableButtons, currentButton.ButtonNumber, level + 1);
            if (childButtons != null)
            {
                chButtonsView = $"\r\n{GetButtonsView(availableButtons, currentButton.ButtonNumber, level + 1)}";
            }
            return $"{GetTabs(level)}{GetButtonName(currentButton, true)}{chButtonsView}";
        }

        public static string GetTabs(int count)
        {
            var result = "";
            for (int i = 0; i < count; i++)
            {
                result += "\t\t\t\t";
            }
            return result;
        }

        private static string GetButtonName(NewsButton button, bool withSettings)
        {
            var buttonName = button.ButtonName ?? "";
            if (!string.IsNullOrEmpty(button.ButtonName))
            {
                if (!string.IsNullOrEmpty(button.ChatName))
                {
                    if (!string.IsNullOrEmpty(button.ThreadName))
                    {
                        buttonName = $"{buttonName}({button.ChatName}:{button.ThreadName})";
                    }
                    else
                    {
                        buttonName = $"{buttonName}({button.ChatName})";
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(button.ChatName))
                {
                    if (!string.IsNullOrEmpty(button.ThreadName))
                    {
                        buttonName = $"{button.ChatName}:{button.ThreadName}";
                    }
                    else
                    {
                        buttonName = $"{button.ChatName}";
                    }
                }
            }

            if (string.IsNullOrEmpty(buttonName))
            {
                buttonName = "Безымянная кнопка";
            }

            if (withSettings)
            {
                return $"{button.ButtonNumber}. {buttonName}. Подключена: {(button.ToSend ? "Да" : "Нет")}. Родительская: {(button.IsParent ? "Да" : "Нет")}";
            }
            else
            {
                return $"{button.ButtonNumber}. {buttonName}";
            }
        }
                       
        private async Task SendTestConnectionMessage(Message message, string addInfo, CancellationToken token)
        {
            await client.SendMessageAsync(message.Chat.Id, addInfo, token);
        }

        private async Task SendMenuButtons( long chatId, Contract.Model.User user, string type, CancellationToken token)
        {
            if (type == "all")
            {
                if (user.IsAdmin)
                {
                    await client.SendMessageAsync(chatId, "Выберите раздел",
                        replyMarkup: new InlineKeyboardMarkup(GetMenuButtons(user)),  token);
                }
                else
                {
                    await client.SendMessageAsync(chatId, "Панель пользователя",
                        replyMarkup: new InlineKeyboardMarkup(await GetUserButtons(token)),  token);
                }
            }

            if (type == "user")
            {
                await client.SendMessageAsync(chatId, "Панель пользователя",
                         replyMarkup: new InlineKeyboardMarkup(await GetUserButtons(token)),  token);
            }

            if (type == "moderator")
            {
                if (user.IsAdmin)
                {
                    await client.SendMessageAsync(chatId, "Панель модератора",
                        replyMarkup: new InlineKeyboardMarkup(GetModeratorButtons(user)),  token);
                }
                else
                {
                    await client.SendMessageAsync(chatId, "У вас нет доступа к этому разделу",  token);
                }
            }

            if (type == "admin")
            {
                if (user.IsAdmin)
                {
                    await client.SendMessageAsync(chatId, "Панель администратора",
                        replyMarkup: new InlineKeyboardMarkup(GetAdminButtons()),  token);
                }
                else
                {
                    await client.SendMessageAsync(chatId, "У вас нет доступа к этому разделу",  token);
                }
            }
        }

        private async Task<List<List<InlineKeyboardButton>>> GetUserButtons(CancellationToken token)
        {            
            var sendButtons = new List<List<InlineKeyboardButton>>();

            

            

            return sendButtons;
        }

        private static List<InlineKeyboardButton> EmptyButton(string? text = null)
        {
            return [new InlineKeyboardButton(text ?? "* * *")
            {
                CallbackData = "-"
            }];
        }

        private static List<List<InlineKeyboardButton>> GetAdminButtons()
        {
            return
            [
                [
                    new InlineKeyboardButton("Добавить администратора")
                    {
                        CallbackData = "AddAdminChoice"
                    },new InlineKeyboardButton("Добавить модератора")
                    {
                        CallbackData = "AddModeratorChoice"
                    }
                ],
                EmptyButton(),                
            ];
        }

        private static List<List<InlineKeyboardButton>> GetModeratorButtons(Contract.Model.User user)
        {
            return new List<List<InlineKeyboardButton>>();
        }

        private static List<List<InlineKeyboardButton>> GetMenuButtons(Contract.Model.User user)
        {
            List<List<InlineKeyboardButton>> result = [];
            if (user.IsAdmin)
            {
                result.Add([ new InlineKeyboardButton("Панель администратора")
                {
                    CallbackData = "MenuAdmin"
                }]);
            }
            if (user.IsAdmin)
            {
                result.Add([ new InlineKeyboardButton("Панель модератора")
                {
                    CallbackData = "MenuModerator"
                }]);
            }
            result.Add([ new InlineKeyboardButton("Панель пользователя")
                {
                    CallbackData = "MenuUser"
                }]);

            return result;
        }

        private async Task StartCommandHandle( long chatId, Contract.Model.User user, string type, CancellationToken cancellationToken)
        {
            await SendMenuButtons(chatId, user, type, cancellationToken);
        }
    }
}
