using System;
using WalletsCrypto.ReadModel.Common;

namespace WalletsCrypto.ReadModel.User
{
    public class User : IReadEntity
    {
        public string Id 
        {
            get;
            set;
        }
    }
}