using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ROTGBot.Contract.Model;
using System.Linq.Dynamic.Core.Tokenizer;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ROTGBot.Service
{
    public class TelegramMessageHandler : ITelegramMessageHandler
    {
        private const string HelloMessage = "Привет, {0}! Для работы нажмите кнопку меню - Старт или введите /start";

        private readonly ILogger<TelegramMessageHandler> _logger;
                
        private readonly IUserDataService _userDataService;
        private readonly ICommandDataService _commandDataService;
        private readonly ITelegramBotWrapper client;

        private readonly int TimeoutSpan = 10;

        private readonly Dictionary<CommandType, RoleEnum[]> commandRoles = new Dictionary<CommandType, RoleEnum[]>()
        {
            { CommandType.AddAdministrator, new RoleEnum[]{ RoleEnum.administrator } },
            { CommandType.AddAdministratorResponse, new RoleEnum[]{ RoleEnum.administrator } },
            { CommandType.AddCommander, new RoleEnum[]{ RoleEnum.administrator } },
            { CommandType.AddCommanderResponse, new RoleEnum[]{ RoleEnum.administrator, RoleEnum.city_commander } },
            { CommandType.AddDistrictCommander, new RoleEnum[]{ RoleEnum.administrator, RoleEnum.city_commander } },
            { CommandType.AddDistrictCommanderResponse, new RoleEnum[]{ RoleEnum.administrator, RoleEnum.city_commander } },
            { CommandType.AddSquaddie, new RoleEnum[]{ RoleEnum.administrator, RoleEnum.city_commander, RoleEnum.district_commander } },
            { CommandType.AddSquaddieResponse, new RoleEnum[]{ RoleEnum.administrator, RoleEnum.city_commander, RoleEnum.district_commander } },            
            { CommandType.ViewDemands, new RoleEnum[]{ RoleEnum.administrator, RoleEnum.city_commander, RoleEnum.district_commander } },           
            { CommandType.ViewDemandsResponse, new RoleEnum[]{ RoleEnum.administrator, RoleEnum.city_commander, RoleEnum.district_commander } },
            { CommandType.ViewUserRights, new RoleEnum[]{ RoleEnum.administrator, RoleEnum.city_commander, RoleEnum.district_commander } },
            { CommandType.DeclineCurrentTask, new RoleEnum[]{ RoleEnum.administrator, RoleEnum.city_commander, RoleEnum.district_commander, RoleEnum.squaddie } },
            { CommandType.ViewUserRightsResponse, new RoleEnum[]{ RoleEnum.administrator, RoleEnum.city_commander, RoleEnum.district_commander } },
            { CommandType.AddUserRights, new RoleEnum[]{ RoleEnum.administrator, RoleEnum.city_commander, RoleEnum.district_commander } },
            { CommandType.DeleteUserRights, new RoleEnum[]{ RoleEnum.administrator, RoleEnum.city_commander, RoleEnum.district_commander } },
            { CommandType.AddUserRightsResponse, new RoleEnum[]{ RoleEnum.administrator, RoleEnum.city_commander, RoleEnum.district_commander } },
            { CommandType.DeleteUserRightsResponse, new RoleEnum[]{ RoleEnum.administrator, RoleEnum.city_commander, RoleEnum.district_commander } },
        };

        private readonly List<CommandType> initialCommands =
            [                
                CommandType.AddSquaddie, 
                CommandType.ViewUserRights, 
                CommandType.AddUserRights,
                CommandType.DeleteUserRights,
                CommandType.AddCommander
            ];

        private readonly List<CommandType> singleCommands =
            [
                CommandType.ViewDemands
            ];

        public TelegramMessageHandler(
            ILogger<TelegramMessageHandler> logger,
            IUserDataService userDataService,
            IConfiguration configuration,
            ITelegramBotWrapper wrapper,
            ICommandDataService commandDataService)
        {
            _logger = logger;
            _userDataService = userDataService;
            var botSettings = configuration.GetSection("BotSettings").Get<BotSettings>();
            client = wrapper;
            _commandDataService = commandDataService;
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
            var user = await _userDataService.GetUserByTGId(tgUser.Id, cancellationToken);
            //var user = await _userDataService.GetOrAddUser(tgUser.Id, tgUser.Username, $"{tgUser.FirstName} {tgUser.LastName} (@{tgUser.Username})", message.Chat.Id, cancellationToken);

            if (user == null)
            {
                user = await _userDataService.AddOrUpdateUser(tgUser.Id, tgUser.Username ?? "Аноним", $"{tgUser.FirstName} {tgUser.LastName} (@{tgUser.Username})", message.Chat.Id, cancellationToken);
            }

            if (user == null)
            {
                return;
            }

            if (message.Text == "/start")
            {
                await StartCommandHandle(user.ChatId, user, "all", cancellationToken);
            }            
            else if (message.IsTopicMessage != true)
            {
                await SendTestConnectionMessage(message, string.Format(HelloMessage, user.Name), cancellationToken);
            }
            else
            {
                var result = await HandleData(user.ChatId, user, null, message.Text, cancellationToken);
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
                user = await _userDataService.AddOrUpdateUser(tgUser.Id, tgUser.Username, $"{tgUser.FirstName} {tgUser.LastName} (@{tgUser.Username})", null, token);
            }
            else
            {
                user = await _userDataService.AddOrUpdateUser(tgUser.Id, tgUser.Username, $"{tgUser.FirstName} {tgUser.LastName} (@{tgUser.Username})", chatId.Value, token);
            }
            if (user == null)
            {
                return false;
            }
            var data = callbackQuery.Data;
            if (data == null) return false;
            var result = await HandleData(user.ChatId, user, data, "", token);
            await client.AnswerCallbackQueryAsync(new AnswerCallbackQueryArgs(callbackQuery.Id), token: token);
            return result;
        }

        private async Task<bool> HandleData(
            long chatId,
            Contract.Model.User user,
            string? dataReq,
            string data,
            CancellationToken token)
        {
            var roles = user.Roles;
            var userId = user.Id;
            Command? command = null;

            if (dataReq == null || dataReq == "-")
            {
                command = await _commandDataService.GetCurrentCommand(user, token);
                if(command == null)
                {
                    return false;
                }
                dataReq = Enum.GetName((CommandType)command.CommandType) + "Response";
            }           

            if(!Enum.TryParse(dataReq, out CommandType commandType) || !await CheckRights(user, chatId, commandType, token))
            {
                return false;
            }

            if(initialCommands.Contains(commandType))
            {
                command = await _commandDataService.AddCommand(user, commandType, token);
            }

            string[] args = [];

            if (command == null)
            {
                if(!singleCommands.Contains(commandType))
                {
                    return false;
                }
            }
            else
            {
                args = await _commandDataService.GetMessages(command.Id);
                args = [.. (args ?? []), data];
            }

                switch (commandType)
                {
                    ///ViewDemands
                    case CommandType.ViewDemands:
                        await ViewDemandsSendRequest(chatId, token);
                        break;                    
                    ///AddSquaddie
                    case CommandType.AddSquaddie:
                        await AddSquaddieSendRequest(chatId, token);
                        break;
                    case CommandType.AddSquaddieResponse:
                        await AddSquaddieHandleResponse(chatId, args, userId, token);
                        break;
                    ///ViewUserRights
                    case CommandType.ViewUserRights:
                        await ViewUserRightsSendRequest(chatId, token);
                        break;
                    case CommandType.ViewUserRightsResponse:
                        await ViewUserRightsHandleResponse(chatId, args, userId, token);
                        break;
                    ///AddUserRights
                    case CommandType.AddUserRights:
                        await AddUserRightsSendRequest(chatId, token);
                        break;
                    case CommandType.AddUserRightsResponse:
                        await AddUserRightsHandleResponse(chatId, args, token);
                        break;
                    ///DeleteUserRights
                    case CommandType.DeleteUserRights:
                        await DeleteUserRightsSendRequest(chatId, token);
                        break;
                    case CommandType.DeleteUserRightsResponse:
                        await DeleteUserRightsHandleResponse(chatId, args, token);
                        break;
                    ///DeclineCurrentTask
                    case CommandType.DeclineCurrentTask:
                        await DeclineCurrentTask(chatId, token);
                        break;
                    ///SendUserNotImplemented
                    default:
                        await SendUserNotImplemented(chatId, token);
                        break;
                }
            return true;
        }

        private async Task DeleteUserRightsHandleResponse(long chatId, string[] args, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private async Task DeleteUserRightsSendRequest(long chatId, CancellationToken token)
        {
            await client.SendMessageAsync(chatId, "Отправьте через запятую или точку с запятой номер дружинника и роль, которые ему надо удалить из списка: " +
                "administrator (Администратор), district_commander (Командир уровня района), city_commander (Командир городского уровня)", GetDeclineReplyMarkUp(), token);
        }

        private async Task DeclineCurrentTask(long chatId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        

        private async Task ViewUserRightsHandleResponse(long chatId, string[] args, Guid userId, CancellationToken token)
        {
            var data = args.Where(s => !string.IsNullOrEmpty(s));
            if(!data.Any())
            {
                await client.SendMessageAsync(chatId, "Не отправлено ни одного логина или номера. Отправьте номер или логин пользователя для просмотра его прав, либо Отмена для отмены действия", GetDeclineReplyMarkUp(), token);
                return;
            }
            foreach (var arg in args)
            {
                var user = await _userDataService.GetUserByNumberOrLogin(arg, token);
                if(user == null)
                {
                    await client.SendMessageAsync(chatId, $"Пользователь {arg} не найден.", token);
                    continue;
                }
                await client.SendMessageAsync(chatId, $"Пользователь {user.Number} : {user.Name} ({user.TGLogin}), права: {string.Join(", ", user.Roles.Select(s => Enum.GetName(s)))}.", token);
            }
            await _commandDataService.CloseCurrentCommand(userId, token);
        }

        private async Task ViewUserRightsSendRequest(long chatId, CancellationToken token)
        {
            await client.SendMessageAsync(chatId, "Отправьте номер или логин пользователя для просмотра его прав, либо Отмена для отмены действия", GetDeclineReplyMarkUp(), token);
        }        

        private async Task ViewDemandsSendRequest(long chatId, CancellationToken token)
        {
            var demands = await _userDataService.GetDemandUsers(token);
            await client.SendMessageAsync(chatId, $"Кандидаты на добавление в дружину:\r\n{string.Join("\r\n", demands.Select(s => $"{s.Number}. {s.Name} ({s.TGLogin})"))}", token);
        }

        private async Task AddSquaddieDecline(long chatId, string[] args, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private async Task AddSquaddieHandleResponse(long chatId, string[] args, Guid userId, CancellationToken token)
        {
            var users = await _userDataService.GetDemandUsers(token);
            string response = string.Empty;
            foreach(var arg in args)
            {
                var numbers = arg.Split(',', ';').Select(s => s.Trim());
                foreach(var number in numbers)
                {
                    if(int.TryParse(number, out int intNumber))
                    {
                        var user = users.FirstOrDefault(s => s.Number == intNumber);
                        if(user == null)
                        {
                            response += $"Не удалось добавить пользователя - не найдена заявка на добавление {number}";
                        }
                        else
                        {
                            await _userDataService.SetRole(user.Id, RoleEnum.squaddie, token);
                        }
                    }
                    else
                    {
                        response += $"Не удалось добавить пользователя - некорректный номер {number}";
                    }
                }
            }
            if(response == string.Empty)
            {
                response += "Пользователи успешно добавлены в дружину";
            }
            await client.SendMessageAsync(chatId, response, token);
            await _commandDataService.CloseCurrentCommand(userId, token);
        }

        private async Task AddSquaddieSendRequest(long chatId, CancellationToken token)
        {
            await client.SendMessageAsync(chatId, "Отправьте через запятую или точку с запятой номера кандидатов для добавления в дружину", GetDeclineReplyMarkUp(), token);
        }

        private async Task AddUserRightsSendRequest(long chatId, CancellationToken token)
        {
            await client.SendMessageAsync(chatId, "Отправьте через запятую или точку с запятой номер дружинника и права, которые ему надо добавить из списка: " +
                "administrator (Администратор), district_commander (Командир уровня района), city_commander (Командир городского уровня) для добавления прав", GetDeclineReplyMarkUp(), token);
        }

        private async Task AddUserRightsHandleResponse(long chatId, string[] args, CancellationToken token)
        {
            var allRoles = Enum.GetNames<RoleEnum>();
            var allArgs = args.Select(s => s.Split(',', ';')).SelectMany(s => s).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

            if(allArgs.Count == 0)
            {
                await client.SendMessageAsync(chatId, "Отправьте через запятую или точку с запятой номер дружинника и права, которые ему надо добавить из списка: " +
                    "administrator (Администратор), district_commander (Командир уровня района), city_commander (Командир городского уровня) для добавления прав", GetDeclineReplyMarkUp(), token);
                return;
            }

            var userNumber = allArgs[0];
            var user = await _userDataService.GetUserByNumberOrLogin(userNumber, token);
            if(user == null)
            {
                await client.SendMessageAsync(chatId, $"Пользователь {userNumber} не найден, задание отменено", GetDeclineReplyMarkUp(), token);
                return;
            }

            var toAddRoles = allArgs.Where(s => allRoles.Contains(s, StringComparer.InvariantCultureIgnoreCase));
            if(!toAddRoles.Any())
            {
                await client.SendMessageAsync(chatId, $"Не отправлено ни одной роли, задание отменено", GetDeclineReplyMarkUp(), token);
                return;
            }

            var userRolesNames = user.Roles.Select(s => Enum.GetName(s));
            toAddRoles = toAddRoles.Where(s => !userRolesNames.Contains(s));

            if (!toAddRoles.Any())
            {
                await client.SendMessageAsync(chatId, $"Указанные роли уже присвоены пользователю, задание отменено", GetDeclineReplyMarkUp(), token);
                return;
            }

            foreach(var role in toAddRoles)
            {
                await _userDataService.SetRole(user.Id, Enum.Parse<RoleEnum>(role), token);
            }
            await client.SendMessageAsync(chatId, $"Роли успешно присвоены пользователю, задание отменено", GetDeclineReplyMarkUp(), token);
        }

        private static InlineKeyboardMarkup GetDeclineReplyMarkUp()
        {           
            return new InlineKeyboardMarkup(
                new List<List<InlineKeyboardButton>>()
                {
                    new()
                    {
                        GetDeclineButton()
                    }
                });
        }

        private static InlineKeyboardButton GetDeclineButton()
        {
            return new InlineKeyboardButton("Отменить")
            {
                CallbackData = "DeclineCurrentTask"
            };
        }

        private async Task<bool> CheckRights(
            Contract.Model.User user,
            long chatId,
            CommandType commandType,           
            CancellationToken token)
        {
            if(!commandRoles.TryGetValue(commandType, out var enableRoles))
            {
                _logger.LogError($"Не заданы права для команды '{Enum.GetName(commandType)}'");
                await SendUserHasNoRights(chatId, token);
                return false;
            }

            if (!user.Roles.Any(s => enableRoles.Contains(s)))
            {
                await SendUserHasNoRights(chatId, token);
                return false;
            }
            return true;
        }
                
        private async Task SendForwardMessageTitle( News userNews, CancellationToken token)
        {
            var user = await _userDataService.GetUser(userNews.UserId, token);
            var tgLogin = !string.IsNullOrEmpty(user.TGLogin) ? $"@{user.TGLogin}" : "Не определен";
            var userName = user.Name ?? "Не определен";
            await client.SendMessageAsync(userNews.GroupId.Value, $"Обращение №{userNews.Number} в раздел \"{userNews.Title}\" от пользователя {userName} (логин: {tgLogin})", (int?)userNews.ThreadId,  token);
        }
                
        private static List<ButtonSetting> ParseButtonsSettings(IEnumerable<NewsMessage> messages)
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

        private static ButtonSetting? ParseButtonsSettings(NewsMessage? message)
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

            if (type == "squaddie")
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
