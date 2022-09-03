using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Identity.DocumentDb.Stores
{
    public abstract class StoreBase
    {
        protected bool disposed = false;

        protected CosmosClient documentClient;
        protected DocumentDbOptions options;
        protected Uri collectionUri;
        protected string collectionName;

        protected StoreBase(CosmosClient documentClient, IOptions<DocumentDbOptions> options, string collectionName)
        {
            this.documentClient = documentClient;
            this.options = options.Value;
            this.collectionName = collectionName;
        }

        protected virtual void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
