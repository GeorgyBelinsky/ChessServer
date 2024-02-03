using ChessServer.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChessServer.Hubs
{
    public class ChessRoomHub : Hub
    {
        private readonly string _botUser;
        private readonly IDictionary<string, UserConnection> _connections;
        //private string last_user_key = "";

        public ChessRoomHub(IDictionary<string, UserConnection> connections)
        {
            _botUser = "ChessBot";
            _connections = connections;
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                _connections.Remove(Context.ConnectionId);
                Clients.Group(userConnection.RoomCode).SendAsync("RecieveMessage", _botUser, $"{userConnection.UserName} has left.");

                SendConnectedUsers(userConnection.RoomCode);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                await Clients.Group(userConnection.RoomCode).SendAsync("RecieveMessage", userConnection.UserName, message);
            }
        }

        public async Task SynchronyzeFigures(string position)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                await Clients.Group(userConnection.RoomCode).SendAsync("GetSynchronyzedFigures", position);
            }
        }

        //public async Task SynchronyzeWithLastUser()
        //{
        //    if (last_user_key != "")
        //        await Clients.Client(last_user_key).SendAsync("SynchronyzeWithLastUser");
        //}

        public async Task SynchronizeGameHistory(List<ChessMoveModel> gameHistory)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                await Clients.Group(userConnection.RoomCode).SendAsync("GetGameHistory", gameHistory);
            }
        }

        public async Task JoinRoom(UserConnection userConnection)
        {
            if (IsUserNameUnique(userConnection.UserName, userConnection.RoomCode))
            {
                userConnection.UserRole = GetUserRole(userConnection.RoomCode, userConnection.UserRole);
                await Clients.Caller.SendAsync("GetUserRole", userConnection.UserRole);

                //if (_connections.Any())
                //    last_user_key = _connections.Last().Key;

                await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.RoomCode);
                _connections[Context.ConnectionId] = userConnection;

                await Clients.Group(userConnection.RoomCode).SendAsync("RecieveMessage", _botUser,
                    $"{userConnection.UserName}" +
                    $" has joined {userConnection.RoomCode}" +
                    $" with role {userConnection.UserRole}");

                await SendConnectedUsers(userConnection.RoomCode);
                //await SynchronyzeWithLastUser();
            }
            else
            {
                await Clients.Caller.SendAsync("OperationFailed", "Operation failed", "User with this name already exists");
            }
        }

        public Task SendConnectedUsers(string room)
        {
            var users = _connections.Values.Where(c => c.RoomCode == room).Select(c => new { c.UserName, c.UserRole });
            return Clients.Group(room).SendAsync("UsersInRoom", users);
        }

        public bool IsUserNameUnique(string userName, string roomCode)
        {
            var users = _connections.Values.Where(c => c.RoomCode == roomCode).Select(c => c.UserName);
            return !users.Contains(userName);
        }

        public string GetUserRole(string roomCode, string userRole)
        {
            string result = userRole;
            var users = _connections.Values.Where(c => c.RoomCode == roomCode).Select(c => c.UserRole);
            if (users.Contains("white") && users.Contains("black"))
            {
                result = "spectator";
            }
            else
            if (users.Contains("white"))
            {
                result = "black";
            }
            else
            if (users.Contains("black"))
            {
                result = "white";
            }

            return result;
        }
    }
}
