using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LitJson;
using MobaCommon.Code;
using MobaCommon.Dto;
using MOBAServer.Cache;
using Photon.SocketServer;

namespace MOBAServer.Logic
{
    /// <summary>
    /// 账号逻辑处理
    /// </summary>
    public class AccountHandler : SingleSend, IOpHandler
    {
        /// <summary>
        /// 账号缓存
        /// </summary>
        private AccountCache cache = Caches.Account;
        public void OnDisconnect(MobaClient client)
        {
            cache.OffLine(client);
        }

        public void OnRequest(MobaClient client, byte subCode, OperationRequest request)
        {
            switch (subCode)
            {
                case OpAccount.Login:
                    AccountDto dto = JsonMapper.ToObject<AccountDto>(request[0].ToString());
                    OnLogin(client, dto.Account, dto.Password);

                    break;
                case OpAccount.Register:
                    string acc = request[0].ToString();
                    string pwd = request[1].ToString();
                    OnRegister(client, acc, pwd);

                    break;
                default:
                    break;
            }
        }

        #region 子处理
        /// <summary>
        /// 登录处理
        /// </summary>
        /// <param name="acc"></param>
        /// <param name="pwd"></param>
        private void OnLogin(MobaClient client, string acc, string pwd)
        {
            //无效检查
            if (acc == null || pwd == null)
            {
                return;
            }

            if (cache.IsOnline(acc))
            {
                this.Send(client, OpCode.AccountCode, OpAccount.Login, -1, "玩家在线");
                return;
            }
            //验证账号密码是否合法
            bool res = cache.Match(acc, pwd);
            if (res == true)
            {
                cache.OnLine(acc, client);
                this.Send(client, OpCode.AccountCode, OpAccount.Login, 0, "登录成功");
            }
            else
            {
                this.Send(client, OpCode.AccountCode, OpAccount.Login, -2, "账号密码错误");
            }
        }
        /// <summary>
        /// 注册处理
        /// </summary>
        /// <param name="acc"></param>
        /// <param name="pwd"></param>
        private void OnRegister(MobaClient client, string acc, string pwd)
        {
            //无效检查
            if (acc == null || pwd == null)
            {
                return;
            }

            if (cache.Has(acc))
            {
                this.Send(client, OpCode.AccountCode, OpAccount.Register, -1, "账号重复");
                return;
            }

            //添加账号
            cache.Add(acc, pwd);
            this.Send(client, OpCode.AccountCode, OpAccount.Register, 0, "注册成功");

        }
        #endregion
    }
}
