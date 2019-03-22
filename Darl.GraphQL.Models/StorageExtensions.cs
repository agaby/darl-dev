using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Connectivity
{
    public static class StorageExtensions
    {
        public static async Task<List<IListBlobItem>> ListBlobsAsync(this CloudBlobContainer blobContainer, string user = "")
        {
            var list = new List<IListBlobItem>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment resultSegment = !string.IsNullOrEmpty(user) ? await blobContainer.ListBlobsSegmentedAsync(user, token) : await blobContainer.ListBlobsSegmentedAsync(token);
                token = resultSegment.ContinuationToken;
                list.AddRange(resultSegment.Results);
            } while (token != null);

            return list;
        }

        public static async Task<List<IListBlobItem>> ListBlobsAsync(this CloudBlobDirectory blobContainer)
        {
            var list = new List<IListBlobItem>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment resultSegment = await blobContainer.ListBlobsSegmentedAsync(token);
                token = resultSegment.ContinuationToken;
                list.AddRange(resultSegment.Results);
            } while (token != null);

            return list;
        }

        public async static Task<T> Get<T>(this CloudTable table, string partitionKey, string rowKey) where T : ITableEntity, new()
        {
            //new T();
            TableQuery<T> query = new TableQuery<T>().Where(
                TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey))).Take(1);
            var re = new T();
            TableContinuationToken continuationToken = null;
            do
            {
                var val = await table.ExecuteQuerySegmentedAsync<T>(query, continuationToken);
                re = val.FirstOrDefault();
                continuationToken = val.ContinuationToken;
            } while (continuationToken != null);
            return re;
        }

        /// <summary>
        /// Partition keys in azure storage tables can't have some characters. This removes them.
        /// </summary>
        /// <param name="str">raw string</param>
        /// <returns>trimmed string that should still be unique</returns>
        public static string ToAzureKeyString(this string str)
        {
            var sb = new StringBuilder();
            foreach (var c in str
                .Where(c => c != '/'
                            && c != '\\'
                            && c != '#'
                            && c != '/'
                            && c != '?'
                            && !char.IsControl(c)))
                sb.Append(c);
            return sb.ToString();
        }
    }
}
