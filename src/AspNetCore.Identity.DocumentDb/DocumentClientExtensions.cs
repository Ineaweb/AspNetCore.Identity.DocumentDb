using System;
using System.Collections.Generic;
using System.Text;
using AspNetCore.Identity.DocumentDb.Tools;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace AspNetCore.Identity.DocumentDb
{
    public static class CosmosClientExtensions
    {
        public static CosmosClient AddDefaultDocumentClientForIdentity(this IServiceCollection services, Uri serviceEndpoint, Azure.Core.TokenCredential token, JsonSerializerSettings serializerSettings = null, ConnectionMode? connectionMode = null, ConsistencyLevel? consistencyLevel = null, Action<CosmosClient> afterCreation = null)
        {
            var cosmosClient = CosmosClientFactory.CreateClient(serviceEndpoint, token, serializerSettings, connectionMode, consistencyLevel);
            afterCreation?.Invoke(cosmosClient);
            services.AddSingleton<CosmosClient>(cosmosClient);
            return cosmosClient;
        }
    }
}
