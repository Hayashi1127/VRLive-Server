using System;
using System.Collections.Generic;
using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using StreamingApp.Shared.Services;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using static System.DateTime;
using UnityEngine;

namespace StreamingApp.Services
{
    // Implements RPC service in the server project.
    // The implementation class must inehrit `ServiceBase<IMyService>` and `IMyService`
    public class MyFirstService : ServiceBase<IMyService>, IMyService
    {
        // `UnaryResult<T>` allows the method to be treated as `async` method.
        public async UnaryResult<string> ReturnConnection()
        {
            Console.WriteLine($"uouo");
            return "uouo";
        }
    }

    public class GamingHub : StreamingHubBase<IGamingHub, IGamingHubReceiver>, IGamingHub
    {
        private IGroup _room;
        private Player _self;
        private User _user;
        private IInMemoryStorage<User> _userStorage;
        //private int _num_players; memo:add limit number of people
        private PenLight _penLightL;
        private PenLight _penLightR;

        public async Task<User[]> JoinAsync(string roomName, string userName, UnityEngine.Vector3 position, Quaternion rotation)
        {
            _self = new Player()
            {
                PlayerId = userName,
                Position = position,
                Rotation = rotation,
                Auth = "audience"
            };
            _user = new User()
            {
                Player = _self,
                PenLightL = null,
                PenLightR = null
            };
            
            (this._room, this._userStorage) = await Group.AddAsync(roomName, _user);
            var roomUsers = _userStorage.AllValues.ToArray();
            if (roomUsers.Length == 1)
            {
                _self.Auth = "admin";
                Console.WriteLine("Join admin");
                //return roomUsers;
            }
            //Console.WriteLine("join:" + _self.PlayerId + " auth:" + _self.Auth);
            BroadcastExceptSelf(_room).OnJoin(_self);
            Console.WriteLine("complete broadcast");
            return roomUsers;
        }
        public async Task LeaveAsync()
        {
            if (_user.PenLightL != null)
            {
                Broadcast(_room).OnDeletePenLight(_user.PenLightL);
                _user.PenLightL = null;
            }else if (_user.PenLightR != null)
            {
                Broadcast(_room).OnDeletePenLight(_user.PenLightR);
                _user.PenLightR = null;
            }
            await _room.RemoveAsync(this.Context);
            Broadcast(_room).OnLeave(_self);
        }

        public async Task MoveAsync(UnityEngine.Vector3 position, UnityEngine.Quaternion rotation)
        {
            _self.Position = position;
            _self.Rotation = rotation;
            _user.Player = _self;
            Broadcast(_room).OnMove(_self);
        }

        /*protected override ValueTask Ondisconnected()
        {
            return ValueTask.CompletedTask;
        }*/

        //about PenLight
        public async Task CreatePenLightAsync(PenLight penLight)
        {
            if (penLight.Handle)
            {
                _penLightL = penLight;
                _user.PenLightL = penLight;
            }
            else
            {
                _penLightR = penLight;
                _user.PenLightR = penLight;
            }
            Console.WriteLine("create penlight");
            BroadcastExceptSelf(_room).OnCreatePenLight(penLight);
        }

        public async Task DeletePenLightAsync(PenLight penLight)
        {
            if (penLight.Handle)
            {
                _penLightL = null;
                _user.PenLightL = null;
            }
            else
            {
                _penLightR = null;
                _user.PenLightR = null;
            }
            Broadcast(_room).OnDeletePenLight(penLight);
        }

        public async Task MovePenLightAsync(PenLight penLight)
        {
            if (penLight.Handle)
            {
                _penLightL = penLight;
                _user.PenLightL = penLight;
            }
            else
            {
                _penLightR = penLight;
                _user.PenLightR = penLight;
            }
            Console.WriteLine("penlight async");
            Broadcast(_room).OnMovePenLight(penLight);
        }

        public async Task PenLightStatusAsync(bool color, bool trail)
        {
            _penLightL.Trail = trail;
            _penLightR.Trail = trail;
            _user.PenLightL = _penLightL;
            _user.PenLightR = _penLightR;
            Broadcast(_room).OnPenLightStatus(_self.PlayerId, color, trail);
        }

        public async Task LiveStartAsync()
        {
            float timeS = DateTime.UtcNow.Second + 2f;
            float timeM = DateTime.UtcNow.Minute;
            if (timeS >= 60)
            {
                timeM += 1;
                timeS -= 60;
            }

            if (timeM >= 60)
            {
                timeM -= 60;
            }

            Console.WriteLine("live start");
            Broadcast(_room).OnLiveStart(timeM, timeS);
        }

        public async Task StageScoreAsync(float score)
        {
            Console.WriteLine("Status Async");
            Broadcast(_room).OnStageScore(score);
        }
    }
}