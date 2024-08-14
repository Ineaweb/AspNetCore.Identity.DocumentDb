using System;
using System.IO;
using System.Text;

using Azure.Identity;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace AspNetCore.Identity.DocumentDb.Tools
{
    internal static class CosmosClientFactory
    {
        internal static CosmosClient CreateClient(Uri serviceEndpoint, Azure.Core.TokenCredential tokenCredential = null, JsonSerializerSettings serializerSettings = null, ConnectionMode? connectionMode = null, ConsistencyLevel? consistencyLevel = null)
        {
            tokenCredential = tokenCredential ?? new DefaultAzureCredential();

            serializerSettings = serializerSettings ?? new JsonSerializerSettings();
            serializerSettings.Converters.Add(new JsonClaimConverter());
            serializerSettings.Converters.Add(new JsonClaimsPrincipalConverter());
            serializerSettings.Converters.Add(new JsonClaimsIdentityConverter());

#if (NETSTANDARD2 || NETSTANDARD21 || NET46 || NET6_0 || NET8_0)
            //return new DocumentClient(serviceEndpoint, authKeyOrResourceToken, serializerSettings, connectionPolicy, consistencyLevel);
            return new CosmosClientBuilder(accountEndpoint: serviceEndpoint.AbsoluteUri, tokenCredential)
            .WithCustomSerializer(new IdentityCosmosSerializer(serializerSettings))
            .Build();

            //return new CosmosClient(accountEndpoint: serviceEndpoint.AbsoluteUri, tokenCredential, new CosmosClientOptions() { Serializer = new IdentityCosmosSerializer(serializerSettings), ConsistencyLevel = consistencyLevel, ConnectionMode = connectionMode.GetValueOrDefault(ConnectionMode.Direct) });
#else
            // DocumentDB SDK only supports setting the JsonSerializerSettings on versions after NetStandard 2.0
            JsonConvert.DefaultSettings = () => serializerSettings;
            return new DocumentClient(serviceEndpoint, authKeyOrResourceToken, connectionPolicy, consistencyLevel);
#endif
        }
    }
}
