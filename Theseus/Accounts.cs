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
    /// <summary>
    /// Implementation of accounts subsystem. It is based on small json db.
    /// </summary>
    public class Accounts : IAccounts {
        /// <summary>
        /// The name of the DB file.
        /// </summary>
        private readonly String DBFileName;

        /// <summary>
        /// The cancellation token.
        /// </summary>
        private CancellationToken CancellationToken;

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private Logger Logger { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Theseus.Accounts"/> subsystem.
        /// </summary>
        /// <param name="dbFileName">Db file name.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public Accounts(String dbFileName, CancellationToken cancellationToken) {
            DBFileName = dbFileName;
            CancellationToken = cancellationToken;
            Logger = LogManager.GetLogger("Accounts");
        }

        /// <summary>
        /// Reads DB file
        /// </summary>
        /// <returns>File content as UTF8 string.</returns>
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

        /// <summary>
        /// Gets the accounts.
        /// </summary>
        /// <returns>The accounts.</returns>
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

        /// <summary>
        /// Checks does account exist.
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="username">Account ID.</param>
        /// <param name="password">Account password.</param>
        public async Task<bool> ExistsAccount(string username, string password){
            return (await GetAccount(username, password)) != null;
        }

        /// <summary>
        /// Gets the account.
        /// </summary>
        /// <returns>The account.</returns>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
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

        /// <summary>
        /// Private json contract resolver.
        /// It is used to deserialize records into <see cref="Api.Account"/> instances, which use private properties and private setters.
        /// </summary>
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

