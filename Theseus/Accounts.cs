//
//  File: Accounts.cs
//  Created: 28.9.2015
//  Author: Yan Zaitsev <yan.zaitsev@gmail.com>
//
//  Copyright (c) 2015 @YZaitsev
//
using System;
using Api;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading;
using NLog;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace Theseus {
    public class Accounts : IAccounts {
        private readonly String DBFileName;
        private CancellationToken CancellationToken;

        private Logger Logger { get; set; }

        public Accounts(String dbFileName, CancellationToken cancellationToken) {
            DBFileName = dbFileName;
            CancellationToken = cancellationToken;
            Logger = LogManager.GetLogger("Accounts");
        }

        private async Task<String> ReadDB(){
            try {
                using (FileStream sourceStream = new FileStream(DBFileName,
                                                     FileMode.Open, FileAccess.Read, FileShare.Read,
                                                     bufferSize: 4096, useAsync: true)) {
                    byte[] buffer = new byte[sourceStream.Length];
                    int bytes = await sourceStream.ReadAsync(buffer, 0, buffer.Length, CancellationToken);
                    return Encoding.UTF8.GetString(buffer, 0, bytes);

                }
            }
            catch (IOException e) {
                Logger.Error(e);
            }
            return null;
        }

        private async Task<Account[]> GetAccounts(){
            var dbRaw = await ReadDB();
            if (dbRaw == null)
                return null;
            try {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                settings.ContractResolver = new PrivateJsonDefaultContractResolver();
                return JsonConvert.DeserializeObject<Account[]>(dbRaw, settings);
            }
            catch (JsonException e) {
                Logger.Error(e);
            }
            return null;
        }

        #region IAccounts implementation

        public async Task<bool> ExistsAccount(string username, string password){
            return (await GetAccount(username, password)) != null;
        }

        public async Task<Account> GetAccount(string username, string password){
            Account[] accounts = await GetAccounts();


            if (accounts == null)
                return null;

            foreach (var account in accounts) {
                if (account.CanAuthWith(username, password))
                    return account;
            }
            return null;
        }

        #endregion

        public class PrivateJsonDefaultContractResolver : DefaultContractResolver {
            protected override JsonProperty CreateProperty(
                MemberInfo member,
                MemberSerialization memberSerialization){
                var prop = base.CreateProperty(member, memberSerialization);

                if (!prop.Writable) {
                    var property = member as PropertyInfo;
                    if (property != null) {
                        var hasPrivateSetter = property.GetSetMethod(true) != null;
                        prop.Writable = hasPrivateSetter;
                    }
                }

                return prop;
            }

            protected override List<MemberInfo> GetSerializableMembers(Type objectType){
                var members = base.GetSerializableMembers(objectType);
                if (objectType == typeof(Account)) {
                    members.AddRange(objectType.GetMember("Password", 
                            BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance
                        ));
                }
                return members;
            }
        }

    }
}

